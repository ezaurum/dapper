using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using slf4net;

namespace Ezaurum.Dapper
{
    public class AutoDapperSubRepository<T, TK> : AutoQueryMaker<T, TK>
    {
        protected ILogger Logger;

        public AutoDapperSubRepository(string tableName) : base(tableName)
        {
            Logger = LoggerFactory.GetLogger(GetType());
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
}