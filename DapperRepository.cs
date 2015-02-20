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
    public class DapperRepository<T> : AutoQueryMaker<T>
    {
        protected readonly SqlConnection DB;
        protected ILogger Logger;

        public DapperRepository(string connectionString, string tableName = null, string prefix = null,
          string suffix = null)
            : base(tableName, prefix, suffix)
        {
            DB = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
            Logger = LoggerFactory.GetLogger(GetType());
        }


        public virtual bool Create(T target)
        {
            return 1 == DB.Execute(AutoInsertQuery, target);
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
                return DB.Query<T>(AutoSelectQuery);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + AutoTableName);
                return null;
            }
        }

        public virtual bool Update(T target)
        {
            return 1 == DB.Execute(AutoUpdateByIDQuery, target);
        }

        public IEnumerable<T> ReadAll(IDbConnection db)
        {
            try
            {
                return db.Query<T>(AutoSelectQuery);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + AutoTableName);
                return null;
            }
        }

    }

    public class DapperRepository<T, TK> : DapperRepository<T> where T : IHasCompoundKey<TK>
    {

        public virtual bool Delete(TK id)
        {
            return 0 < DB.Execute(AutoDeleteByIDQuery, id);
        }

        public virtual Dictionary<TK, T> ReadAll()
        {
            try
            {
                return DB.Query<T>(AutoSelectQuery).ToDictionary(e => e.Key);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + AutoTableName);
                return null;
            }
        }

        public virtual Dictionary<TK, T> ReadAll(Func<T, bool> filter)
        {
            try
            {
                return DB.Query<T>(AutoSelectQuery).Where(filter).ToDictionary(e => e.Key);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + AutoTableName);
                return null;
            }
        } 

        public virtual T Read(TK id)
        {
            try
            {
                return DB.Query<T>(AutoSelectByIDQuery, id).FirstOrDefault();
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + AutoTableName);
                return default(T);
            }
        }


        public DapperRepository(string connectionString, string tableName = null, string prefix = null, string suffix = null) : base(connectionString, tableName, prefix, suffix)
        {
        }
    }
}