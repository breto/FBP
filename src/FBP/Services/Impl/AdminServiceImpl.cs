using FBP.Dao;
using FBP.Dao.Sql;
using FBP.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace FBP.Services.Impl
{
    public class AdminServiceImpl : BaseService, IAdminService
    {
        public IFbpDao fbpDao { get; set; }

        public AdminServiceImpl(IOptions<AppSettings> appSettingsAccessor, IMemoryCache memoryCache) : base(appSettingsAccessor, memoryCache)
        {
            fbpDao = new FbpDaoSql(appSettingsAccessor);
        }

        public int executeSQL(string sql)
        {
            //todo check authorized
            return fbpDao.executeSQL(sql);
        }

        public IEnumerable<Object> executeQuery(string sql)
        {
            return fbpDao.executeQuery(sql);
        }

        public int createLeague(string name, string password)
        {
            return fbpDao.createLeague(name, password);
        }

    }
}
