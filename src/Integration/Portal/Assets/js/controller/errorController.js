var integrationsApp = angular.module('integrationsApp', ['ngRoute']);
// configure our routes
integrationsApp.config(function ($routeProvider) {
	$routeProvider

		// route for the home page
		.when('/', {
			templateUrl: '/assets/pages/home.html',
			controller: 'homeController'
		})

		 // route for the about page
        .when('/running-tasks', {
        	templateUrl: '/assets/pages/runningTasks.html',
        	controller: 'runningTasksController'
        })

		 // route for the about page
        .when('/latest-tasks', {
        	templateUrl: '/assets/pages/latestTasks.html',
        	controller: 'latestTasksController'
        })
	;
});
integrationsApp.controller('mainController', function($scope, $location) {
	$scope.isActive = function (route) {
		return route === $location.path();
	}
	$scope.message = 'main controller.';
});
integrationsApp.controller('homeController', function ($scope, $http) {
    $http.get("/errors").success(function (xhr) {
        $scope.errors = xhr;
    });

});
integrationsApp.controller('runningTasksController', function ($scope, $http) {

	$http.get("/runningtasks").success(function (xhr) {
		$scope.tasks = xhr;
	});
});
integrationsApp.controller('latestTasksController', function ($scope, $http) {

	$http.get("/runningtasks?count=100").success(function (xhr) {
		$scope.tasks = xhr;
	});
});
