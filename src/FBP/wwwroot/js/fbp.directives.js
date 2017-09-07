(function () {
    'use strict';

    app

        .directive('commentSection', function ($http) {
            return {
                scope: {
                    leagueid: '@',
                    refreshrate: '@',
                },
                link: link,
                template: '<div class="col-md-4">' +
                '<table class="table table-sm table-hover">' +
                '    <thead>' +
                '        <tr>' +
                '            <th>Player</th>' +
                '            <th>Comment</th>' +
                '        </tr>' +
                '    </thead>' +
                '        <tr ng-repeat="c in comments">' +
                '            <td>{{c.user_name}}</td>' +
                '            <td>{{c.comment}}</td>' +
                '        </tr>' +
                '</table>' +
                '<form name="commentForm">' +
                '    <textarea ng-model="commentToAdd" class="comment-textarea" ng-minlength="1" ng-maxlength="200"></textarea>' +
                '    <button class="btn btn-primary" ng-click="saveComment(showAllComments)">Add Comment</button>' +
                '    <button class="btn btn-primary" ng-click="getComments(showAllComments)">Refresh</button>' +
                '    <button class="btn btn-primary" ng-click="getComments(true)" ng-show="!showAllComments">All</button>' +
                '    <button class="btn btn-primary" ng-click="getComments(false)" ng-show="showAllComments">Recent</button>' +
                '</form>' +
                '</div>'
            }

            function link(scope, elem, attrs) {
                scope.showAllComments = false;
                scope.updateComments = function () {
                    $http.get('/get-comments?league_id=' + scope.leagueid + '&show_all= ' + scope.showAllComments).success(function (response) {
                        scope.comments = response;
                    });
                }

                scope.saveComment = function () {
                    if (scope.commentToAdd != undefined) {
                        var c = new Object();
                        c.league_id = scope.leagueid;
                        c.comment = scope.commentToAdd;
                        $http.post('/save-comment', c).success(function (response) {
                            scope.commentToAdd = null;
                            scope.comments = scope.getComments(scope.showAllComments);
                        });
                    }
                }

                scope.getComments = function (showAll) {
                    $http.get('/get-comments?league_id=' + scope.leagueid + '&show_all= ' + showAll).success(function (response) {
                        scope.comments = response;
                        scope.showAllComments = showAll;
                    });
                }

                scope.comments = scope.getComments(scope.showAllComments);

                $interval(function () { $scope.updateComments(); }, refreshrate);
            };
        })

        .directive('scoreboard', function ($http, $uibModal, $interval) {
            return {
                scope: {
                    //probably dont need to pass in username...
                    username: '@',
                    week: '@',
                    leagueid: '@',
                    refreshrate:'@',
                },
                link: link,
                template: '<div class="col-md-4">' +
                    '<table class="table table-sm table-hover">' +
                    '    <thead>' +
                    '        <tr>' +
                    '            <th>#</th>' +
                    '            <th>Player</th>' +
                    '            <th>Week Score</th>' +
                    '            <th>Season Score</th>' +
                    '        </tr>' +
                    '    </thead>' +
                    '    <tbody>' +
                    '        <tr ng-repeat="s in scores">' +
                    '            <td>{{ $index + 1 }}</td>' +
                    '            <td><div ng-click="openModal(week,s.userName,leagueid)">{{ s.userName }}</div></td>' +
                    '            <td>{{ s.weekScore }}</td>' +
                    '            <td>{{ s.seasonScore }}</td>' +
                    '        </tr>' +
                    '    </tbody>' +
                    '</table>' +
                    '</div>'
            }

            function link(scope, elem, attrs) {

                scope.getBracket = function (username, week, leagueid) {
                    $http.get('/get-week-for-player?week=' + week + '&name=' + username + '&league_id=' + leagueid).then(function (response) {
                        scope.compBracket = response.data;
                    });
                }
                scope.getLeagueMembers = function (leagueid) {
                    $http.get('/league-members-ajax?league_id=' + leagueid).success(function (response) {
                        scope.leagueMembers = response;
                    });
                }
                scope.getPlayerScores = function (leagueid, week) {
                    $http.get('/players-scores-ajax?league_id=' + leagueid + '&week=' + week).success(function (response) {
                        scope.scores = response;
                    });
                }

                $interval(function () { scope.getPlayerScores(scope.week, scope.leagueid) }, scope.refreshrate);

                scope.getBracket(scope.username, scope.week, scope.leagueid);
                scope.scores = scope.getPlayerScores(scope.leagueid, scope.week);
                scope.modalContentTemplate = '<div class="modal-top"><button class="btn btn-primary btn-right" type="button" ng-click="ok()">OK</button ><br/>' +
                    '<select name="userSelect" ng-model="compBracket.user_name" ng-options="i for i in leagueMembers" ng-change="setCompBracket(compBracket.user_name, week, leagueid)" ></select>' +
                    '</div > ' +
                    '<table class="table table-sm table-hover">' +
                    '    <thead>' +
                    '        <tr>' +
                    '            <th>Score</th>' +
                    '            <th>Visitor</th>' +
                    '            <th></th>' +
                    '            <th>Home</th>' +
                    '            <th>Score</th>' +
                    '            <th>Weight</th>' +
                    '        </tr>' +
                    '    </thead>' +
                    '    <tr ng-class="getModalRowClass(p.winner_id, bracket.picks[$index].winner_id, p.matchup.game_has_started)" ng-repeat="p in compBracket.picks">' +
                    '        <td><span ng-class="getScoreCssClass(p, false)">{{ p.matchup.visit_team_score | zeroOutScore }}</span></td>' +
                    '        <td>' +
                    '            <img src="/images/team_logos/{{p.matchup.visitTeam.short_name}}.png" alt="{{p.matchup.visitTeam.name}}" class="img-responsive" />' +
                    '               <div ng-if="p.matchup.game_has_started">' +
                    '               <input type="radio" ng-model="p.winner_id" ng-disabled="true" value="{{p.matchup.visit_team_id}}" />' +
                    '               </div>' +
                    '               <span><strong>{{ p.matchup.visitTeam.short_name }}</strong></span>' +
                    '       </td>' +
                    '            <td>vs</td>' +
                    '        <td>' +
                    '            <img src="/images/team_logos/{{p.matchup.homeTeam.short_name}}.png" alt="{{p.matchup.homeTeam.name}}" class="img-responsive" />' +
                    '            <strong>{{ p.matchup.homeTeam.short_name }}</strong>' +
                    '               <div ng-if="p.matchup.game_has_started">' +
                    '               <input type="radio" ng-model="p.winner_id" ng-disabled="true" value="{{p.matchup.home_team_id}}" /></td>' +
                    '           </div>' +
                    '        <td>' +
                    '           <div><span ng-class="getScoreCssClass(p, false)" > {{ p.matchup.home_team_score | zeroOutScore }}</span ></div>' +
                    '        </td > ' +
                    '        <td> ' +
                    '           <div ng-if="p.matchup.game_has_started" > {{p.weight}} </div>' +
                    '           <div ng-if="!p.matchup.game_has_started" > ~ </div>' +
                    '        </td > ' +
                    '    </tr>' +
                    '</table>'
                scope.openModal = function (week, username, leagueid) {
                    
                    scope.compBracket = scope.getBracket(username, week, leagueid);
                    scope.leagueMembers = scope.getLeagueMembers(leagueid);

                    scope.modalInstance = $uibModal.open({
                        scope: scope,
                        template: scope.modalContentTemplate,
                        size: 'custom',
                        controller: [
                            '$scope', '$uibModalInstance', function ($scope, $uibModalInstance) {
                                $scope.submitting = false;
                                $scope.ok = function () {
                                    $uibModalInstance.dismiss('cancel');
                                };

                                $scope.clearParameterForm = function () {
                                };

                                $scope.setCompBracket = function (username, week, leagueid) {
                                    $scope.$parent.compBracket = $scope.$parent.getBracket(username, week, leagueid);
                                    console.log()
                                }

                                $scope.getModalRowClass = function (userScore, compScore, matchStarted) {
                                    if (matchStarted && (userScore == compScore)) {
                                        return "modal-row-match";
                                    } else {
                                        return "modal-row-nonmatch";
                                    }
                                }
                            }
                        ]
                    });
                };
            };
        })


})();
