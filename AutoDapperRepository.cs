using System;
using System.Collections.Generic;
using System.Configuration; 
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Ezaurum.Commons;
using slf4net;


namespace Ezaurum.Dapper
{
    public class AutoDapperRepository<T> : AutoQueryMaker<T, long> where T : IIdentifiableToken
    {
        protected readonly SqlConnection DB;
        protected ILogger Logger;

        public AutoDapperRepository(string connectionString, string tableName = null)
            : base(tableName)
        {
            DB = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
            Logger = LoggerFactory.GetLogger(GetType());
        }
    }

    public class AutoDapperRepository<T, TK> : AutoQueryMaker<T, TK> where T : IHasCompoundKey<TK>
    {
        protected readonly SqlConnection DB;
        protected ILogger Logger;
        public AutoDapperRepository(string connectionString, string tableName = null, string prefix = null, string suffix = null)
            : base(tableName, prefix, suffix)
        {
            DB = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
            Logger = LoggerFactory.GetLogger(GetType());
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

        public virtual bool Create(T target)
        {
            return 1 == DB.Execute(AutoInsertQuery, target);
        }

        //TODO
        public interface IDbAction : IDisposable
        {
            IDbTransaction Tx { get; }
            IDbConnection Connection { get; }

            void Success();
            void Fail();
        }

        //TODO
        public IDbAction GetDbAction()
        {
            return null;
        }

        //TODO
        public bool TestDelete()
        {
            using (var c = GetDbAction())
            {

                return true;
            }
        }
    }
}