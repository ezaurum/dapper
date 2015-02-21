using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ezaurum.Dapper
{
    public abstract class AutoQueryMaker<T>
    {
        protected AutoQueryMaker(string tableName = null, string prefix = null, string suffix = null)
        {
            var type = typeof (T);
            if (type.IsPrimitive) return;

            //set table name
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            var hasTable = null != tableAttribute;
            if (null != tableName)
            {
                TableName = tableName;
            }
            else if (hasTable && null != tableAttribute.Name)
            {
                TableName = tableAttribute.Name;
            }
            else
            {
                TableName = prefix + type.Name + suffix;
            }

            var primaryKey = new List<PropertyInfo>();
            var properties = type.GetProperties();
            var propertyInfos = properties.Where(
                p =>
                {
                    var columnAttribute = p.GetCustomAttribute<ColumnAttribute>();

                    if (null == columnAttribute) return false;

                    if (columnAttribute.IsPrimaryKey)
                    {
                        primaryKey.Add(p);
                    }

                    return p.CanRead && p.CanWrite;
                });

            //generate snippets
            var columnStringBuilder = new StringBuilder();
            var updateValueStringBuilder = new StringBuilder();
            foreach (var property in propertyInfos)
            {
                if (columnStringBuilder.Length > 1)
                {
                    columnStringBuilder.Append(SqlQuerySnippet.Comma);
                    updateValueStringBuilder.Append(SqlQuerySnippet.Comma);
                }
                updateValueStringBuilder.AppendFormat("{0}=@{0}", property.Name);
                columnStringBuilder.Append(property.Name);
            }
            
            ColumnsSetValues = updateValueStringBuilder.ToString();
            ColumnSnippet = columnStringBuilder.ToString();
            ValuesSnippet = SqlQuerySnippet.At +
                            ColumnSnippet.Replace(SqlQuerySnippet.Comma, SqlQuerySnippet.Comma + SqlQuerySnippet.At);

            InsertQuery = string.Format(SqlQuerySnippet.InsertFormat, TableName, ColumnSnippet, ValuesSnippet);
            SelectQuery = SqlQuerySnippet.SelectAllSnippet + TableName;

            //generate primary key snippet
            if (primaryKey.Count < 1) throw new InvalidOperationException("no primary key in " + type.Name);

            var keyStringBuilder = new StringBuilder();
            foreach (var keyInfo in primaryKey)
            {
                var keyType = keyInfo.PropertyType;
                if (keyType.IsPrimitive)
                {
                    if (keyStringBuilder.Length > 1) keyStringBuilder.Append(SqlQuerySnippet.AndSnippet);
                    keyStringBuilder.Append(keyInfo.Name + "=" + SqlQuerySnippet.At + keyInfo.Name);
                }
                else if (keyType.IsValueType)
                {
                    foreach (var fieldInfo in keyType.GetFields().Where(p => p.IsPublic))
                    {
                        if (keyStringBuilder.Length > 1) keyStringBuilder.Append(SqlQuerySnippet.AndSnippet);
                        keyStringBuilder.Append(fieldInfo.Name + "=" + SqlQuerySnippet.At + fieldInfo.Name);
                    }
                }
                else
                {
                    foreach (var propertyInfo in keyType.GetProperties().Where(p => p.CanRead && p.CanWrite))
                    {
                        if (keyStringBuilder.Length > 1) keyStringBuilder.Append(SqlQuerySnippet.AndSnippet);
                        keyStringBuilder.Append(propertyInfo.Name + "=" + SqlQuerySnippet.At + propertyInfo.Name);
                    }
                }
            }

            PrimaryKeySnippet = keyStringBuilder.ToString();
            SelectByIDQuery = string.Format(SqlQuerySnippet.SelectFormat, TableName, keyStringBuilder);
            DeleteByIDQuery = string.Format(SqlQuerySnippet.DeleteFormat, TableName, keyStringBuilder);
            UpdateByIDQuery = string.Format(SqlQuerySnippet.UpdateFormat, TableName, ColumnsSetValues, keyStringBuilder);
        }

        #region auto generated query snippets 

        protected readonly string InsertQuery;
        protected readonly string SelectQuery;
        protected readonly string SelectByIDQuery;
        protected readonly string UpdateByIDQuery;
        protected readonly string DeleteByIDQuery;

        protected readonly string TableName;
        protected readonly string ColumnSnippet;
        protected readonly string ValuesSnippet;
        protected readonly string ColumnsSetValues;
        protected readonly string PrimaryKeySnippet;

        #endregion
    }
}