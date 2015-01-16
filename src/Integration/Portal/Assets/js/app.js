var integrationsApp = angular.module('integrationsApp', ['ngRoute']);

// configure our routes
integrationsApp.config(function ($routeProvider) {
    $routeProvider

		// route for the home page
		.when('/', {
		    templateUrl: '/assets/pages/main.html',
		    controller: 'mainController'
		})

        .when('/errors', {
            templateUrl: '/assets/pages/errors.html',
            controller: 'errorsController'
        })

        .when('/errors/:errorId', {
            templateUrl: '/assets/pages/errors-detail.html',
            controller: 'errorsController'
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