integrationsApp.controller('configurationDetailController', function ($scope, $http, $routeParams) {

	$http.get('/configuration?id=' + $routeParams.id).success(function (xhr) {
        $scope.model = xhr;
	});

	$scope.saveConfiguration = function () {
	    $http.put('/configuration', $scope.model).success(function (xhr) {
	        $scope.model = xhr;
	    });
    }

});
