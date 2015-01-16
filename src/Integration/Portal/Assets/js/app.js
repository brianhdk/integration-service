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
            controller: 'errorDetailController'
        })

        .when('/running-tasks', {
            templateUrl: '/assets/pages/runningTasks.html',
            controller: 'tasksController'
        })

        .when('/latest-tasks', {
            templateUrl: '/assets/pages/latestTasks.html',
            controller: 'tasksController'
        })
    ;
});