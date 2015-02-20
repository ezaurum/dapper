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
                AutoTableName = tableName;
            }
            else if (hasTable && null != tableAttribute.Name)
            {
                AutoTableName = tableAttribute.Name;
            }
            else
            {
                AutoTableName = prefix + type.Name + suffix;
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

            var sb = new StringBuilder();
            foreach (var property in propertyInfos)
            {
                if (sb.Length > 1) sb.Append(SqlQuerySnippet.Comma);
                sb.Append(property.Name);
            }

            ColumnSnippet = sb.ToString();
            ValuesSnippet = SqlQuerySnippet.At +
                            ColumnSnippet.Replace(SqlQuerySnippet.Comma, SqlQuerySnippet.Comma + SqlQuerySnippet.At);

            AutoInsertQuery = string.Format(SqlQuerySnippet.InsertFormat, AutoTableName, ColumnSnippet, ValuesSnippet);
            AutoSelectQuery = SqlQuerySnippet.SelectAllSnippet + AutoTableName;

            if (primaryKey.Count < 1) throw new InvalidOperationException("no primary key in " + type.Name);

            var sb2 = new StringBuilder();
            foreach (var keyInfo in primaryKey)
            {
                var keyType = primaryKey.GetType();
                if (keyType.IsPrimitive)
                {
                    if (sb2.Length > 1) sb2.Append(SqlQuerySnippet.Comma);
                    sb2.Append(keyInfo.Name + "=" + SqlQuerySnippet.At + keyInfo.Name);
                }
                else if (keyType.IsValueType)
                {
                    foreach (var fieldInfo in keyType.GetFields().Where(p => p.IsPublic))
                    {
                        if (sb2.Length > 1) sb2.Append(SqlQuerySnippet.Comma);
                        sb2.Append(fieldInfo.Name + "=" + SqlQuerySnippet.At + fieldInfo.Name);
                    }
                }
                else
                {
                    foreach (var propertyInfo in keyType.GetProperties().Where(p => p.CanRead && p.CanWrite))
                    {
                        if (sb2.Length > 1) sb2.Append(SqlQuerySnippet.Comma);
                        sb2.Append(propertyInfo.Name + "=" + SqlQuerySnippet.At + propertyInfo.Name);
                    }
                }
            }

            AutoSelectByIDQuery = string.Format(SqlQuerySnippet.SelectFormat, AutoTableName, sb2);
            AutoDeleteByIDQuery = string.Format(SqlQuerySnippet.DeleteFormat, AutoTableName, sb2);
            AutoUpdateByIDQuery = string.Format(SqlQuerySnippet.UpdateFormat, AutoTableName, sb2);
        }

        

        #region auto generated query snippets 

        protected readonly string AutoInsertQuery;
        protected readonly string AutoSelectQuery;
        protected readonly string AutoSelectByIDQuery;
        protected readonly string AutoUpdateByIDQuery;
        protected readonly string AutoDeleteByIDQuery;

        protected readonly string AutoTableName;
        protected readonly string ColumnSnippet;
        protected readonly string ValuesSnippet;

        #endregion
    }
}