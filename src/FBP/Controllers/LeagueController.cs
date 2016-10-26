using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FBP.ViewModels;
using FBP.Models;
using FBP.Service;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace FBP.Controllers
{
    public class LeagueController : Controller
    {
        [PropertyFromServices]
        public IMatchupService matchupService { get; set; }

        [HttpGet("league/index", Name = "Index")]
        public IActionResult Index()
        {
            return View("JoinLeagueView");
        }

        [HttpPost("league/joinLeague", Name = "JoinLeague")]
        public IActionResult JoinLeague(string leagueName, string leaguePassword)
        {
            string userName = User.Identity.Name;
            FootballPoolViewModel vm = new FootballPoolViewModel();
            List<Alert> errors = matchupService.joinLeague(leagueName, leaguePassword, userName);
            if (errors.ToList().Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                vm.errors = errors;
                League league = matchupService.getLeagueByUserName(userName);
                return View("JoinLeagueView", vm);
            }
        }

    }
}
