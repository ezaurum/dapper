using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ezaurum.Dapper
{
    public abstract class AutoQueryMaker<T, TK> : SqlQueryContainer
    {
        protected AutoQueryMaker(string tableName = null, string prefix = null, string suffix = null)
        {
            var type = typeof(T);
            if (type.IsPrimitive) return;

            bool hasColumn = type.GetProperties().Any(p => p.HasAttribute(typeof(ColumnAttribute)));
            bool hasIgnoreColumn = type.GetProperties().Any(p => p.HasAttribute(typeof(NotMappedAttribute)));

            IEnumerable<PropertyInfo> propertyInfos;
            if (hasColumn && hasIgnoreColumn)
            {
                propertyInfos =
                    type.GetProperties()
                        .Where(
                            p =>
                                p.CanRead && p.CanWrite && p.HasAttribute<ColumnAttribute>() &&
                                !p.HasAttribute<NotMappedAttribute>());
            }
            else if (hasColumn)
            {
                //only column 모드
                propertyInfos =
                    type.GetProperties().Where(p => p.CanRead && p.CanWrite && p.HasAttribute<ColumnAttribute>());
            }
            else if (hasIgnoreColumn)
            {
                propertyInfos =
                    type.GetProperties().Where(p => p.CanRead && p.CanWrite && !p.HasAttribute<NotMappedAttribute>());
            }
            else
            {
                //전체
                propertyInfos = type.GetProperties().Where(p => p.CanRead && p.CanWrite);
            }

            var sb = new StringBuilder();
            sb.Append("(");

            foreach (var property in propertyInfos)
            {
                sb.Append(property.Name + ",");
            }
            sb.Append(")");
            sb.Replace(",)", ")");
            ColumnSnippet = sb.ToString();
            ValuesSnippet = ColumnSnippet.Replace("(", "(@").Replace(",", ",@");

            AutoTableName = tableName ?? prefix + type.Name + suffix;

            AutoInsertQuery = InsertSnippet + AutoTableName + ColumnSnippet + Values + ValuesSnippet;
            AutoSelectQuery = SelectAllSnippet + AutoTableName;

            var keyType = typeof(TK);
            if (keyType.IsPrimitive || !keyType.IsValueType) return;
            var sb2 = new StringBuilder(WhereSnippet);
            foreach (var fieldInfo in keyType.GetFields().Where(p => p.IsPublic))
            {
                sb2.Append(fieldInfo.Name + "=" + At + fieldInfo.Name + Comma);
            }
            AutoSelectByIDQuery = SelectAllSnippet + AutoTableName + sb2.ToString().Substring(0, sb2.Length - 1);
        }

        #region auto generated query snippets

        protected readonly string AutoInsertQuery;
        protected readonly string AutoSelectByIDQuery;
        protected readonly string AutoSelectQuery;
        protected readonly string AutoTableName;
        protected readonly string ColumnSnippet;
        protected readonly string ValuesSnippet;
        #endregion
    }
}