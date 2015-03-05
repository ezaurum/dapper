using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Ezaurum.Commons;
using slf4net;

namespace Ezaurum.Dapper
{
    //기본 키가 long형인 게임 객체들
    public abstract class AbstractDapperRepository<T> : AbstractDapperRepository<T, long> where T : IIdentifiableToken<long>
    {
        protected AbstractDapperRepository(string connectionString)
            : base(connectionString)
        {
        }
    }

    //기본 키 형태가 long이 아닌 객체들
    public abstract class AbstractDapperRepository<T, TK> : ICRUDRepository<T, TK>
    {
        protected readonly SqlConnection DB;

        protected ILogger Logger;

        protected AbstractDapperRepository(string connectionString)
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

        #region query snippets

        protected const string SelectAllSnippet = "SELECT * FROM ";
        protected const string WhereSnippet = " WHERE ";
        protected const string AndSnippet = " AND ";
        protected const string OrSnippet = " OR ";
        protected const string IDConditionSnippet = WhereSnippet + "ID=@ID";
        protected const string NameConditionSnippet = WhereSnippet + "Name=@Name";
        protected const string PlayerIDConditionSnippet = WhereSnippet + "PlayerID=@PlayerID";
        protected const string UserIDConditionSnippet = WhereSnippet + "UserID=@UserID";
        protected const string DeleteSnippet = "DELETE FROM ";
        protected const string InsertSnippet = @"INSERT INTO ";
        protected const string UpdateSnippet = @"UPDATE ";
        protected const string Comma = ",";
        protected const string At = "@";
        protected const string Values = " VALUES ";
        protected const string Set = " SET ";
        protected const string BracketEnd = " ) ";
        protected const string BracketStart = " ( ";

        #endregion

    }
}