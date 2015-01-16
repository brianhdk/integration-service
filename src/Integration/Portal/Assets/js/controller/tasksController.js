integrationsApp.controller('tasksController', function ($scope, $http) {

    $http.get("/runningtasks").success(function (xhr) {
        $scope.tasks = xhr;
    });

    $http.get("/runningtasks?count=100").success(function (xhr) {
        $scope.tasks = xhr;
    });
});
