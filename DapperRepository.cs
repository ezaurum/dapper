﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Ezaurum.Commons;
using slf4net;

namespace Ezaurum.Dapper
{
    //기본 게임 객체들
    public abstract class DapperRepository<T> : DapperRepository<T, long> where T : IIdentifiableToken
    {
        protected DapperRepository(string connectionString)
            : base(connectionString)
        {
        }
    }

    //기본 키 형태가 long이 아닌 객체들
    public abstract class DapperRepository<T, TK> : SqlQueryContainer, ICRUDRepository<T, TK>
    {
        protected readonly SqlConnection DB;

        protected ILogger Logger;

        protected DapperRepository(string connectionString)
        {
            DB = new SqlConnection(ConfigurationManager.ConnectionStrings[connectionString].ConnectionString);
            Logger = LoggerFactory.GetLogger(GetType());
        }

        public abstract bool Create(T target);
        public abstract T Read(TK id);
        public abstract bool Update(T target);
        public abstract bool Delete(TK id);

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
    }

    /// 혼자서는 안 쓰이는 애들
    public abstract class DapperSubRepository<T, TK> : SqlQueryContainer, ICRUDTransactionalRepository<T, TK>
    {
        protected ILogger Logger;

        protected DapperSubRepository()
        {
            Logger = LoggerFactory.GetLogger(GetType());
        }

        public abstract bool Create(T target, IDbTransaction tx);
        public abstract bool Update(T target, IDbTransaction tx);
        public abstract bool Delete(TK id, IDbTransaction tx);
        public abstract T Read(TK id, IDbConnection connection);
    }
}