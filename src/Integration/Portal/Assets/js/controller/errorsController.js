integrationsApp.controller('errorsController', function ($scope, $http) {

    $http.get("/errors").success(function (xhr) {
        $scope.errors = xhr;
    });
});
