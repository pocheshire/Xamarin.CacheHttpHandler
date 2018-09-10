using System;
using System.Collections.Generic;

namespace CacheHandlerPlugin.Realm.Services.Repository
{
    public interface IRealmRepository
    {
        bool Add<T>(T entity) where T : class;

        bool Update<T>(string primaryKey, Action<T> updateAction) where T : class;

        bool Update(Action updateAction);

        T Find<T>(string primaryKey) where T : class;

        IEnumerable<T> All<T>() where T : class;

        bool Remove<T>(string key) where T : class;

        bool Remove<T>(T entity) where T : class;

        bool RemoveAll<T>() where T : class;

        bool RemoveAll();
    }
}
