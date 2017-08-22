using FBP.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;
using System.Collections.Generic;
using System.Xml;
using FBP.Services;
using FBP.Services.Impl;
using FBP.Dao;
using Microsoft.Extensions.Options;
using FBP.Dao.Sql;
using System.Net.Http;
using System.Linq;
using System.Threading.Tasks;
using FBP.Utility;
using System.Collections.ObjectModel;

namespace FBP.Service.Impl
{
    public class MatchupServiceImpl : BaseService, IMatchupService
    {
        public IFbpDao fbpDao { get; set; }

        public MatchupServiceImpl(IOptions<AppSettings> appSettingsAccessor, IMemoryCache memoryCache) : base(appSettingsAccessor, memoryCache)
        {
            fbpDao = new FbpDaoSql(appSettingsAccessor);
        }

        public void saveBracket(Bracket bracket, bool onlyPending)
        {
            List<Pick> picks = bracket.picks.ToList<Pick>();
            foreach (var p in picks)
            {
                p.matchup = getMatchupByNflId(p.nfl_id);
                if (!onlyPending || !p.matchup.game_has_started)
                {
                    fbpDao.savePick(p);
                }
            }
        }

        public Bracket getUsersBracketByWeek(string user_name, string season, int week, int league_id)
        {
            Bracket bracket = new Bracket(user_name, week, league_id);
            IEnumerable<Pick> picks = fbpDao.getUsersPicksByWeek(user_name, week);
            List<Pick> pickList = picks.ToList<Pick>();
            if (pickList != null && pickList.Count != 0)
            {
                bracket.picks = picks;
            } else
            {
                bracket.picks = getBlankPicksForWeek(user_name, league_id, week);
            }
            bracket.hasWeekStarted = hasFirstGameOfWeekStarted(season, week);
            bracket.allGamesStarted = haveAllGamesStarted(season, week);
            return bracket;
        }

        public bool haveAllGamesStarted(string season, int week)
        {
            IEnumerable<Matchup> l = fbpDao.getMatchupsByWeekAndStatus(season, week, new string[] { "P" });
            return l == null || l.Count() == 0;
        }

        public bool hasFirstGameOfWeekStarted(string season, int week)
        {
            Matchup first = fbpDao.getFirstMatchupForWeek(season, week);

            DateTime est = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            
            if (first.game_date.CompareTo(est) < 1)
            {
                return true;
            } else
            {
                return false;
            }
        }
        public IEnumerable<Pick> getBlankPicksForWeek(string user_name, int league_id, int week)
        {
            IEnumerable<Matchup> matchups = getMatchupsForWeek(getCurrentSeason(), week);
            List<Pick> picks = new List<Pick>();
            matchups.ToList<Matchup>().ForEach(m => picks.Add(new Pick(user_name, league_id, m)));
            return picks;
        }

