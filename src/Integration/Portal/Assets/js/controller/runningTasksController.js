integrationsApp.controller('runningTasksController', function ($scope, $http) {

    $http.get("/runningtasks").success(function (xhr) {
        $scope.tasks = xhr;
    });
});
