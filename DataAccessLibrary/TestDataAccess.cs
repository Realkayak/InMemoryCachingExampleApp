using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public class TestDataAccess
    {
        private readonly MyInMemoryCacheComponent _myInMemoryCache;

        public TestDataAccess(MyInMemoryCacheComponent myInMemoryCache)
        {
            _myInMemoryCache = myInMemoryCache;
            _myInMemoryCache.SetCacheSizeLimit = 3;
        }

        public async Task<string> AddRoleCacheEntry()
        {
            string added;

            var cacheExists = _myInMemoryCache.TryGetValue<string>("role", out added);

            if (!cacheExists)
            {
                added = "data";
                await Task.Delay(3000);

                _myInMemoryCache.AddToCache<string>("role", added, null, null);
            }

            return added;
        }

        public async Task<int> AddDepartmentCacheEntry()
        {
            int added;

            var cacheExists = _myInMemoryCache.TryGetValue<int>("department", out added);

            if (!cacheExists)
            {
                added = 1024;
                await Task.Delay(3000);

                _myInMemoryCache.AddToCache<int>("department", added, null, null);
            }

            return added;
        }

        public async Task<object> AddEmployeeCacheEntry()
        {
            object added;

            var cacheExists = _myInMemoryCache.TryGetValue<object>("employee", out added);

            if (!cacheExists)
            {
                added = new();
                await Task.Delay(3000);

                _myInMemoryCache.AddToCache<object>("employee", added, null, null);
            }

            return added;
        }

        public async Task<object> AddAccountCacheEntry()
        {
            object added;

            var cacheExists = _myInMemoryCache.TryGetValue<object>("account", out added);

            if (!cacheExists)
            {
                added = new();
                    await Task.Delay(3000);

                    _myInMemoryCache.AddToCache<object>("account", added, null, null);
            }

            return added;
        }
    }
}
