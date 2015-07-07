namespace Dapper.Repository
{
    /// <summary>
    /// Sql 쿼리 조각들
    /// Sql query snippets
    /// </summary>
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

        /// Insert 쿼리 일반 형태. 테이블 이름, 컬럼, 값을 채워넣는다.
        ///  Insert query default format. Requires tablename, columns, values.
        public const string InsertFormat =
            InsertSnippet + "{0}" + BracketStart + "{1}" + BracketEnd + Values + BracketStart + "{2}" + BracketEnd;

        ///  Select query default format. Requires tablename, conditions.
        public const string SelectFormat = SelectAllSnippet + "{0}" + WhereSnippet + "{1}";

        ///  Update query default format. Requires tablename, columns=values, conditions.
        public const string UpdateFormat = UpdateSnippet + "{0}" + Set + "{1}" + WhereSnippet + "{2}";

        /// Delete query default format. tablename, conditions
        public const string DeleteFormat = DeleteSnippet + "{0}" + WhereSnippet + "{1}";

        /// <summary>
        /// 값 매칭 형식
        /// </summary>
        public const string ValueMatchFormat = "{0}=@{1}";

        public const byte TableNameIndex = 0;
        public const byte InsertIndex = 1;
        public const byte SelectIndex = 2;
        public const byte SelectPKIndex = 3;
        public const byte SelectFKIndex = 4;
        public const byte UpdateIndex = 5;
        public const byte UpdatePKIndex = 6;
        public const byte DeleteIndex = 7;
        public const byte DeletePKIndex = 8;
    }
}