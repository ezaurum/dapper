using System.Collections.Generic;

namespace Dapper.Repository
{
    /// <summary>
    /// Repository interface have ForeignKey
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TFK"></typeparam>
    public interface IFKRepository<T, in TK, in TFK> : IRepository<T, TK>
    {
        IEnumerable<T> ReadByForeignKey(TFK id);
    }

    public interface IFKRepository<T, in TK> : IRepository<T, TK>
    {
        IEnumerable<T> ReadByForeignKey(TK id);
    }
}