integrationsApp.controller("mainController", function ($scope, $location, $http) {

    $scope.isActive = function (route) {
        return route === $location.path();
    }

    $http.get("/uptime").success(function(xhr) {
        $scope.uptime = xhr;
    });
});
