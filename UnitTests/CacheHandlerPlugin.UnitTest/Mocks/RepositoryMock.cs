using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CacheHandlerPlugin.Realm.Services.Repository;

namespace CacheHandlerPlugin.UnitTest.Mocks
{
    public class RepositoryMock : IRealmRepository
    {
        private readonly List<object> realmObjects = new List<object>();

        public bool Add<T>(T entity) where T : class
        {
            this.realmObjects.Add(entity);

            return true;
        }

        public IEnumerable<T> All<T>() where T : class
        {
            return this.realmObjects.Where(x => x.GetType() == typeof(T)).Select(x => (T)x);
        }

        public T Find<T>(string primaryKey) where T : class
        {
            return (T)this.realmObjects.Find(x =>
            {
                var primaryKeyProperty = this.GetPrimaryKeyProperty(x);

                return primaryKeyProperty.GetValue(x).Equals(primaryKey);
            });
        }

        public bool Remove<T>(string key) where T : class
        {
            var entity = this.Find<T>(key);

            return this.realmObjects.Remove(entity);
        }

        public bool Remove<T>(T entity) where T : class
        {
            return this.realmObjects.Remove(entity);
        }

        public bool RemoveAll<T>() where T : class
        {
            this.realmObjects.RemoveAll(x => x.GetType() == typeof(T));

            return true;
        }

        public bool RemoveAll()
        {
            this.realmObjects.Clear();

            return true;
        }

        public bool Update<T>(string primaryKey, Action<T> updateAction) where T : class
        {
            var entity = this.Find<T>(primaryKey);
            updateAction?.Invoke(entity);

            return true;
        }

        public bool Update(Action updateAction)
        {
            updateAction?.Invoke();

            return true;
        }

        private PropertyInfo GetPrimaryKeyProperty(object x)
        {
            return x.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .First(p => p.GetCustomAttributes(typeof(Realms.PrimaryKeyAttribute), false).Count() == 1);
        }
    }
}
