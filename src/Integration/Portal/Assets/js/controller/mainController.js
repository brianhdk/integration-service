integrationsApp.controller('mainController', function ($scope, $location) {
    $scope.isActive = function (route) {
        return route === $location.path();
    }
    $scope.message = 'main controller.';
});
