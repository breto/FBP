using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using FBP;
using FBP.Services;
using FBP.Service;
using FBP.ViewModels;
using FBP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using FBP.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;

namespace FBP.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [PropertyFromServices]
        public IMatchupService matchupService { get; set; }

        [PropertyFromServices]
        public ILogger<FbpLogger> Logger { get; set; }

        public IActionResult Index()
        {
            return RedirectToAction("GetWeek", new { week = matchupService.getCurrentWeek()});
        }

        [HttpGet("get-week", Name = "GetWeek")]
        public IActionResult GetWeek(int week)
        {
            FootballPoolViewModel vm = new FootballPoolViewModel();
            string name = User.Identity.Name;
            League league = matchupService.getLeagueByUserName(name);
            if (league == null)
            {
                return RedirectToAction("Index", "League");
            }
            vm.weeksInSeason = matchupService.getNumberOfWeeksInSeason(matchupService.getCurrentSeason());
            week = week <= 0 ? 1 : week > vm.weeksInSeason ? vm.weeksInSeason : week;
            vm.bracket = matchupService.getUsersBracketByWeek(name, matchupService.getCurrentSeason(), week, league.id);
            vm.teams = matchupService.getAllTeams();
            vm.currentWeek = matchupService.getCurrentWeek();
            vm.comments = matchupService.getCommentsRecent(vm.bracket.league_id, 10);
            vm.userLeague = league;
            return View("PicksView", vm);
        }

        [HttpPost("home/saveBracket", Name = "SaveBracket")]
        public FootballPoolViewModel saveBracket([FromBody] Bracket bracket)
        {
            List<Alert> errors = matchupService.validateBracket(bracket).ToList();
            if(errors.Count == 0)
            {
                matchupService.saveBracket(bracket, true);
            } else
            {
                bool hasErrors = false;
                foreach (Alert a in errors)
                {
                    if (Alert.DANGER_TYPE.Equals(a.type))
                    {
                        hasErrors = true;
                        break;
                    }
                }
                if (!hasErrors)
                {
                    matchupService.saveBracket(bracket, true);
                }
            }
            FootballPoolViewModel vm = new FootballPoolViewModel();
            vm.errors = errors;
            vm.bracket = bracket;
            return vm;
        }

        [HttpPost("home/saveComment", Name = "SaveComment")]
        public void saveComment([FromBody] Comment comment)
        {
            comment.user_name = User.Identity.Name;
            matchupService.saveComment(comment);
        }

        [HttpGet("get-comments")]
        public IEnumerable<Comment> GetCommentsAjax(int league_id, bool show_all)
        {
            return show_all ? matchupService.getCommentsAll(league_id) : matchupService.getCommentsRecent(league_id, 10);
        }

        [HttpGet("league-names-ajax")]
        public IEnumerable<SelectListItem> GetLeagueNamesAjax()
        {
            return matchupService.getLeagueNames().Select(a => new SelectListItem { Text = a, Value = a }).OrderBy(a => a.Text);
        }

        [HttpGet("league-members-ajax")]
        public IEnumerable<string> GetLeagueMembersAjax(int league_id)
        {
            return matchupService.getLeagueMemberNames(league_id).OrderBy(a => a);
        }

        [HttpGet("matchups-ajax", Name = "GetMatchupsAjax")]
        public IEnumerable<Matchup> getMatchupsAjax(int week)
        {
            return matchupService.getMatchupsForWeek(matchupService.getCurrentSeason(), week);
        }

        [HttpGet("comments-ajax", Name = "GetCommentsAjax")]
        public IEnumerable<Comment> getComments(int league_id)
        {
            return matchupService.getCommentsRecent(league_id, -1);
        }

        [HttpGet("players-scores-ajax", Name = "GetPlayersScoresAjax")]
        public IEnumerable<UserScore> getPlayersScoresAjax(int week, int league_id)
        {
            return matchupService.getLeaguePlayersScores(league_id, matchupService.getCurrentSeason(), week);
        }

        [HttpGet("get-week-ajax", Name = "GetWeekAjax")]
        public FootballPoolViewModel getWeekAjax(int week, int league_id)
        {
            FootballPoolViewModel vm = new FootballPoolViewModel();
            string name = User.Identity.Name;
            vm.weeksInSeason = matchupService.getNumberOfWeeksInSeason(matchupService.getCurrentSeason());
            week = week <= 0 ? 1 : week > vm.weeksInSeason ? vm.weeksInSeason : week;
            vm.bracket = matchupService.getUsersBracketByWeek(name, matchupService.getCurrentSeason(), week, league_id);
            vm.teams = matchupService.getAllTeams();
            vm.currentWeek = matchupService.getCurrentWeek();
            return vm;
        }

        [HttpGet("get-week-for-player", Name = "GetWeekForPlayer")]
        public Bracket GetWeekForPlayer(int week, string name, int league_id)
        {
            return matchupService.getUsersBracketByWeek(name, matchupService.getCurrentSeason(), week, league_id);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        [AllowAnonymous]
        [Route("error/{statusCode:max(600)}")]
        public IActionResult Error(int statusCode)
        {
            return View();
        }
        public IActionResult Error()
        {
            var e = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
            Logger.LogError(new EventId(), e, "Caught an exception", new { });
            return View();
        }
    }
}
