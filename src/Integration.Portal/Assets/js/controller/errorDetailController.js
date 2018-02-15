integrationsApp.controller("errorDetailController", function ($scope, $http, $routeParams) {

    $http.get("/errors?id=" + $routeParams.errorId).success(function (xhr) {
        $scope.error = xhr;
    });
});
