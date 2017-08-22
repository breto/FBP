using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Services
{
    public interface IAdminService
    {
        int executeSQL(string sql);
        IEnumerable<Object> executeQuery(string sql);
        int createLeague(string name, string password);
    }
}
