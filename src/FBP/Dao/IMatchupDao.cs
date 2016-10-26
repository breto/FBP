using FBP.Models;
using Iuf.Apps.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Dao
{
    public interface IMatchupDao
    {
        void saveMatchup(Matchup m, SqlDataAccess db);
        
        IEnumerable<Matchup> getMatchupsForWeek(string season, int week, SqlDataAccess db);
        IEnumerable<Matchup> getMatchupsByWeekAndStatus(string season, int week, string[] status, SqlDataAccess db);
        IEnumerable<Matchup> getActiveMatchups(int week, SqlDataAccess db);
        Matchup getFirstMatchupForWeek(string season, int week, SqlDataAccess db);
        Matchup getMatchupByNflId(int nfl_id, SqlDataAccess db);

        Team getTeamById(int id, SqlDataAccess db);
        IEnumerable<Team> getAllTeams(SqlDataAccess db);
        Team getTeamByShortName(string shortName, SqlDataAccess db);
        IEnumerable<Pick> getUsersPicksByWeek(string user_name, int week, SqlDataAccess db);
        
        
        League getLeagueByUserName(string name, SqlDataAccess db);
        League getLeagueByLeagueName(string leagueName, SqlDataAccess db);
        League getLeagueById(int id, SqlDataAccess db);
        IEnumerable<string> getLeagueNames(SqlDataAccess db);
        void joinLeague(string leagueName, string userName, SqlDataAccess db);
        IEnumerable<string> getLeagueMemberNames(int league_id, SqlDataAccess db);

        int getCurrentWeek(SqlDataAccess db);
        int getTeamWinCount(int team_id, SqlDataAccess db);
        int getTeamLossCount(int team_id, SqlDataAccess db);
        int getTeamPointsAgainst(int team_id, SqlDataAccess db);
        int getTeamPointsFor(int team_id, SqlDataAccess db);
        int getNumberOfWeeksInSeason(string season, SqlDataAccess db);
        string getCurrentSeason(SqlDataAccess db);
        int getPlayersWeekScore(string season, int week, string userName, int league_id, SqlDataAccess db);
        int getPlayersSeasonScore(string season, string userName, int league_id, SqlDataAccess db);
        void saveBracket(Bracket bracket, SqlDataAccess db);
        Pick savePick(Pick pick, SqlDataAccess db);
        void deleteSeason(string season, SqlDataAccess db);

        IEnumerable<Comment> getCommentsAll(int league_id, SqlDataAccess db);
        IEnumerable<Comment> getCommentsRecent(int league_id, int numberToRetrieve, SqlDataAccess db);
        void saveComment(Comment comment, SqlDataAccess db);
        
    }
}
