using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FBP.ViewModels;
using FBP.Service;
using Microsoft.AspNetCore.Authorization;

namespace FBP.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        [PropertyFromServices]
        public IMatchupService matchupService { get; set; }

        public IActionResult Index()
        {
            return View("Admin");
        }

        [HttpPost("admin/exec", Name = "ExecuteSql")]
        public IActionResult executeSql(AdminViewModel avm)
        {
            int x = matchupService.executeSQL(avm.sqlCommand);
            return View("Admin");
        }

        [HttpPost("admin/execQuery", Name = "ExecuteQuery")]
        public IActionResult executeQuery(AdminViewModel avm)
        {
            avm.results = matchupService.executeQuery(avm.sqlCommand);
            return View("Admin", avm);
        }

        [HttpPost("admin/loadMatchups", Name = "LoadMatchups")]
        public IActionResult loadMatchups(AdminViewModel avm)
        {
            if (avm.week <= 0)
            {
                matchupService.loadSeason(avm.season);
            } else
            {
                matchupService.loadSchedule(avm.season, avm.week);
            }
            
            return View("Admin");
        }
    }
}