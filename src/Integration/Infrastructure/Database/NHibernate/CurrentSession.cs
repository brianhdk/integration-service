
using System;
using System.Data;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using NHibernate.Type;

namespace Vertica.Integration.Infrastructure.Database.NHibernate
{
	internal class CurrentSession : ICurrentSession
	{
		private readonly Lazy<ISession> _session;
		private readonly Func<bool> _saveChanges;

		public CurrentSession(ISessionFactory sessionFactory, Func<bool> saveChanges)
		{
			if (sessionFactory == null) throw new ArgumentNullException("sessionFactory");
			if (saveChanges == null) throw new ArgumentNullException("saveChanges");

			_session = new Lazy<ISession>(() =>
			{
				ISession session = sessionFactory.OpenSession();
				session.BeginTransaction();

				return session;
			});

			_saveChanges = saveChanges;
		}

		public void Dispose()
		{
			if (_session.IsValueCreated)
			{
				try
				{
					if (_session.Value.IsOpen && _session.Value.Transaction != null && _session.Value.Transaction.IsActive)
					{
						if (_saveChanges())
						{
							try
							{
								_session.Value.Transaction.Commit();
							}
							catch (Exception)
							{
								_session.Value.Transaction.Rollback();
								throw;
							}
						}
						else
						{
							_session.Value.Transaction.Rollback();
						}
					}
				}
				finally
				{
					_session.Value.Dispose();
				}
			}
		}

		public void Flush()
		{
			_session.Value.Flush();
		}

		public IDbConnection Disconnect()
		{
			return _session.Value.Disconnect();
		}

		public void Reconnect()
		{
			_session.Value.Reconnect();
		}

		public void Reconnect(IDbConnection connection)
		{
			_session.Value.Reconnect(connection);
		}

		public IDbConnection Close()
		{
			return _session.Value.Close();
		}

		public void CancelQuery()
		{
			_session.Value.CancelQuery();
		}

		public bool IsDirty()
		{
			return _session.Value.IsDirty();
		}

		public bool IsReadOnly(object entityOrProxy)
		{
			return _session.Value.IsReadOnly(entityOrProxy);
		}

		public void SetReadOnly(object entityOrProxy, bool readOnly)
		{
			_session.Value.SetReadOnly(entityOrProxy, readOnly);
		}

		public object GetIdentifier(object obj)
		{
			return _session.Value.GetIdentifier(obj);
		}

		public bool Contains(object obj)
		{
			return _session.Value.Contains(obj);
		}

		public void Evict(object obj)
		{
			_session.Value.Evict(obj);
		}

		public object Load(Type theType, object id, LockMode lockMode)
		{
			return _session.Value.Load(theType, id, lockMode);
		}

		public object Load(string entityName, object id, LockMode lockMode)
		{
			return _session.Value.Load(entityName, id, lockMode);
		}

		public object Load(Type theType, object id)
		{
			return _session.Value.Load(theType, id);
		}

		public T Load<T>(object id, LockMode lockMode)
		{
			return _session.Value.Load<T>(id, lockMode);
		}

		public T Load<T>(object id)
		{
			return _session.Value.Load<T>(id);
		}

		public object Load(string entityName, object id)
		{
			return _session.Value.Load(entityName, id);
		}

		public void Load(object obj, object id)
		{
			_session.Value.Load(obj, id);
		}

		public void Replicate(object obj, ReplicationMode replicationMode)
		{
			_session.Value.Replicate(obj, replicationMode);
		}

		public void Replicate(string entityName, object obj, ReplicationMode replicationMode)
		{
			_session.Value.Replicate(entityName, obj, replicationMode);
		}

		public object Save(object obj)
		{
			return _session.Value.Save(obj);
		}

		public void Save(object obj, object id)
		{
			_session.Value.Save(obj, id);
		}

		public object Save(string entityName, object obj)
		{
			return _session.Value.Save(entityName, obj);
		}

		public void SaveOrUpdate(object obj)
		{
			_session.Value.SaveOrUpdate(obj);
		}

		public void SaveOrUpdate(string entityName, object obj)
		{
			_session.Value.SaveOrUpdate(entityName, obj);
		}

		public void Update(object obj)
		{
			_session.Value.Update(obj);
		}

		public void Update(object obj, object id)
		{
			_session.Value.Update(obj, id);
		}

		public void Update(string entityName, object obj)
		{
			_session.Value.Update(entityName, obj);
		}

		public object Merge(object obj)
		{
			return _session.Value.Merge(obj);
		}

		public object Merge(string entityName, object obj)
		{
			return _session.Value.Merge(entityName, obj);
		}

		public T Merge<T>(T entity) where T : class
		{
			return _session.Value.Merge(entity);
		}

		public T Merge<T>(string entityName, T entity) where T : class
		{
			return _session.Value.Merge(entityName, entity);
		}

		public void Persist(object obj)
		{
			_session.Value.Persist(obj);
		}

		public void Persist(string entityName, object obj)
		{
			_session.Value.Persist(entityName, obj);
		}

		public void Delete(object obj)
		{
			_session.Value.Delete(obj);
		}

		public void Delete(string entityName, object obj)
		{
			_session.Value.Delete(entityName, obj);
		}

		public int Delete(string query)
		{
			return _session.Value.Delete(query);
		}

		public int Delete(string query, object value, IType type)
		{
			return _session.Value.Delete(query, value, type);
		}

		public int Delete(string query, object[] values, IType[] types)
		{
			return _session.Value.Delete(query, values, types);
		}

		public void Lock(object obj, LockMode lockMode)
		{
			_session.Value.Lock(obj, lockMode);
		}

