(function () {
    'use strict';

    app

        .directive('bracket', function ($http) {
            return {
                scope: {
                    weeksinseason: '@',
                    week: '@',
                    username: '@',
                    leagueid: '@',
                },
                link: link,
                template:
                '<h2>' +
                '   <i class="fa fa-arrow-circle-left" aria-hidden="true" ng-show="bracket.week > 1" ng-click="getBracket(username, bracket.week - 1, leagueid)"></i> &nbsp; Week  &nbsp; ' +
                '   <select name= "weekSelect" ng-model="bracket.week" ng-options="i for i in getArrayOfInts(1, weeksinseason)" ng-change="getBracket(username, bracket.week, leagueid)" ></select >' +
                '   <i class="fa fa-arrow-circle-right" aria-hidden="true" ng-show="bracket.week < weeksinseason" ng-click="getBracket(username, bracket.week + 1, leagueid)"></i>' +
                '</h2 >' +
                '<div class="container">' +
                '    <div class="row">' +
                '        <div class="col-md-4">' +
                '            <div ng-show="saved">Saved!</div>' +
                '            <div uib-alert ng-repeat="alert in errorsAndWarningsList" ng-class="alert- + (alert.type || warning)" close="closeAlert($index)">{{ alert.message }}</div>' +
                '            <form name="userForm" ng-submit="saveBracket()">' +
                '                <div ng-show="!bracket.allGamesStarted">' +
                '                    Weights - <span class="color-unused">Unused</span> <span class="color-used">Used</span> <span class="color-used-multi">Duplicate</span>' +
                '                    <div ng-repeat="a in availableWeights">' +
                '                        <span ng-model="a" ng-class="getWeightBoardClass(a, availableWeights.length)">{{ a.score }}</span>' +
                '                    </div>' +
                '                </div>' +
                '         <table class="table table-sm table-hover">' +
                '                    <thead>' +
                '                        <tr>' +
                '                            <th>Score</th>' +
                '                            <th>Visitor</th>' +
                '                            <th></th>' +
                '                            <th>Home</th>' +
                '                            <th>Score</th>' +
                '                            <th>Weight</th>' +
                '                        </tr>' +
                '                    </thead>' +
                '                    <tr ng-repeat="p in bracket.picks">' +
                '                        <td>' +
                '                           <span ng-class="getScoreCssClass(p, false)">{{ p.matchup.visit_team_score | zeroOutScore }}</span>' +
                '                        </td>' +
                '                        <td>' +
                '                            <img src="/images/team_logos/{{p.matchup.visitTeam.short_name}}.png" alt="{{p.matchup.visitTeam.name}}" class="img-responsive" uib-popover-html="getTeamRecordPopoverHtml({{p.matchup.visitTeam}})"  />' +
                '                            <input type="radio" ng-model="p.winner_id" ng-disabled="p.matchup.game_has_started" value="{{p.matchup.visit_team_id}}" />' +
                '                            <span><strong>{{ p.matchup.visitTeam.short_name }}</strong></span>' +
                '                        </td>' +
                '                        <td></td>' +
                '                        <td>' +
                '                            <img src="/images/team_logos/{{p.matchup.homeTeam.short_name}}.png" alt="{{p.matchup.homeTeam.name}}" class="img-responsive" uib-popover-html="getTeamRecordPopoverHtml({{p.matchup.homeTeam}})"  />' +
                '                            <strong>{{ p.matchup.homeTeam.short_name }}</strong>' +
                '                            <input type="radio" ng-model="p.winner_id" ng-disabled="p.matchup.game_has_started" value="{{p.matchup.home_team_id}}" />' +
                '                        </td>' +
                '                        <td>' +
                '                           <span ng-class="getScoreCssClass(p, true)">{{ p.matchup.home_team_score | zeroOutScore }}</span>' +
                '                        </td>' +
                '                        <td ng-class="getWeightCssClass(p)"> ' +
                '                            <span ng-if="!bracket.allGamesStarted">' +
                '                                <select name="weight_list_{{$index}}" ng-model="p.weight" ng-options="i for i in numberOfGames" ng-disabled="p.matchup.game_has_started" ng-change="getAvailableWeights()" ng-class="getWeightSelectClass(p.weight)"></select>' +
                '                            </span>' +
                '                            <span ng-if="bracket.allGamesStarted" >' +
                '                                {{ p.weight }}' +
                '                            </span>' +
                '                        </td>' +
                '                    </tr>' +
                '                </table>' +
                '                <div ng-if="!bracket.allGamesStarted">' +
                '                    <button class="btn btn-primary" type="submit">Save Picks</button>' +
                '                    <span ng-show="saved">Saved!</span>' +
                '                    <span ng-show="hasErrors">Picks not saved. Errors at top.</span>' +
                '                    <span ng-show="hasWarnings">Picks saved but with warnings. Warnings at top.</span>' +
                '                </div>' +
                '            </form>' +
                '        </div>' +
                '   </div>' +
                '</div>'
            }
            function link(scope, elem, attrs) {
                scope.getArrayOfInts = function (start, end) {
                    var result = [];
                    for (var i = start; i <= end; i++) {
                        result.push(i);
                    }
                    return result;
                };

                scope.availableWeights = [];

                scope.getAvailableWeights = function () {
                    var pickedWeights = [];
                    scope.availableWeights = [];
                    for (var i = 0; i < scope.bracket.picks.length; i++) {
                        var pick = scope.bracket.picks[i];
                        pickedWeights.push(pick.weight);
                        var aw = new Object();
                        aw.score = i + 1;
                        aw.unused = 1;
                        scope.availableWeights.push(aw);
                    }
                    for (var i = 0; i < scope.availableWeights.length; i++) {
                        for (var j = 0; j < pickedWeights.length; j++) {
                            if (scope.availableWeights[i].score == pickedWeights[j]) {
                                if (scope.availableWeights[i].unused == 1) {
                                    scope.availableWeights[i].unused = 0;
                                } else {
                                    scope.availableWeights[i].unused = -1
                                }
                            }
                        }
                    }
                    return scope.availableWeights;
                };
                
                scope.getBracket = function (username, week, leagueid) {
                    $http.get('/get-week-for-player?week=' + week + '&name=' + username + '&league_id=' + leagueid).success(function (response) {
                        scope.bracket = response;
                        scope.hasErrors = false;
                        scope.hasWarnings = false;
                        scope.saved = null;
                        scope.errorsAndWarningsList = null;
                        scope.availableWeights = scope.getAvailableWeights();
                        scope.numberOfGames = scope.getArrayOfInts(1, scope.bracket.picks.length);
                    });
                }

                scope.saveBracket = function () {

                    $http.post('/save-bracket', scope.bracket).success(function (response) {
                        scope.viewmodel = response;
                        scope.bracket = scope.viewmodel.bracket;
                        var l = scope.viewmodel.errors;
                        if (l.length == 0) {
                            scope.errorsAndWarningsList = scope.viewmodel.errors;
                            scope.hasErrors = false;
                            scope.hasWarnings = false;
                        } else {
                            scope.errorsAndWarningsList = scope.viewmodel.errors;
                            for (var i = 0; i < scope.errorsAndWarningsList.length; i++) {
                                var e = scope.errorsAndWarningsList[i];
                                if ("danger" == e.type) {
                                    scope.hasErrors = true;
                                } else if ("warning" == e.type) {
                                    scope.hasWarnings = true;
                                }
                            }
                        }
                        if (scope.hasErrors) {
                            scope.saved = false;
                        } else {
                            scope.saved = true;
                        }
                    });
                };

                scope.closeAlert = function (index) {
                    scope.errorsAndWarningsList.splice(index, 1);
                };

                scope.getWeightBoardClass = function (a, l) {
                    var r = a.unused == 1 ? 'weightboard-unused color-unused' : a.unused == 0 ? 'weightboard-used color-used' : 'weightboard-used-multi color-used-multi';
                    var w = l == 16 ? 'weightboard-width-16' : l == 15 ? 'weightboard-width-15' : l == 14 ? 'weightboard-width-14' : 'weightboard-width-16';
                    return r + ' ' + w;
                };

                scope.getScoreCssClass = function (p, isHome) {
                    if (p.matchup.status == 'F' || p.matchup.status == 'FO') {
                        if (isHome) {
                            if (p.matchup.home_team_id == p.matchup.win_team_id) {
                                return 'score-final-winner';
                            } else {
                                return 'score-final-loser';
                            }
                        } else {
                            if (p.matchup.visit_team_id == p.matchup.win_team_id) {
                                return 'score-final-winner';
                            } else {
                                return 'score-final-loser';
                            }
                        }
                    } else {
                        if (!p.matchup.game_has_started) {
                            return 'score-pending';
                        } else {
                            return 'score-active';
                        }
                    }
                };

                scope.getTeamRecordPopoverHtml = function (team) {
                    return '<table class=\'table table-sm\'><tr><th>W</th><th>L</th></tr><tr><td><b>' + team.wins + '</b></td><td><b>' + team.losses + '</b></td></tr></table>';
                }

                scope.getGameDatePopoverHtml = function (date) {
                    return date;
                }

                scope.getWeightSelectClass = function (w) {
                    var count = 0;
                    for (var i = 0; i < scope.bracket.picks.length; i++) {
                        if (scope.bracket.picks[i].weight == w) {
                            count++;
                        }
                    }
                    if (count > 1) {
                        return "select-multi";
                    } else {
                        return "selectpicker";
                    }
                }

                scope.resetWeightClasses = function () {
                    scope.getAvailableWeights();
                    var allSelects = $("select");
                    angular.forEach(allSelects, function (newBracketPicks, index) {
                        var s = allSelects[index];
                        if (s.name.indexOf('weight_list_') > -1) {
                            angular.forEach(s.options, function (option, index) {
                                var optionValue = parseInt(option.label, 10);
                                if (option.label != '' && optionValue != NaN) {
                                    var newClassList = [];
                                    if (scope.availableWeights[optionValue - 1].unused == 0) {
                                        newClassList.push("color-used");
                                    } else if (scope.availableWeights[optionValue - 1].unused == 1) {
                                        newClassList.push("color-unused");
                                    }
                                    else {
                                        newClassList.push("color-used-multi");
                                    }
                                    option.classList = newClassList;
                                }
                            });
                        }
                    });
                };

                //initialize bracket
                scope.bracket = { picks: {} };
                scope.getBracket(scope.username, scope.week, scope.leagueid);

                scope.$watch(function (scope) {
                    return scope.bracket.picks;
                }, function (newValue, oldValue, scope) {
                    scope.resetWeightClasses();
                }, true);

                angular.element(document).ready(function () {
                    //runs when document is ready to set the css class on the weight select options
                    scope.resetWeightClasses();
                });

            }


        })

        .directive('commentSection', function ($http, $interval) {
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

                $interval(function () { scope.updateComments(); }, scope.refreshrate);
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

                $interval(function () { scope.getPlayerScores(scope.leagueid, scope.week) }, scope.refreshrate);

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
                                }

                                $scope.getModalRowClass = function (userScore, compScore, matchStarted) {
                                    if (matchStarted && (userScore == compScore)) {
                                        return "modal-row-match";
                                    } else {
                                        return "modal-row-nonmatch";
                                    }
                                }
                                //this is duplicated
                                $scope.getScoreCssClass = function (p, isHome) {
                                    if (p.matchup.status == 'F' || p.matchup.status == 'FO') {
                                        if (isHome) {
                                            if (p.matchup.home_team_id == p.matchup.win_team_id) {
                                                return 'score-final-winner';
                                            } else {
                                                return 'score-final-loser';
                                            }
                                        } else {
                                            if (p.matchup.visit_team_id == p.matchup.win_team_id) {
                                                return 'score-final-winner';
                                            } else {
                                                return 'score-final-loser';
                                            }
                                        }
                                    } else {
                                        if (!p.matchup.game_has_started) {
                                            return 'score-pending';
                                        } else {
                                            return 'score-active';
                                        }
                                    }
                                }
                            }
                        ]
                    });
                };
            };
        })

    .filter('zeroOutScore', function () {
        return function (input) {
            if (input < 0) {
                return 0;
            } else {
                return input;
            }
        }
    });
})();
