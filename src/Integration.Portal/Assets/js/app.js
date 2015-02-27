var integrationsApp = angular.module('integrationsApp', ['ngRoute', 'ngTable', 'ngResource']);

// configure our routes
integrationsApp.config(function ($routeProvider) {
    $routeProvider

		.when('/', {
		    templateUrl: '/assets/pages/errors.html',
		    controller: 'errorsController'
		})

        .when('/errors', {
            templateUrl: '/assets/pages/errors.html',
            controller: 'errorsController',
        })

        .when('/tasks', {
            templateUrl: '/assets/pages/tasks.html',
            controller: 'taskDetailsController'
        })

        .when('/taskdetail', {
            templateUrl: '/assets/pages/taskDetail.html',
            controller: 'taskDetailsController'
        })

        .when('/taskexecutiondetail/:taskId', {
            templateUrl: '/assets/pages/taskExecutionDetail.html',
            controller: 'taskExecutionDetailsController'
        })

        .when('/errors/:errorId', {
            templateUrl: '/assets/pages/errors-detail.html',
            controller: 'errorDetailController'
        })

        .when('/running-tasks', {
            templateUrl: '/assets/pages/taskList.html',
            controller: 'runningTasksController'
        })

        .when('/latest-tasks', {
            templateUrl: '/assets/pages/taskList.html',
            controller: 'latestTasksController'
        })

        .when('/graph', {
            templateUrl: '/assets/pages/graph.html',
            controller: 'graphController'
        })
    ;
});