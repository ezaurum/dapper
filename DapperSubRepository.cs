using System.Data;
using Ezaurum.Commons;
using slf4net;

namespace Ezaurum.Dapper
{
    /// 혼자서는 안 쓰이는 애들
    public abstract class DapperSubRepository<T, TK> : ICRUDTransactionalRepository<T, TK>
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