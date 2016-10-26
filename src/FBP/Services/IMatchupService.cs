using FBP.Models;
using FBP.Service.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP.Service
{
    public interface IMatchupService
    {
        Task loadSchedule(string season, int week);
        void loadSeason(string season);
        IEnumerable<Matchup> getMatchupsForWeek(string season, int week);
        Matchup getMatchupByNflId(int nfl_id);
        void updateMatchups(string season, int week);
        void updateMatchups();
        Team getTeamById(int id);
        IEnumerable<Team> getAllTeams();
        Team getTeamByShortName(string shortName);
        Bracket getUsersBracketByWeek(string user_name, string season, int week, int league_id);
        void saveBracket(Bracket bracket);
        bool hasFirstGameOfWeekStarted(string season, int week);
        bool haveAllGamesStarted(string season, int week);
        League getLeagueByUserName(string name);
        IEnumerable<string> getLeagueNames();
        List<Alert> joinLeague(string leagueName, string password, string userName);
        IEnumerable<string> getLeagueMemberNames(int league_id);
        int getCurrentWeek();
        void saveMatchup(Matchup m);
        string getCurrentSeason();
        IEnumerable<UserScore> getLeaguePlayersScores(int league_id, string season, int week);
        int getNumberOfWeeksInSeason(string season);
        IEnumerable<Comment> getCommentsAll(int league_id);
        IEnumerable<Comment> getCommentsRecent(int league_id, int numberToRetrieve);
        void saveComment(Comment comment);
        int getPlayersWeekScore(string season, int week, string userName, int league_id);
        int getPlayersSeasonScore(string season, string userName, int league_id);
        int executeSQL(string sql);
        IEnumerable<Object> executeQuery(string sql);
        IEnumerable<Alert> validateBracket(Bracket b);
    }
}
