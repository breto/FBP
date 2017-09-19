using FBP.Models;
using Iuf.Apps.Services.DataAccess;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Dao.Sql
{
    public class FbpDaoSql : IFbpDao
    {
        protected SqlDataAccess db { get; set; }

        public FbpDaoSql(IOptions<AppSettings> appSettingsAccessor)
        {
            db = new SqlDataAccess(appSettingsAccessor.Value.ConnectionStringFBP);
        }
        
        public void saveMatchup(Matchup m)
        {
            Matchup exists = getMatchupByNflId(m.nfl_id);
            if (exists != null)
            {
                m.id = exists.id;
                db.Update<Matchup>(m);
            }
            else
            {
                db.Create(m);
            }
        }
        public void deleteSeason(string season)
        {
            db.Execute("delete from matchups where season = @season", new { season });
        }

        public IEnumerable<Matchup> getMatchupsForWeek(string season, int week)
        {
            IEnumerable<Matchup> matchups = db.GetList<Matchup>("select * from matchups where season = @season and week_number = @week", new { season, week});
            matchups.ToList<Matchup>().ForEach(m => setTeamsById(m));
            return matchups;
        }

        public void setTeamsById(Matchup m)
        {
            m.homeTeam = getTeamById(m.home_team_id);
            m.visitTeam = getTeamById(m.visit_team_id);
        }

        public IEnumerable<Team> getAllTeams()
        {
            IEnumerable<Team> teams = db.GetList<Team>("select * from team");
            teams.ToList<Team>().ForEach(t => setTeamRecordFields(t));
            return teams;
        }

        public Team getTeamById(int id)
        {
            Team t = db.GetSingle<Team>("select * from team where id = @id", new { id });
            setTeamRecordFields(t);
            return t;
        }

        public Team getTeamByShortName(string shortName)
        {
            Team t = db.GetSingle<Team>("select * from team where short_name = @shortName", new { shortName });
            setTeamRecordFields(t);
            return t;
        }

        public void setTeamRecordFields(Team t)
        {
            t.wins = getTeamWinCount(t.id);
            t.losses = getTeamLossCount(t.id);
            t.pointsAgainst = getTeamPointsAgainst(t.id);
            t.pointsFor = getTeamPointsFor(t.id);
        }

        public IEnumerable<Pick> getUsersPicksByWeek(string user_name, int week)
        {
            IEnumerable<Pick> picks = db.GetList<Pick>("select * from picks where user_name = @user_name and week = @week", new { user_name, week });
            picks.ToList<Pick>().ForEach(p => setMatchupByNflId(p));
            return picks;
        }

        public void setMatchupByNflId(Pick p)
        {
            p.matchup = getMatchupByNflId(p.nfl_id);
        }
        
        public Matchup getMatchupByNflId(int nfl_id)
        {
            Matchup m = db.GetFirst<Matchup>("select * from matchups where nfl_id = @nfl_id", new { nfl_id });
            if (m == null)
            {
                return null;
            }
            m.homeTeam = getTeamById(m.home_team_id);
            m.visitTeam = getTeamById(m.visit_team_id);
            return m;
        }

        public void saveBracket(Bracket bracket)
        {
            bracket.picks.ToList<Pick>().ForEach(p => savePick(p));
        }

        public Pick savePick(Pick pick)
        {
            if (pick.pick_id == 0 || "".Equals(pick.pick_id))
            {
                long l = db.Create(pick);
                pick.pick_id = unchecked((int)l); ;
                return pick;
            }
            else
            {
                bool x = db.Update(pick);
                return pick;
            }
        }

        public Matchup getFirstMatchupForWeek(string season, int week)
        {
            return db.GetSingle<Matchup>("select * from matchups x where x.id = (select min(id) from matchups a where a.season = @season and a.week_number = @week and a.game_date = (select min(b.game_date) from matchups b where a.week_number = b.week_number and a.season = b.season))", new { season, week });
        }

        public League getLeagueByUserName(string name)
        {
           return db.GetFirst<League>("select * from league where id in (select league_id from league_members where user_name = @name)", new { name});
        }

        public IEnumerable<string> getLeagueNames()
        {
            return db.GetList<string>("select name from league");
        }

        public League getLeagueByLeagueName(string name)
        {
            League l = db.GetFirst<League>("select * from league where name = @name", new { name });
            if (l != null)
            {
                l.members = getLeagueMembers(l.id);
            }
            return l;
        }

        public League getLeagueById(int id)
        {
            League l = db.GetFirst<League>("select * from league where id = @id", new { id });
            l.members = getLeagueMembers(l.id);
            return l;
        }

        public IEnumerable<LeagueMember>getLeagueMembers(int id)
        {
            return db.GetList<LeagueMember>("select * from league_members where id = @id", new { id });
        }

        public void joinLeague(string leagueName, string userName)
        {
            //TODO prevent person from joining league twice
            League l = getLeagueByLeagueName(leagueName);
            db.Create<LeagueMember>(new LeagueMember(l.id, userName));
        }

        public int getCurrentWeek()
        {
            return db.GetFirst<int>("select min(week_number) from matchups where status = 'P' and season = (select max(season) from matchups)", new { });
        }

        public IEnumerable<Matchup> getActiveMatchups(int week)
        {
            return db.GetList<Matchup>("select * from matchups where status not in ('F','FO') and week_number = @week", new { week });
        }

        public IEnumerable<Matchup> getMatchupsByWeekAndStatus(string season, int week, string[] status)
        {
            return db.GetList<Matchup>("select * from matchups where season = @season and week_number = @week and status in (@status)", new { season, week, status });
        }

        public IEnumerable<string> getLeagueMemberNames(int league_id)
        {
            return db.GetList<string>("select user_name from league_members a, league b where a.league_id = b.id and b.id = @league_id", new { league_id });
        }

        public int getTeamWinCount(int team_id)
        {
            return db.GetFirst<int>("select count(*) from matchups where win_team_id = @team_id and season = (select max(season) from matchups)", new { team_id });
        }
        public int getTeamLossCount(int team_id)
        {
            return db.GetFirst<int>("select count(*) from matchups where (home_team_id = @team_id or visit_team_id = @team_id) and status in ('F','FO') and win_team_id <> @team_id and season = (select max(season) from matchups)", new { team_id }); 
        }
        public int getTeamPointsAgainst(int team_id) {
            return 0;
        }
        public int getTeamPointsFor(int team_id)
        {
            return 0;
        }
        public int getNumberOfWeeksInSeason(string season)
        {
            return db.GetFirst<int>("select max(week_number) from matchups where season = @season", new { season });
        }
        public string getCurrentSeason()
        {
            return db.GetFirst<string>("select max(season) from matchups", new { });
        }
        public IEnumerable<Comment> getCommentsAll(int league_id)
        {
            return db.GetList<Comment>("select * from comments where league_id = @league_id order by date_posted desc", new { league_id });
        }
        public IEnumerable<Comment> getCommentsRecent(int league_id, int numberToRetrieve)
        {
            //TODO: fix numberToRetrieve
            return db.GetList<Comment>("select top 10 * from comments where league_id = @league_id order by date_posted desc", new { numberToRetrieve, league_id });
        }
        public void saveComment(Comment comment)
        {
            comment.date_posted = DateTime.Now;
            db.Create<Comment>(comment);
        }

        public int getPlayersWeekScore(string season, int week, string userName, int league_id)
        {
            return db.GetFirst<int>("select COALESCE(sum(weight),0) from picks p, matchups m where p.nfl_id = m.nfl_id and p.nfl_id in (select nfl_id from matchups where season = @season) and user_name = @userName and week = @week and p.winner_id = m.win_team_id and p.league_id = @league_id", new { season, userName, week, league_id });
        }

        public int getPlayersSeasonScore(string season, string userName, int league_id)
        {
            return db.GetFirst<int>("select COALESCE(sum(weight),0) from picks p, matchups m where p.nfl_id = m.nfl_id and p.nfl_id in (select nfl_id from matchups where season = @season) and user_name = @userName and p.winner_id = m.win_team_id and p.league_id = @league_id", new { season, userName, league_id });
        }

        public int createLeague(string name, string password)
        {
            return db.Create<League>(new League() { name = name, password = password });
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
    }
}
