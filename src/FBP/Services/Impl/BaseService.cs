using Iuf.Apps.Services.DataAccess;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Services.Impl
{
    public abstract class BaseService
    {
        protected IMemoryCache cache { get; set; }
        
        protected BaseService(IOptions<AppSettings> appSettingsAccessor, IMemoryCache memoryCache)
        {
            cache = memoryCache;
        }
    }
}
