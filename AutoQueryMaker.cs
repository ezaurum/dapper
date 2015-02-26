using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ezaurum.Dapper
{
    public static class AutoQueryMaker
    {
        public static string GetTableName(string tableName, 
            string prefix, string suffix, Type type)
        {
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            var hasTable = null != tableAttribute;
            if (null != tableName)
            {
                return tableName;
            }

            if (hasTable && null != tableAttribute.Name)
            {
                return tableAttribute.Name;
            }

            return prefix + type.Name + suffix;
        }

        public static void GenerateSnippets(Type type, out string columnSnippet,
            out string valuesSnippet,
            out string updateSnippet,
            out string primaryKeySnippet)
        {
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

            columnSnippet = insertColumnsBuilder.ToString();
            valuesSnippet = insertValuesBuilder.ToString();

            updateSnippet = updateValueStringBuilder.ToString();

            //generate key string
            var keyStringBuilder = new StringBuilder();
            foreach (var keyInfo in primaryKey)
            {
                AppendPropertyColumns(keyInfo, keyStringBuilder,
                    SqlQuerySnippet.ValueMatchFormat,
                    SqlQuerySnippet.AndSnippet);
            }
            primaryKeySnippet = keyStringBuilder.ToString();
        }

        private static void AppendPropertyColumns(PropertyInfo property, 
            StringBuilder stringBuilder, string format, 
            string delimeter = SqlQuerySnippet.Comma)
        {
            var keyType = property.PropertyType;
            if (keyType.IsPrimitive || keyType == typeof(string) || keyType == typeof(DateTime)|| keyType.IsEnum)
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

        private static void AppendSingleColumn(MemberInfo memberInfo,
            StringBuilder stringBuilder, string format, string delimeter)
        {
            var columnAttribute = memberInfo.GetCustomAttribute<ColumnAttribute>();
            if (null == columnAttribute) return;
            if (stringBuilder.Length > 1) stringBuilder.Append(delimeter);
            var name = string.IsNullOrWhiteSpace(columnAttribute.Name)
                ? memberInfo.Name
                : columnAttribute.Name;
            stringBuilder.AppendFormat(format, name, memberInfo.Name);
        }
    }
}