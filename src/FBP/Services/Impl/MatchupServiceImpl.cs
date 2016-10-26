﻿using FBP.Models;
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

namespace FBP.Service.Impl
{
    public class MatchupServiceImpl : BaseService, IMatchupService
    {
        public IMatchupDao matchupDao { get; set; }

        public MatchupServiceImpl(IOptions<AppSettings> appSettingsAccessor, IMemoryCache memoryCache) : base(appSettingsAccessor, memoryCache)
        {
            matchupDao = new MatchupDaoSql();
        }

        public void saveBracket(Bracket bracket)
        {
            List<Pick> picks = bracket.picks.ToList<Pick>();
            foreach (var p in picks)
            {
                if ("P".Equals(p.matchup.status))
                {
                    matchupDao.savePick(p, db);
                }
            }
            matchupDao.saveBracket(bracket, db);
        }

        public Bracket getUsersBracketByWeek(string user_name, string season, int week, int league_id)
        {
            Bracket bracket = new Bracket(user_name, week, league_id);
            IEnumerable<Pick> picks = matchupDao.getUsersPicksByWeek(user_name, week, db);
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
            IEnumerable<Matchup> l = matchupDao.getMatchupsByWeekAndStatus(season, week, new string[] { "P" }, db);
            return l == null || l.Count() == 0;
        }

        public bool hasFirstGameOfWeekStarted(string season, int week)
        {
            Matchup first = matchupDao.getFirstMatchupForWeek(season, week, db);
            
            if(first.game_date.CompareTo(DateTime.Now) < 1)
            {
                return true;
            } else
            {
                return false;
            }
        }
        public IEnumerable<Pick> getBlankPicksForWeek(string user_name, int league_id, int week)
        {
            //TODO: change to get currentweek
            IEnumerable<Matchup> matchups = getMatchupsForWeek("2016", week);
            List<Pick> picks = new List<Pick>();
            matchups.ToList<Matchup>().ForEach(m => picks.Add(new Pick(user_name, league_id, m)));
            return picks;
        }

        public IEnumerable<Matchup> getMatchupsForWeek(string season, int week)
        {
            return cache.Get("MatchupsForWeek" + season + week, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)), () => matchupDao.getMatchupsForWeek(season, week, db));
        }

        public int getNumberOfWeeksInSeason(string season)
        {
            return cache.Get("GamesInSeason", new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(50)), () => matchupDao.getNumberOfWeeksInSeason(season, db));
        }

        public async void loadSeason(string season)
        {
            matchupDao.deleteSeason(season, db);
            for(int i=1; i<18; i++)
            {
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
            //using (var transactionScope = new TransactionScope())
            //{
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
                    matchupDao.saveMatchup(m, db);
                //}
                //transactionScope.Complete();
            }
        }

        public IEnumerable<string> getLeagueMemberNames(int league_id)
        {
            return matchupDao.getLeagueMemberNames(league_id, db);
        }

        public IEnumerable<UserScore> getLeaguePlayersScores(int league_id, string season, int week)
        {
            IEnumerable<string> leagueMemberNames = getLeagueMemberNames(league_id);
            List<UserScore> playerScores = new List<UserScore>();
            leagueMemberNames.ToList<string>().ForEach(s => playerScores.Add(new UserScore(s, getPlayersWeekScore(season, week, s, league_id), getPlayersSeasonScore(season, s, league_id))));
            playerScores.Sort((x,y) => x.weekScore.CompareTo(y.weekScore));
            playerScores.Reverse();
            return playerScores;
        }

        public int getPlayersWeekScore(string season, int week, string userName, int league_id)
        {
            return matchupDao.getPlayersWeekScore(season, week, userName, league_id, db);
        }

        public int getPlayersSeasonScore(string season, string userName, int league_id)
        {
            return matchupDao.getPlayersSeasonScore(season, userName, league_id, db);
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
            return cache.Get("CurrentSeason", new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(50)), () => matchupDao.getCurrentSeason(db));
        }

        public bool isWeekStillActive(int week)
        {
            IEnumerable<Matchup> l = cache.Get("WeekStillActive" + week, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1)), () => matchupDao.getActiveMatchups(week, db));
            return (l == null || !l.Any()) ? false : true;
        }

        public Team getTeamByShortName(string shortName)
        {
            return matchupDao.getTeamByShortName(shortName, db);
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
            return cache.Get("TeamById" + id, () => matchupDao.getTeamById(id, db));
        }

        public IEnumerable<Team> getAllTeams()
        {
            return cache.Get("AllTeams", () => matchupDao.getAllTeams(db));
        }

        public League getLeagueByUserName(string name)
        {
            return matchupDao.getLeagueByUserName(name, db);
        }

        public IEnumerable<string> getLeagueNames()
        {
            return matchupDao.getLeagueNames(db);
        }

        public List<Alert> joinLeague(string leagueName, string password, string userName)
        {
            League l = matchupDao.getLeagueByLeagueName(leagueName, db) ;
            List<Alert> errors = new List<Alert>();
            if(l != null)
            {
                if (!l.password.Equals(password))
                {
                    errors.Add(new Alert(Alert.DANGER_TYPE, "Invalid password"));
                }
                else
                {
                    matchupDao.joinLeague(leagueName, userName, db);
                }
            } else
            {
                errors.Add(new Alert(Alert.DANGER_TYPE, "Could not find league " + leagueName));
            }
            return errors;
        }

        public int getCurrentWeek()
        {
            return cache.Get("CurrentWeek", new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10)), () => matchupDao.getCurrentWeek(db));
        }
        
        public Matchup getMatchupByNflId(int nfl_id)
        {
            return cache.Get("MatchupByNflId" + nfl_id, () => matchupDao.getMatchupByNflId(nfl_id, db));
        }

        public void saveMatchup(Matchup m)
        {
            matchupDao.saveMatchup(m, db);
        }

        public IEnumerable<Comment> getCommentsAll(int league_id) {
            return matchupDao.getCommentsAll(league_id, db);
        }
        public IEnumerable<Comment> getCommentsRecent(int league_id, int numberToRetrieve)
        {
            if(numberToRetrieve < 0)
            {
                numberToRetrieve = 10;
            }
            return matchupDao.getCommentsRecent(league_id, numberToRetrieve, db);
        }
        public void saveComment(Comment comment)
        {
            matchupDao.saveComment(comment, db);
        }
        public int executeSQL(string sql)
        {
            //todo check authorized
            return db.Execute(sql, new { });
        }

        public IEnumerable<Object> executeQuery(string sql)
        {
            return db.GetList<Object>(sql);
        }

        public IEnumerable<Alert> validateBracket(Bracket b)
        {
            HashSet<Alert> errors = new HashSet<Alert>();
            HashSet<int> weights = new HashSet<int>();
            //if (matchupService.hasFirstGameOfWeekStarted(b.week))
            //{
            //    b.hasWeekStarted = false;
            //    errors.Add(new Alert(Alert.DANGER_TYPE, "Picks cant be saved - week " + b.week + " has already started."));
            //}
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
                if (!"P".Equals(p.matchup.status))
                {
                    errors.Add(new Alert(Alert.WARNING_TYPE, p.matchup.homeTeam.short_name + " game cant be saved b/c it already started."));
                }
            }
            return errors;
        }

    }
}
