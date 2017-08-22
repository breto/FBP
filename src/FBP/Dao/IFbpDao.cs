using FBP.Models;
using Iuf.Apps.Services.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Dao
{
    public interface IFbpDao
    {
        void saveMatchup(Matchup m);
        
        IEnumerable<Matchup> getMatchupsForWeek(string season, int week);
        IEnumerable<Matchup> getMatchupsByWeekAndStatus(string season, int week, string[] status);
        IEnumerable<Matchup> getActiveMatchups(int week);
        Matchup getFirstMatchupForWeek(string season, int week);
        Matchup getMatchupByNflId(int nfl_id);

        Team getTeamById(int id);
        IEnumerable<Team> getAllTeams();
        Team getTeamByShortName(string shortName);
        IEnumerable<Pick> getUsersPicksByWeek(string user_name, int week);
        
        
        League getLeagueByUserName(string name);
        League getLeagueByLeagueName(string leagueName);
        League getLeagueById(int id);
        IEnumerable<string> getLeagueNames();
        void joinLeague(string leagueName, string userName);
        IEnumerable<string> getLeagueMemberNames(int league_id);

        int getCurrentWeek();
        int getTeamWinCount(int team_id);
        int getTeamLossCount(int team_id);
        int getTeamPointsAgainst(int team_id);
        int getTeamPointsFor(int team_id);
        int getNumberOfWeeksInSeason(string season);
        string getCurrentSeason();
        int getPlayersWeekScore(string season, int week, string userName, int league_id);
        int getPlayersSeasonScore(string season, string userName, int league_id);
        void saveBracket(Bracket bracket);
        Pick savePick(Pick pick);
        void deleteSeason(string season);

        IEnumerable<Comment> getCommentsAll(int league_id);
        IEnumerable<Comment> getCommentsRecent(int league_id, int numberToRetrieve);
        void saveComment(Comment comment);
        int createLeague(string name, string password);

        int executeSQL(string sql);
        IEnumerable<Object> executeQuery(string sql);

    }
}
