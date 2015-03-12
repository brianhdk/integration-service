integrationsApp.controller('configurationDetailController', function ($scope, $http, $routeParams) {

	$http.get('/configuration?clrType=' + $routeParams.clrType).success(function (xhr) {
        $scope.clr = xhr;
	});

	$scope.saveConfiguration = function () {
	    $http.put('/configuration', $scope.clr).success(function (xhr) {
	        $scope.clr = xhr;
	    });
    }

});
