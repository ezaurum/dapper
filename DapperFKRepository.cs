using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper.Repository
{
    public class DapperFKRepository<T, TK, TFk> : DapperRepository<T, TK>, IFKRepository<T, TK, TFk>
    {
        public DapperFKRepository(IDbConnection connection, string tableName = null, string prefix = null,
            string suffix = null)
            : base(connection, tableName, prefix, suffix)
        {
        }

        public DapperFKRepository(string connectionString, string tableName = null, string prefix = null,
            string suffix = null) : base(connectionString, tableName, prefix, suffix)
        {
        }

        public IEnumerable<T> ReadByForeignKey(TFk id)
        {
            try
            {
                return DB.Query<T>(SelectByForeignKeyQuery, new {FK_ID = id});
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return null;
            }
        }
    }
}
