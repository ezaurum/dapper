using System.Collections.Generic;
using System.Data;

namespace Dapper.Repository
{
    /// <summary>
    /// Repository interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TFK"></typeparam>
    public interface IRepository<T, TK, in TFK>
    {
        bool Create(T target);
        bool Create(T target, IDbTransaction tx);
        bool Create(IEnumerable<T> items);
        bool Create(IEnumerable<T> items, IDbTransaction tx);

        T Read(TK id);
        IEnumerable<T> ReadByForeignKey(TFK id);
        IEnumerable<T> ReadBy(object condition);

        bool Update(T target);
        bool Update(T target, IDbTransaction tx);
        bool Update(IEnumerable<T> items);
        bool Update(IEnumerable<T> items, IDbTransaction tx);

        bool Delete(TK id);
        bool Delete(TK id, IDbTransaction tx);
        bool Delete(IEnumerable<T> items);
        bool Delete(IEnumerable<TK> itemIDs);
        bool Delete(IEnumerable<T> items, IDbTransaction tx);
        bool Delete(IEnumerable<TK> itemIDs, IDbTransaction tx);
    }
}