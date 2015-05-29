using Ezaurum.Commons;

namespace Ezaurum.Dapper
{
    public class DapperGameRepository<T, TK, TFK> : DapperRepository<T, TK, TFK> , IGameRepository<T, TK, TFK>
    {
        public DapperGameRepository(string connectionString, string tableName = null, string prefix = null, string suffix = null) : base(connectionString, tableName, prefix, suffix)
        {
        }
    }

    public class DapperGameRepository<T, TK> : DapperRepository<T, TK>, IGameRepository<T, TK>
    {
        public DapperGameRepository(string connectionString, string tableName = null, string prefix = null, string suffix = null)
            : base(connectionString, tableName, prefix, suffix)
        {
        }
    }

    public class DapperGameRepository<T> : DapperRepository<T>, IGameRepository<T, long>
    {
        public DapperGameRepository(string connectionString, string tableName = null, string prefix = null, string suffix = null)
            : base(connectionString, tableName, prefix, suffix)
        {
        }
    }
}