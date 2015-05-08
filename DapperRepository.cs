using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Ezaurum.Commons;
using slf4net;

namespace Ezaurum.Dapper
{
    public class DapperRepository<T, TK> : IEnumerableRepository<T, TK>, ICRUDTransactionalRepository<T, TK>
    {
        protected readonly SqlConnection DB;
        protected ILogger Logger;

        public DapperRepository(string connectionString, string tableName = null, string prefix = null,
            string suffix = null)
        {
            if (typeof(T).IsPrimitive) return;

            DB = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
            Logger = LoggerFactory.GetLogger(GetType());

            //set table name 
            AutoQueryMaker.GenerateQueries(typeof (T), tableName, prefix, suffix, out TableName, 
                out InsertQuery, out SelectQuery, out SelectByIDQuery, out DeleteByIDQuery, out UpdateByIDQuery); 

            Logger.Info(InsertQuery);
            Logger.Info(SelectQuery);
            Logger.Info(SelectByIDQuery);
            Logger.Info(DeleteByIDQuery);
            Logger.Info(UpdateByIDQuery);
        }

        #region CREATE

        public virtual bool Create(T target, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(InsertQuery, target, tx);
        }

        public virtual bool Create(T target)
        {
            return 1 == DB.Execute(InsertQuery, target);
        }

        public virtual bool Create(IEnumerable<T> targets)
        {
            return ExecuteTransaction(tx => targets.All(target => Create(target, tx)));
        }

        #endregion

        #region READ

        public T Read(TK id)
        {
            try
            {
                return DB.Query<T>(SelectByIDQuery, new {PK_ID = id}).FirstOrDefault();
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return default(T);
            }
        }

        public virtual IEnumerable<T> ReadAllList()
        {
            return ReadAll(DB);
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

        #endregion

        #region UPDATE

        public virtual bool Update(IEnumerable<T> targets)
        {
            return ExecuteTransaction(tx => targets.All(target => Update(target, tx)));
        }

        public virtual bool Update(T target, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(UpdateByIDQuery, target, tx);
        }

        public virtual bool Update(T target)
        {
            return 1 == DB.Execute(UpdateByIDQuery, target);
        }

        #endregion
        
        #region DELETE

        public bool Delete(TK id, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(DeleteByIDQuery, id, tx);
        }

        public virtual bool Delete(IEnumerable<T> targets)
        {
            return ExecuteTransaction(tx => targets.All(target => Delete(target, tx)));
        }

        public virtual bool Delete(IEnumerable<TK> itemIDs)
        {
            return ExecuteTransaction(tx => itemIDs.All(target => Delete(target, tx)));
        }

        public bool Delete(TK id)
        {
            return 0 < DB.Execute(DeleteByIDQuery, new {ID=id});
        }

        private bool Delete(T target, IDbTransaction tx)
        {
            return 0 < tx.Connection.Execute(DeleteByIDQuery, target, tx);
        }

        #endregion
        
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

        #region auto generated query snippets

        protected readonly string InsertQuery;
        protected readonly string SelectQuery;
        protected readonly string SelectByIDQuery;
        protected readonly string UpdateByIDQuery;
        protected readonly string DeleteByIDQuery;

        protected readonly string TableName; 
        #endregion
    }

    public class DapperCompoundRepository<T, TK> : DapperRepository<T, TK> where T : IHasCompoundKey<TK>
    {
        public DapperCompoundRepository(string connectionString, string tableName = null, string prefix = null,
            string suffix = null) : base(connectionString, tableName, prefix, suffix)
        {
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
    }
}