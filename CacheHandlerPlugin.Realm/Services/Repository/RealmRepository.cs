using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CacheHandlerPlugin.Realm.Extensions;
using Realms;

namespace CacheHandlerPlugin.Realm.Services.Repository
{
    public class RealmRepository : IRealmRepository
    {
        private RealmConfiguration Configuration { get; }

        private Realms.Realm Instance => Configuration == null ? Realms.Realm.GetInstance() : Realms.Realm.GetInstance(Configuration);

        public RealmRepository(RealmConfiguration configuration)
        {
            Configuration = configuration;
        }

        public bool RemoveAll()
        {
            return BeginTransaction(realm => realm.RemoveAll());
        }

        public bool Remove<T>(string key) where T : class
        {
            return BeginTransaction(realm => realm.Remove(realm.Find(GetClassName<T>(), key)));
        }

        public bool Remove<T>(T entity) where T : class
        {
            return BeginTransaction(realm => realm.Remove(entity as RealmObject));
        }

        public bool RemoveAll<T>() where T : class
        {
            return BeginTransaction(realm => realm.RemoveAll(GetClassName<T>()));
        }

        public T Find<T>(string primaryKey) where T : class
        {
            return Instance.Find(GetClassName<T>(), primaryKey) as T;
        }

        public IEnumerable<T> All<T>() where T : class
        {
            return Instance.All(GetClassName<T>()).Cast<T>();
        }

        public bool Add<T>(T entity) where T : class
        {
            return BeginTransaction(realm => realm.Add(entity as RealmObject, true));
        }

        public bool Update<T>(string primaryKey, Action<T> updateAction) where T : class
        {
            return BeginTransaction(realm => updateAction?.Invoke(realm.Find(GetClassName<T>(), primaryKey) as T));
        }

        public bool Update(Action updateAction)
        {
            return BeginTransaction(realm => updateAction?.Invoke());
        }

        private static string GetClassName<T>() where T : class
        {
            var type = typeof(T);
            return type.GetTypeInfo().GetMappedOrOriginalName();
        }

        private bool BeginTransaction(Action<Realms.Realm> tune = null)
        {
            var realm = Instance;
            using (var transaction = realm.BeginWrite())
            {
                tune?.Invoke(realm);
                transaction.Commit();
            }

            return true;
        }
    }
}
