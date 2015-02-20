using System.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using System.Text;

namespace Ezaurum.Dapper
{
    public abstract class AutoQueryMaker<T, TK>
    {
        protected AutoQueryMaker(string tableName = null, string prefix = null, string suffix = null)
        {
            var type = typeof(T);
            if (type.IsPrimitive) return;

            //set table name
            TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();
            bool hasTable = null != tableAttribute;
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
            
            bool hasColumn = type.GetProperties().Any(p => p.HasAttribute(typeof(ColumnAttribute)));
            var propertyInfos = hasColumn 
                ? type.GetProperties().Where(p => p.CanRead && p.CanWrite && p.HasAttribute<ColumnAttribute>()) 
                : type.GetProperties().Where(p => p.CanRead && p.CanWrite);

            var sb = new StringBuilder();
            foreach (var property in propertyInfos)
            {
                if (sb.Length > 1) sb.Append(SqlQuerySnippet.Comma);
                sb.Append(property.Name);
            }
            
            ColumnSnippet = sb.ToString();
            ValuesSnippet = SqlQuerySnippet.At + ColumnSnippet.Replace(SqlQuerySnippet.Comma, SqlQuerySnippet.Comma+SqlQuerySnippet.At);

            AutoInsertQuery = string.Format(SqlQuerySnippet.InsertFormat,AutoTableName, ColumnSnippet, ValuesSnippet);
            AutoSelectQuery = SqlQuerySnippet.SelectAllSnippet + AutoTableName;

            var keyType = typeof(TK);
            if (keyType.IsPrimitive || !keyType.IsValueType) return;
            var sb2 = new StringBuilder();
            foreach (var fieldInfo in keyType.GetFields().Where(p => p.IsPublic))
            {
                if (sb2.Length > 1) sb2.Append(SqlQuerySnippet.Comma);
                sb2.Append(fieldInfo.Name + "=" + SqlQuerySnippet.At + fieldInfo.Name);
            }
            AutoSelectByIDQuery = string.Format(SqlQuerySnippet.SelectFormat, AutoTableName,sb2);
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