        public IEnumerable<Matchup> getMatchupsForWeek(string season, int week)
        {
            return cache.Get("MatchupsForWeek" + season + week, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)), () => fbpDao.getMatchupsForWeek(season, week));
        }

        public int getNumberOfWeeksInSeason(string season)
        {
            return cache.Get("GamesInSeason", new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(50)), () => fbpDao.getNumberOfWeeksInSeason(season));
        }

        public async void loadSeason(string season)
        {
            fbpDao.deleteSeason(season);
            for(int i=1; i<18; i++)
            {
                System.Diagnostics.Debug.WriteLine("Loading " + i.ToString());
                await loadSchedule(season, i);
            }
        }

        public async Task loadSchedule(string season, int week)
        {
            HttpClient hc = new HttpClient();
            
            XmlDocument doc = new XmlDocument();
            String docString =  await hc.GetStringAsync("http://www.nfl.com/ajax/scorestrip?seasonType=REG&season=" + season + "&week=" + week);
            System.Diagnostics.Debug.WriteLine("http://www.nfl.com/ajax/scorestrip?seasonType=REG&season=" + season + "&week=" + week);
            doc.LoadXml(docString);
            XmlNodeList list = doc.GetElementsByTagName("g");
            foreach (XmlNode gameNode in list)
            {
                    
                Matchup m = new Matchup();
                m.season = season;
                m.week_number = week;

                XmlAttributeCollection xac = gameNode.Attributes;
                m.game_date = getDateTimeFromNflString(xac.GetNamedItem("eid").Value, xac.GetNamedItem("t").Value);
                m.home_team_id = getTeamByShortName(xac.GetNamedItem("h").Value).id;
                m.visit_team_id = getTeamByShortName(xac.GetNamedItem("v").Value).id;
                string n = xac.GetNamedItem("h").Value;
                m.visit_team_score = int.Parse(xac.GetNamedItem("vs").Value.Equals("") ? "-1" : xac.GetNamedItem("vs").Value);
                m.home_team_score = int.Parse(xac.GetNamedItem("hs").Value.Equals("") ? "-1" : xac.GetNamedItem("hs").Value);
                m.nfl_id = int.Parse(xac.GetNamedItem("eid").Value);
                m.status = xac.GetNamedItem("q").Value;

                if (m.home_team_score > m.visit_team_score)
                {
                    m.win_team_id = m.home_team_id;
                }
                else if (m.home_team_score < m.visit_team_score)
                {
                    m.win_team_id = m.visit_team_id;
                }
                else
                {
                    m.win_team_id = Matchup.TIE_INDICATOR;
                }
                fbpDao.saveMatchup(m);
                System.Diagnostics.Debug.WriteLine("Saved season" + m.season + " week" + m.week_number + " nfl id" + m.nfl_id);
            }
        }

        public IEnumerable<string> getLeagueMemberNames(int league_id)
        {
            return fbpDao.getLeagueMemberNames(league_id);
        }

        public IEnumerable<UserScore> getLeaguePlayersScores(int league_id, string season, int week)
        {
            IEnumerable<string> leagueMemberNames = getLeagueMemberNames(league_id);
            List<UserScore> playerScores = new List<UserScore>();
            leagueMemberNames.ToList<string>().ForEach(s => playerScores.Add(new UserScore(s, getPlayersWeekScore(season, week, s, league_id), getPlayersSeasonScore(season, s, league_id), week, league_id)));
            playerScores.Sort((x,y) => x.weekScore.CompareTo(y.weekScore));
            playerScores.Reverse();
            return playerScores;
        }

        public int getPlayersWeekScore(string season, int week, string userName, int league_id)
        {
            return fbpDao.getPlayersWeekScore(season, week, userName, league_id);
        }

        public int getPlayersSeasonScore(string season, string userName, int league_id)
        {
            return fbpDao.getPlayersSeasonScore(season, userName, league_id);
        }

        public void updateMatchups(string season, int week)
        {
            if (isWeekStillActive(week))
            {
                loadSchedule(season, week);
            }
        }

        public void updateMatchups()
        {
            updateMatchups(getCurrentSeason(), getCurrentWeek());
        }

        public string getCurrentSeason()
        {
            return cache.Get("CurrentSeason", new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(50)), () => fbpDao.getCurrentSeason());
        }

        public bool isWeekStillActive(int week)
        {
            IEnumerable<Matchup> l = cache.Get("WeekStillActive" + week, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)), () => fbpDao.getActiveMatchups(week));
            return (l == null || !l.Any()) ? false : true;
        }

        public Team getTeamByShortName(string shortName)
        {
            return fbpDao.getTeamByShortName(shortName);
        }

        public DateTime getDateTimeFromNflString(string date, string time)
        {
            date = date.Substring(0, 8);
            int year = int.Parse(date.Substring(0, 4));
            int month = int.Parse(date.Substring(4, 2));
            int day = int.Parse(date.Substring(6, 2));

            int militaryTimeHour = (int.Parse(time.Substring(0, time.IndexOf(":"))) + (int.Parse(time.Substring(0, time.IndexOf(":"))) == 12 ? 0 : 12));
            int minute = int.Parse(time.Substring(time.IndexOf(":") + 1));


            DateTime dt = new DateTime(year, month, day, militaryTimeHour, minute, 0);
            return dt;
        }

        public Team getTeamById(int id)
        {
            return cache.Get("TeamById" + id, () => fbpDao.getTeamById(id));
        }

        public IEnumerable<Team> getAllTeams()
        {
            return cache.Get("AllTeams", () => fbpDao.getAllTeams());
        }
        public League getLeagueById(int id)
        {
            return fbpDao.getLeagueById(id);
        }
        public League getLeagueByUserName(string name)
        {
            return fbpDao.getLeagueByUserName(name);
        }

        public IEnumerable<string> getLeagueNames()
        {
            return fbpDao.getLeagueNames();
        }

        public List<Alert> joinLeague(string leagueName, string password, string userName)
        {
            League l = fbpDao.getLeagueByLeagueName(leagueName) ;
            List<Alert> errors = new List<Alert>();
            if(l != null)
            {
                if (!l.password.Equals(password))
                {
                    errors.Add(new Alert(Alert.DANGER_TYPE, "Invalid password"));
                }
                else
                {
                    fbpDao.joinLeague(leagueName, userName);
                }
            } else
            {
                errors.Add(new Alert(Alert.DANGER_TYPE, "Could not find league " + leagueName));
            }
            return errors;
        }

        public int getCurrentWeek()
        {
            return cache.Get("CurrentWeek", new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)), () => fbpDao.getCurrentWeek());
        }
        
        public Matchup getMatchupByNflId(int nfl_id)
        {
            return cache.Get("MatchupByNflId" + nfl_id, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)), () => fbpDao.getMatchupByNflId(nfl_id));
        }

        public void saveMatchup(Matchup m)
        {
            fbpDao.saveMatchup(m);
        }

        public IEnumerable<Comment> getCommentsAll(int league_id) {
            return fbpDao.getCommentsAll(league_id);
        }
        public IEnumerable<Comment> getCommentsRecent(int league_id, int numberToRetrieve)
        {
            if(numberToRetrieve < 0)
            {
                numberToRetrieve = 10;
            }
            return fbpDao.getCommentsRecent(league_id, numberToRetrieve);
        }
        public void saveComment(Comment comment)
        {
            fbpDao.saveComment(comment);
        }
        
        public IEnumerable<Alert> validateBracket(Bracket b)
        {
            HashSet<Alert> errors = new HashSet<Alert>();
            HashSet<int> weights = new HashSet<int>();
            int maxWeight = b.picks.ToArray().Length;

            foreach (Pick p in b.picks)
            {
                if (0 != p.weight && weights.Contains(p.weight))
                {
                    errors.Add(new Alert(Alert.DANGER_TYPE, "Duplicate score: " + p.weight));
                }
                else
                {
                    weights.Add(p.weight);
                }
                p.matchup = getMatchupByNflId(p.nfl_id);
                if (p.matchup.game_has_started)
                {
                    errors.Add(new Alert(Alert.WARNING_TYPE, p.matchup.homeTeam.short_name + " game cant be saved b/c it already started."));
                }
                if(p.weight > maxWeight)
                {
                    errors.Add(new Alert(Alert.DANGER_TYPE, "Invalid weight: " + p.weight));
                }
            }
            return errors;
        }

    }
}
