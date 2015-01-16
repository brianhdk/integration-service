var integrationsApp = angular.module('integrationsApp', ['ngRoute']);
// configure our routes
integrationsApp.config(function ($routeProvider) {
	$routeProvider

		// route for the home page
		.when('/', {
			templateUrl: 'pages/profile.html',
			controller: 'mainController'
		})

		 // route for the about page
        .when('/about', {
            templateUrl: 'pages/about.html',
            controller: 'aboutController'
        })
	;
});
integrationsApp.controller('mainController', function ($scope) {
	$scope.message = 'Look! I am a home page.';
});
integrationsApp.controller('aboutController', function ($scope) {
	$scope.message = 'Look! I am an about page.';
});
