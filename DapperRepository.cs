using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Npgsql;
using slf4net;

namespace Dapper.Repository
{
    public class DapperRepository<T, TK> : IRepository<T, TK>
    {
        public string ConnectionString { get; set; }
        protected readonly ILogger Logger;

        /// <exception cref="ArgumentNullException"><paramref name=" is not properly loaded. " /> is <see langword="null" />.</exception>
        public DapperRepository(string connectionString, string tableName = null, string prefix = null,
            string suffix = null)
            : this(tableName, prefix, suffix)
        {
            ConnectionString = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
        }

        private DapperRepository(string tableName, string prefix, string suffix)
        {
            //set table name 
            Dictionary<byte, string> queries;
            AutoQueryMaker.GenerateQueries(typeof (T), out queries, tableName, prefix, suffix);

            TableName = queries[SqlQuerySnippet.TableNameIndex];
            InsertQuery = queries[SqlQuerySnippet.InsertIndex];
            SelectQuery = queries[SqlQuerySnippet.SelectIndex];
            SelectByQuery = SelectQuery + SqlQuerySnippet.WhereSnippet + "{0}";
            SelectByIDQuery = queries[SqlQuerySnippet.SelectPKIndex];
            SelectByForeignKeyQuery = queries[SqlQuerySnippet.SelectFKIndex];
            UpdateByQuery = queries[SqlQuerySnippet.UpdateIndex];
            UpdateByIDQuery = queries[SqlQuerySnippet.UpdatePKIndex];
            DeleteByQuery = queries[SqlQuerySnippet.DeleteIndex];
            DeleteByIDQuery = queries[SqlQuerySnippet.DeletePKIndex];

            Logger = LoggerFactory.GetLogger(GetType());

            if (!Logger.IsDebugEnabled) return;
            Logger.Debug(InsertQuery);
            Logger.Debug(SelectQuery);
            Logger.Debug(SelectByIDQuery);
            Logger.Debug(DeleteByIDQuery);
            Logger.Debug(UpdateByIDQuery);
        }

        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        protected bool ExecuteTransaction(Func<IDbTransaction, bool> action)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    if (!action(tx))
                    {
                        tx.Rollback();
                        return false;
                    }
                    tx.Commit();
                }
                return true;
            }
            
        }

        #region CREATE

        public virtual bool Create(T target, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(InsertQuery, target, tx);
        }

        public virtual bool Create(T target)
        {
            using (var conn = new NpgsqlConnection(ConnectionString)) 
            {return 1 == conn.Execute(InsertQuery, target);}
        }

        /// <exception cref="ArgumentNullException"><paramref name="source" /> is null.</exception>
        public virtual bool Create(IEnumerable<T> targets)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {return targets.Count() == conn.Execute(InsertQuery, targets);}
        }

        /// <exception cref="OverflowException">
        ///     The number of elements in <paramref name="source" /> is larger than
        ///     <see cref="F:System.Int32.MaxValue" />.
        /// </exception>
        public virtual bool Create(IEnumerable<T> items, IDbTransaction tx)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {return items.Count() == conn.Execute(InsertQuery, items, tx);}
        }

        #endregion

        #region READ

        /// <summary>
        ///     Read one row by one PK
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Read(TK id)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                return con.Query<T>(SelectByIDQuery, new { PK_ID = id }).FirstOrDefault();
            }
        }

        public IEnumerable<T> ReadBy(object condition)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                return con.Query<T>(SelectByIDQuery, condition);
            }
        }

        public IEnumerable<T> ReadAll()
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                return conn.Query<T>(SelectQuery);
            } 
        }

        public IEnumerable<T> ReadAll(IDbConnection db)
        {
            return db.Query<T>(SelectQuery);
        }

        #endregion

        #region UPDATE

        public virtual bool Update(IEnumerable<T> targets)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                return targets.Count() == con.Execute(UpdateByIDQuery, targets);
            }
        }

        public bool Update(IEnumerable<T> items, IDbTransaction tx)
        {
            return items.Count() == tx.Connection.Execute(UpdateByIDQuery, items, tx);
        }

        public virtual bool Update(T target, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(UpdateByIDQuery, target, tx);
        }

        public virtual bool Update(T target)
        {
            using (var con = new NpgsqlConnection(ConnectionString))
            {
                return 1 == con.Execute(UpdateByIDQuery, target);
            }
        }

        #endregion

        #region DELETE

        /// <summary>
        ///     Delete row by one PK
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(TK id)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {return 0 < conn.Execute(DeleteByIDQuery, new {PK_ID = id});}
        }
      

        public bool Delete(TK id, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(DeleteByIDQuery, new {PK_ID = id}, tx);
        }

        public bool Delete(object id, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(DeleteByIDQuery, id, tx);
        }

        public virtual bool Delete(IEnumerable<T> targets)
        {
            using (var conn = new NpgsqlConnection(ConnectionString)) {return targets.Count() == conn.Execute(DeleteByIDQuery, targets);}
        }

        public virtual bool Delete(IEnumerable<TK> itemIDs)
        {
            using (var conn = new NpgsqlConnection(ConnectionString)) {return itemIDs.Count() == conn.Execute(DeleteByIDQuery, itemIDs.Select(p => new { ID = p }).ToArray());}
        }

        public bool Delete(IEnumerable<T> items, IDbTransaction tx)
        {
            return items.Count() == tx.Connection.Execute(DeleteByIDQuery, items, tx);
        }

        public bool Delete(IEnumerable<TK> itemIDs, IDbTransaction tx)
        {
            return itemIDs.Count() ==
                   tx.Connection.Execute(DeleteByIDQuery, itemIDs.Select(p => new {ID = p}).ToArray(), tx);
        }

        public bool DeleteBy(object condition)
        {

            using (var conn = new NpgsqlConnection(ConnectionString)) {return 0 < conn.Execute(DeleteByIDQuery, condition);}
        }

        public bool DeleteBy(string where, object condition)
        {
            using (var conn = new NpgsqlConnection(ConnectionString))
            {
                return 0 < conn.Execute(string.Format(DeleteByQuery , where), condition);
            }
        }

        #endregion

        #region auto generated query snippets

        protected readonly string InsertQuery;
        protected readonly string SelectQuery;
        protected readonly string SelectByQuery;
        protected readonly string SelectByIDQuery;
        protected readonly string SelectByForeignKeyQuery;
        protected readonly string UpdateByQuery;
        protected readonly string UpdateByIDQuery;
        protected readonly string DeleteByQuery;
        protected readonly string DeleteByIDQuery;

        protected readonly string TableName;

        #endregion
    }
}