		public void Lock(string entityName, object obj, LockMode lockMode)
		{
			_session.Value.Lock(entityName, obj, lockMode);
		}

		public void Refresh(object obj)
		{
			_session.Value.Refresh(obj);
		}

		public void Refresh(object obj, LockMode lockMode)
		{
			_session.Value.Refresh(obj, lockMode);
		}

		public LockMode GetCurrentLockMode(object obj)
		{
			return _session.Value.GetCurrentLockMode(obj);
		}

		public ITransaction BeginTransaction()
		{
			return _session.Value.BeginTransaction();
		}

		public ITransaction BeginTransaction(IsolationLevel isolationLevel)
		{
			return _session.Value.BeginTransaction(isolationLevel);
		}

		public ICriteria CreateCriteria<T>() where T : class
		{
			return _session.Value.CreateCriteria<T>();
		}

		public ICriteria CreateCriteria<T>(string alias) where T : class
		{
			return _session.Value.CreateCriteria<T>(alias);
		}

		public ICriteria CreateCriteria(Type persistentClass)
		{
			return _session.Value.CreateCriteria(persistentClass);
		}

		public ICriteria CreateCriteria(Type persistentClass, string alias)
		{
			return _session.Value.CreateCriteria(persistentClass, alias);
		}

		public ICriteria CreateCriteria(string entityName)
		{
			return _session.Value.CreateCriteria(entityName);
		}

		public ICriteria CreateCriteria(string entityName, string alias)
		{
			return _session.Value.CreateCriteria(entityName, alias);
		}

		public IQueryOver<T, T> QueryOver<T>() where T : class
		{
			return _session.Value.QueryOver<T>();
		}

		public IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class
		{
			return _session.Value.QueryOver(alias);
		}

		public IQueryOver<T, T> QueryOver<T>(string entityName) where T : class
		{
			return _session.Value.QueryOver<T>(entityName);
		}

		public IQueryOver<T, T> QueryOver<T>(string entityName, Expression<Func<T>> alias) where T : class
		{
			return _session.Value.QueryOver(entityName, alias);
		}

		public IQuery CreateQuery(string queryString)
		{
			return _session.Value.CreateQuery(queryString);
		}

		public IQuery CreateFilter(object collection, string queryString)
		{
			return _session.Value.CreateFilter(collection, queryString);
		}

		public IQuery GetNamedQuery(string queryName)
		{
			return _session.Value.GetNamedQuery(queryName);
		}

		public ISQLQuery CreateSQLQuery(string queryString)
		{
			return _session.Value.CreateSQLQuery(queryString);
		}

		public void Clear()
		{
			_session.Value.Clear();
		}

		public object Get(Type clazz, object id)
		{
			return _session.Value.Get(clazz, id);
		}

		public object Get(Type clazz, object id, LockMode lockMode)
		{
			return _session.Value.Get(clazz, id, lockMode);
		}

		public object Get(string entityName, object id)
		{
			return _session.Value.Get(entityName, id);
		}

		public T Get<T>(object id)
		{
			return _session.Value.Get<T>(id);
		}

		public T Get<T>(object id, LockMode lockMode)
		{
			return _session.Value.Get<T>(id, lockMode);
		}

		public string GetEntityName(object obj)
		{
			return _session.Value.GetEntityName(obj);
		}

		public IFilter EnableFilter(string filterName)
		{
			return _session.Value.EnableFilter(filterName);
		}

		public IFilter GetEnabledFilter(string filterName)
		{
			return _session.Value.GetEnabledFilter(filterName);
		}

		public void DisableFilter(string filterName)
		{
			_session.Value.DisableFilter(filterName);
		}

		public IMultiQuery CreateMultiQuery()
		{
			return _session.Value.CreateMultiQuery();
		}

		public ISession SetBatchSize(int batchSize)
		{
			return _session.Value.SetBatchSize(batchSize);
		}

		public ISessionImplementor GetSessionImplementation()
		{
			return _session.Value.GetSessionImplementation();
		}

		public IMultiCriteria CreateMultiCriteria()
		{
			return _session.Value.CreateMultiCriteria();
		}

		public ISession GetSession(EntityMode entityMode)
		{
			return _session.Value.GetSession(entityMode);
		}

		public EntityMode ActiveEntityMode
		{
			get { return _session.Value.ActiveEntityMode; }
		}

		public FlushMode FlushMode
		{
			get { return _session.Value.FlushMode; }
			set { _session.Value.FlushMode = value; }
		}

		public CacheMode CacheMode
		{
			get { return _session.Value.CacheMode; }
			set { _session.Value.CacheMode = value; }
		}

		public ISessionFactory SessionFactory
		{
			get { return _session.Value.SessionFactory; }
		}

		public IDbConnection Connection
		{
			get { return _session.Value.Connection; }
		}

		public bool IsOpen
		{
			get { return _session.Value.IsOpen; }
		}

		public bool IsConnected
		{
			get { return _session.Value.IsConnected; }
		}

		public bool DefaultReadOnly
		{
			get { return _session.Value.DefaultReadOnly; }
			set { _session.Value.DefaultReadOnly = value; }
		}

		public ITransaction Transaction
		{
			get { return _session.Value.Transaction; }
		}

		public ISessionStatistics Statistics
		{
			get { return _session.Value.Statistics; }
		}

	    public void Save(string entityName, object obj, object id)
	    {
	        _session.Value.Save(entityName, obj, id);
	    }

	    public void SaveOrUpdate(string entityName, object obj, object id)
	    {
            _session.Value.SaveOrUpdate(entityName, obj, id);
	    }

	    public void Update(string entityName, object obj, object id)
	    {
            _session.Value.Update(entityName, obj, id);
	    }
	}
}