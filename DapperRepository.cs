using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Ezaurum.Commons;
using slf4net;

namespace Ezaurum.Dapper
{
    public class DapperRepository<T>
    {
        protected readonly SqlConnection DB;
        protected ILogger Logger;

        public DapperRepository(string connectionString, string tableName = null, string prefix = null,
            string suffix = null)
        {
            DB = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
            Logger = LoggerFactory.GetLogger(GetType());

            var type = typeof (T);
            if (type.IsPrimitive) return;

            //set table name
            TableName = AutoQueryMaker.GetTableName(tableName, prefix, suffix, type);

            //generate snippets
            AutoQueryMaker.GenerateSnippets(type, 
                out ColumnSnippet, out ValuesSnippet, out ColumnsSetValues, 
                out PrimaryKeySnippet); 

            InsertQuery = string.Format(SqlQuerySnippet.InsertFormat, TableName, ColumnSnippet, ValuesSnippet);
            SelectQuery = SqlQuerySnippet.SelectAllSnippet + TableName;
            SelectByIDQuery = string.Format(SqlQuerySnippet.SelectFormat, TableName, PrimaryKeySnippet);
            DeleteByIDQuery = string.Format(SqlQuerySnippet.DeleteFormat, TableName, PrimaryKeySnippet);
            UpdateByIDQuery = string.Format(SqlQuerySnippet.UpdateFormat, TableName, ColumnsSetValues, PrimaryKeySnippet);
        }

        public virtual bool Create(T target)
        {
            return 1 == DB.Execute(InsertQuery, target);
        }

        protected bool ExecuteTransaction(Func<IDbTransaction, bool> action)
        {
            try
            {
                DB.Open();
                using (IDbTransaction tx = DB.BeginTransaction())
                {
                    if (!action(tx))
                    {
                        tx.Rollback();
                        DB.Close();
                        return false;
                    }
                    tx.Commit();
                }
                DB.Close();
                return true;
            }
            catch (Exception e)
            {
                //
                return false;
            }
        }

        public virtual IEnumerable<T> ReadAllList()
        {
            try
            {
                return DB.Query<T>(SelectQuery);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return null;
            }
        }

        public virtual bool Update(T target)
        {
            return 1 == DB.Execute(UpdateByIDQuery, target);
        }

        public IEnumerable<T> ReadAll(IDbConnection db)
        {
            try
            {
                return db.Query<T>(SelectQuery);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return null;
            }
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

    public class DapperRepository<T, TK> : DapperRepository<T> where T : IHasCompoundKey<TK>
    {
        public DapperRepository(string connectionString, string tableName = null, string prefix = null,
            string suffix = null) : base(connectionString, tableName, prefix, suffix)
        {
        }

        public virtual bool Delete(TK id)
        {
            return 0 < DB.Execute(DeleteByIDQuery, id);
        }

        public virtual Dictionary<TK, T> ReadAll()
        {
            try
            {
                return DB.Query<T>(SelectQuery).ToDictionary(e => e.Key);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return null;
            }
        }

        public virtual Dictionary<TK, T> ReadAll(Func<T, bool> filter)
        {
            try
            {
                return DB.Query<T>(SelectQuery).Where(filter).ToDictionary(e => e.Key);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return null;
            }
        }

        public virtual T Read(TK id)
        {
            try
            {
                return DB.Query<T>(SelectByIDQuery, id).FirstOrDefault();
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return default(T);
            }
        }
    }
}