var integrationsApp = angular.module('integrationsApp', ['ngRoute']);
// configure our routes
integrationsApp.config(function ($routeProvider) {
	$routeProvider

		// route for the home page
		.when('/', {
			templateUrl: 'pages/home.html',
			controller: 'homeController'
		})

		 // route for the about page
        .when('/about', {
            templateUrl: 'pages/about.html',
            controller: 'aboutController'
        })
	;
});
integrationsApp.controller('mainController', function ($scope) {
	$scope.message = 'main controller.';
	$scope.active = false;
});
integrationsApp.controller('homeController', function ($scope) {
	$scope.message = 'Look! I am a home page.';
	$scope.active = true;
});
integrationsApp.controller('aboutController', function ($scope) {
	$scope.message = 'Look! I am an about page.';
	$scope.active = true;
});
