namespace Ezaurum.Dapper
{
    public abstract class SqlQueryContainer
    {
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