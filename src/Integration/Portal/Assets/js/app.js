var integrationsApp = angular.module('integrationsApp', ['ngRoute', 'ngTable']);

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

        .when('/taskdetails', {
            templateUrl: '/assets/pages/tasks.html',
            controller: 'taskDetailsController'
        })

        .when('/errors/:errorId', {
            templateUrl: '/assets/pages/errors-detail.html',
            controller: 'errorDetailController'
        })

        .when('/running-tasks', {
            templateUrl: '/assets/pages/runningTasks.html',
            controller: 'runningTasksController'
        })

        .when('/latest-tasks', {
            templateUrl: '/assets/pages/latestTasks.html',
            controller: 'latestTasksController'
        })
    ;
});