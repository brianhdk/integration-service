integrationsApp.controller('latestTasksController', function ($scope, $http) {

    $http.get("/latestTasks").success(function (xhr) {
        $scope.tasks = xhr;
    });
});
