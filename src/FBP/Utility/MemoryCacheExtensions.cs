using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Utility
{
    public static class MemoryCacheExtensions
    {
        public static T Get<T>(this IMemoryCache cache, Object key, MemoryCacheEntryOptions options, Func<T> f)
        {
            T o;
            if (!cache.TryGetValue(key, out o)){
                o = f();
                cache.Set(key, o, options);
            }
            return o;
        }

        public static T Get<T>(this IMemoryCache cache, Object key, Func<T> f)
        {
            T o;
            if (!cache.TryGetValue(key, out o))
            {
                o = f();
                cache.Set(key, o);
            }
            return o;
        }
    }
}

