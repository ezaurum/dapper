using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using slf4net;

namespace Ezaurum.Dapper
{
    public static class AutoQueryMaker
    {
        private static readonly ILogger Logger = LoggerFactory.GetLogger(typeof(AutoQueryMaker));

        private static readonly Type[] SingleColumnTypes =
        {
            typeof (string),
            typeof (DateTime),
            typeof (byte[])
        }; 

        private static void AppendPropertyColumns(PropertyInfo property, StringBuilder stringBuilder, string format, string delimeter = SqlQuerySnippet.Comma)
        {
            var keyType = property.PropertyType;
            if (keyType.IsPrimitive || keyType.IsEnum || SingleColumnTypes.Contains(keyType))
            {
                AppendSingleColumn(property, stringBuilder, format, delimeter);
            }
            else if (keyType.IsValueType)
            {
                foreach (var fieldInfo in keyType.GetFields().Where(p => p.IsPublic))
                {
                    AppendSingleColumn(fieldInfo, stringBuilder, format, delimeter);
                }
            }
            else
            {
                foreach (var propertyInfo in keyType.GetProperties())
                {
                    AppendSingleColumn(propertyInfo, stringBuilder, format, delimeter);
                }
            }
        }

        /// <summary>
        /// Append Single Column to StringBuilder by format
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="stringBuilder"></param>
        /// <param name="format"></param>
        /// <param name="delimeter"></param>
        /// <param name="scalarName"></param>
        private static void AppendSingleColumn(MemberInfo memberInfo,
            StringBuilder stringBuilder, string format, string delimeter, string scalarName = null)
        {
            var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (null == columnAttribute) return;
            if (stringBuilder.Length > 1) stringBuilder.Append(delimeter);
            
            if (null == scalarName)
            {
                scalarName = memberInfo.Name;
            }

            var name = string.IsNullOrWhiteSpace(columnAttribute.Name)
                    ? memberInfo.Name
                    : columnAttribute.Name;

            stringBuilder.AppendFormat(format, name, scalarName);
        }

        public static void GenerateQueries(Type type, string preparedTableName, string prefix, string suffix, out string tableName,
            out string insertQuery, out string selectQuery, out string selectByIDQuery, out string deleteByIDQuery, out string updateByIDQuery)
        {
            
            if (null != preparedTableName)
            {
                tableName = preparedTableName;
            }
            else
            {
                TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();
                bool hasTable = null != tableAttribute;
                if (hasTable && null != tableAttribute.Name)
                {
                    tableName = tableAttribute.Name;
                }
                else
                {
                    tableName = prefix + type.Name + suffix;
                    Logger.Info("GetTableName:" + tableName);
                }
            }

            var primaryKey = new List<PropertyInfo>();
            var insertColumnsBuilder = new StringBuilder();
            var updateValueStringBuilder = new StringBuilder();
            var insertValuesBuilder = new StringBuilder();

            foreach (var property in type.GetProperties())
            {
                var columnAttribute
                    = property.GetCustomAttribute<ColumnAttribute>();
                if (null == columnAttribute) continue;
                if (columnAttribute.IsDbGenerated) continue;

                if (columnAttribute.IsPrimaryKey)
                {
                    primaryKey.Add(property);
                }
                else
                {
                    AppendPropertyColumns(property, updateValueStringBuilder,
                        SqlQuerySnippet.ValueMatchFormat);
                }

                AppendPropertyColumns(property, insertColumnsBuilder, "{0}");
                AppendPropertyColumns(property, insertValuesBuilder, "@{1}");
            }

            //generate primary key snippet
            if (primaryKey.Count < 1)
                throw new InvalidOperationException("no primary key in " + type.Name);

            var columnSnippet = insertColumnsBuilder.ToString();
            var valuesSnippet = insertValuesBuilder.ToString();

            var columnsSetValues = updateValueStringBuilder.ToString();

            //generate key string
            var keyStringBuilder = new StringBuilder();
            foreach (var keyInfo in primaryKey)
            {
                AppendPropertyColumns(keyInfo, keyStringBuilder,
                    SqlQuerySnippet.ValueMatchFormat,
                    SqlQuerySnippet.AndSnippet);
            }
            var primaryKeySnippet = keyStringBuilder.ToString();

            insertQuery = string.Format(SqlQuerySnippet.InsertFormat, tableName, columnSnippet, valuesSnippet);
            selectQuery = SqlQuerySnippet.SelectAllSnippet + tableName;
            var replace = new Regex("@([^ ]*)").Replace(primaryKeySnippet, "@PK_ID");
            selectByIDQuery = string.Format(SqlQuerySnippet.SelectFormat, tableName, replace);
            deleteByIDQuery = string.Format(SqlQuerySnippet.DeleteFormat, tableName, primaryKeySnippet);
            updateByIDQuery = string.Format(SqlQuerySnippet.UpdateFormat, tableName, columnsSetValues, primaryKeySnippet);
        }
    }
}