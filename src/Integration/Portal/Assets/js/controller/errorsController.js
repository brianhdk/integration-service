integrationsApp.controller('errorsController', function ($scope, $http, $routeParams) {
    console.log($routeParams.errorId)

    $http.get("/errors").success(function (xhr) {
        $scope.errors = xhr;
    });
});
