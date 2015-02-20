namespace Ezaurum.Dapper
{
    public static class SqlQuerySnippet
    {
        public const string SelectAllSnippet = "SELECT * FROM ";
        public const string WhereSnippet = " WHERE ";
        public const string AndSnippet = " AND ";
        public const string OrSnippet = " OR ";
        public const string DeleteSnippet = "DELETE FROM ";
        public const string InsertSnippet = "INSERT INTO ";
        public const string UpdateSnippet = "UPDATE ";
        public const string Comma = ",";
        public const string At = "@";
        public const string Values = " VALUES ";
        public const string Set = " SET ";
        public const string BracketEnd = " ) ";
        public const string BracketStart = " ( ";

        ///  Insert query default format. Requires tablename, columns, values.
        public const string InsertFormat =
            InsertSnippet + "{0}" + BracketStart + "{1}" + BracketEnd + Values + BracketStart + "{2}" + BracketEnd;

        ///  Select query default format. Requires tablename, conditions.
        public const string SelectFormat = SelectAllSnippet + "{0}" + WhereSnippet + "{1}";

        ///  Update query default format. Requires tablename, columns=values, conditions.
        public const string UpdateFormat = UpdateSnippet + "{0}" + Set + "{1}" + WhereSnippet + "{2}";

        /// Delete query default format. tablename, conditions
        public const string DeleteFormat = DeleteSnippet + "{0}" + WhereSnippet + "{1}";
    }
}