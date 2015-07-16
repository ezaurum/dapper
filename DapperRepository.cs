using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data; 
using System.Linq;
using System.Reflection;
using System.Security;
using slf4net;

namespace Dapper.Repository
{
    public class DapperRepository<T, TK> : IRepository<T, TK>
    {
        protected readonly IDbConnection DB;
        protected ILogger Logger;

        /// <exception cref="MemberAccessException">The class is abstract.-or- The constructor is a class initializer. </exception>
        /// <exception cref="MethodAccessException">The constructor is private or protected, and the caller lacks <see cref="F:System.Security.Permissions.ReflectionPermissionFlag.MemberAccess" />. </exception>
        /// <exception cref="TargetInvocationException">The invoked constructor throws an exception. </exception>
        /// <exception cref="TargetParameterCountException">An incorrect number of parameters was passed. </exception>
        /// <exception cref="SecurityException">The caller does not have the necessary code access permission.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="ProviderName"/> is <see langword="null" />.</exception> 
        public DapperRepository(string connectionString, string tableName = null, string prefix = null,
            string suffix = null)
        {
            if (typeof(T).IsPrimitive) return;

            var connectionStringSettings = ConfigurationManager.ConnectionStrings[connectionString];

            Type t;
            try
            {
                t = Type.GetType(connectionStringSettings.ProviderName);
            }
            catch (Exception e)
            {
                throw new ArgumentNullException(connectionStringSettings.ProviderName + " is not properly loaded. ", e);
            }

            if (null == t)
            {
                throw new ArgumentNullException(connectionStringSettings.ProviderName + " is not exist.");
            } 

            var constructorInfos = t.GetConstructors().FirstOrDefault(c =>
            {
                ParameterInfo[] parameterInfos = c.GetParameters();
                return parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(string);
            });

            if (constructorInfos != null)
            {
                DB = constructorInfos.Invoke(new[] {connectionStringSettings.ConnectionString}) as IDbConnection;
            }
            else
            {
                throw new ArgumentNullException(connectionStringSettings.ProviderName +" is not exist.");
            }
                
            Logger = LoggerFactory.GetLogger(GetType());

            //set table name 
            Dictionary<byte, string> queries;
            AutoQueryMaker.GenerateQueries(typeof(T), out queries, tableName, prefix, suffix);

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

            Logger.Info(InsertQuery);
            Logger.Info(SelectQuery);
            Logger.Info(SelectByIDQuery);
            Logger.Info(DeleteByIDQuery);
            Logger.Info(UpdateByIDQuery);
        }

        #region CREATE

        public virtual bool Create(T target, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(InsertQuery, target, tx);
        }

        public virtual bool Create(T target)
        {
            return 1 == DB.Execute(InsertQuery, target);
        }

        /// <exception cref="ArgumentNullException"><paramref name="source" /> is null.</exception>
        public virtual bool Create(IEnumerable<T> targets)
        {
            return targets.Count() == DB.Execute(InsertQuery, targets);
        }

        /// <exception cref="OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
        public virtual bool Create(IEnumerable<T> items, IDbTransaction tx)
        {
            return items.Count() == DB.Execute(InsertQuery, items, tx);
        }

        #endregion

        #region READ

        /// <summary>
        /// Read one row by one PK
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T Read(TK id)
        {
            try
            {
                return DB.Query<T>(SelectByIDQuery, new { PK_ID = id }).FirstOrDefault();
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return default(T);
            }
        } 

        public IEnumerable<T> ReadBy(object condition)
        {
            try
            {
                return DB.Query<T>(SelectByIDQuery, condition);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return null;
            }
        }

        public virtual IEnumerable<T> ReadAllList()
        {
            return ReadAll(DB);
        }

        public IEnumerable<T> ReadAll(IDbConnection db)
        {
            try
            {
                return db.Query<T>(SelectQuery);
            }
            catch (Exception e1)
            {
                Logger.Error(e1, "while auto data " + TableName);
                return null;
            }
        }

        #endregion

        #region UPDATE

        public virtual bool Update(IEnumerable<T> targets)
        {
            return targets.Count() == DB.Execute(UpdateByIDQuery, targets);
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
            return 1 == DB.Execute(UpdateByIDQuery, target);
        }

        #endregion

        #region DELETE

        /// <summary>
        /// Delete row by one PK
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(TK id)
        {
            return 0 < DB.Execute(DeleteByIDQuery, new { PK_ID = id });
        }

        public bool Delete(TK id, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(DeleteByIDQuery, new { PK_ID = id }, tx);
        }

        public bool Delete(object id, IDbTransaction tx)
        {
            return 1 == tx.Connection.Execute(DeleteByIDQuery, id, tx);
        }

        public virtual bool Delete(IEnumerable<T> targets)
        {
            return targets.Count() == DB.Execute(DeleteByIDQuery, targets);
        }

        public virtual bool Delete(IEnumerable<TK> itemIDs)
        {
            return itemIDs.Count() == DB.Execute(DeleteByIDQuery, itemIDs.Select(p => new { ID = p }).ToArray());
        }

        public bool Delete(IEnumerable<T> items, IDbTransaction tx)
        {
            return items.Count() == tx.Connection.Execute(DeleteByIDQuery, items, tx);
        }

        public bool Delete(IEnumerable<TK> itemIDs, IDbTransaction tx)
        {
            return itemIDs.Count() == tx.Connection.Execute(DeleteByIDQuery, itemIDs.Select(p => new { ID = p }).ToArray(), tx);
        }



        public bool Delete(object condition)
        {
            return 0 < DB.Execute(DeleteByIDQuery, condition);
        }

        #endregion

        protected bool ExecuteTransaction(Func<IDbTransaction, bool> action)
        {
            try
            {
                DB.Open();
                using (IDbTransaction tx = DB.BeginTransaction())
                {
                    if (!action(tx))
                    {
                        tx.Rollback();
                        DB.Close();
                        return false;
                    }
                    tx.Commit();
                }
                DB.Close();
                return true;
            }
            catch (Exception e)
            {
                //
                return false;
            }
        }

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