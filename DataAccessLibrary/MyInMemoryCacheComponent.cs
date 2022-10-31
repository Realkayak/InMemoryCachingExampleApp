using Microsoft.Extensions.Caching.Memory;

namespace DataAccessLibrary
{
    public class MyInMemoryCacheComponent
    {
        private readonly Dictionary<string, DateTime> _cacheEntryLastRecentlyUsed = new();
        private readonly MemoryCache _memoryCache;
        private readonly MemoryCacheOptions _cacheOptions = new();
        private readonly double _entryAbsoluteExpirationInSeconds = 30;
        private readonly int _entrySize = 1;
        private int _cacheSizeLimit = 1;


        public MyInMemoryCacheComponent(IMemoryCache memoryCache)
        {
            _memoryCache = (MemoryCache)memoryCache;
        }

        public int SetCacheSizeLimit { set
            {
                _cacheSizeLimit = value;

                // set the maximum number of items; use a default value if none specified
                _cacheOptions.SizeLimit = _cacheSizeLimit;
            }
        }

        public void AddToCache<T>(string key, T value, int? entrySize, double? entryAbsoluteExpxirationInSeconds)
        {
            if(_memoryCache.Count == _cacheSizeLimit)
            {
                // check for 'least recently used' entries and remove as many entries as necessary
                var listOfEntries = GetLeastRecentlyUsedEntries(entrySize ?? _entrySize);
                if (listOfEntries != null)
                {
                    RemoveFromCache<T>(listOfEntries);
                }
            }            

            double entryAbsExpInSeconds = entryAbsoluteExpxirationInSeconds ?? _entryAbsoluteExpirationInSeconds;

            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                Size = entrySize ?? _entrySize
                , AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(entryAbsExpInSeconds)
            };

            cacheEntryOptions.RegisterPostEvictionCallback(CacheEntryEvicted);

            _memoryCache.Set<T>(key, value, cacheEntryOptions);

            // update dictionary which stores LastRecentlyUsed data
            _cacheEntryLastRecentlyUsed.Add(key, DateTime.UtcNow);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            value = default(T);

            if(_memoryCache.TryGetValue(key, out T obj))
            {
                // object found in cache
                value = obj;

                // update dictionary which stores LastRecentlyUsed data
                _cacheEntryLastRecentlyUsed[key] = DateTime.UtcNow;

                return true;
            }

            // object not found in cache
            return false;
        }

        private void RemoveFromCache<T>(List<string> entryKeys)
        {
            foreach (var entryKey in entryKeys)
            {
                _memoryCache?.Remove(entryKey);

                // throw event to alert of cache entry removal
                CacheItemEvictedEventArgs args = new();
                args.EvictedKey = entryKey;

                OnCacheItemEvicted(args);
            }
        }

        private List<string>? GetLeastRecentlyUsedEntries(int entrySize)
        {
            // sort cache keys, by date in ascending order and get the number of items needing
            var orderedEntryList = _cacheEntryLastRecentlyUsed?.OrderBy(x => x.Value).Take(entrySize).Select(x => x.Key).ToList();

            return orderedEntryList;
        }

        public event EventHandler<CacheItemEvictedEventArgs>? CacheItemEvicted;

        public class CacheItemEvictedEventArgs : EventArgs
        {
            public string? EvictedKey { get; set; }
        }

        protected virtual void OnCacheItemEvicted(CacheItemEvictedEventArgs e)
        {
            CacheItemEvicted?.Invoke(this, e);
        }
        
        private void CacheEntryEvicted(object key, object value, EvictionReason reason, object state)
        {
            var rKey = key.ToString() ?? "";

            switch (reason)
            {
                case EvictionReason.Removed:
                    if (_memoryCache.TryGetValue(key.ToString(), out _) == false)
                    {
                        // update dictionary which stores LastRecentlyUsed data
                        _cacheEntryLastRecentlyUsed?.Remove(rKey);
                    }
                    break;
                case EvictionReason.Expired:
                    // update dictionary which stores LastRecentlyUsed data
                    _cacheEntryLastRecentlyUsed?.Remove(rKey);
                    break;
                default:
                    break;
            }
        }
    }
}
