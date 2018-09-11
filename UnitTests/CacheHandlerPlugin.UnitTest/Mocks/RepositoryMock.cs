using System;
using System.Collections.Generic;
using CacheHandlerPlugin.Realm.Services.Repository;

namespace CacheHandlerPlugin.UnitTest.Mocks
{
    public class RepositoryMock : IRealmRepository
    {
        public bool Add<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> All<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public T Find<T>(string primaryKey) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Remove<T>(string key) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Remove<T>(T entity) where T : class
        {
            throw new NotImplementedException();
        }

        public bool RemoveAll<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public bool RemoveAll()
        {
            throw new NotImplementedException();
        }

        public bool Update<T>(string primaryKey, Action<T> updateAction) where T : class
        {
            throw new NotImplementedException();
        }

        public bool Update(Action updateAction)
        {
            throw new NotImplementedException();
        }
    }
}
