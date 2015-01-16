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
        .when('/about', {
        	templateUrl: '/assets/pages/about.html',
            controller: 'aboutController'
        })
	;
});
integrationsApp.controller('mainController', function ($scope) {
	$scope.message = 'main controller.';
});
integrationsApp.controller('homeController', function ($scope) {
	$scope.message = 'Look! I am a home page.';
});
integrationsApp.controller('aboutController', function ($scope) {
	$scope.message = 'Look! I am an about page.';
});
