var integrationsApp = angular.module('integrationsApp', ['ngRoute', 'ngTable', 'ngResource', 'ui.ace']);

// configure our routes
integrationsApp.config(function ($routeProvider, $locationProvider) {

    $locationProvider.html5Mode(false);

    $routeProvider

        .when('/tasks', {
            templateUrl: '/assets/pages/tasks.html',
            controller: 'taskDetailsController'
        })

        .when('/errors', {
            templateUrl: '/assets/pages/errors.html',
            controller: 'errorsController',
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

        .when('/archives', {
            templateUrl: '/assets/pages/archives.html',
            controller: 'archivesController'
        })

        .when('/configuration', {
        	templateUrl: '/assets/pages/configuration.html',
        	controller: 'configurationController'
        })

        .when('/configuration/:clrType', {
        	templateUrl: '/assets/pages/configurationDetail.html',
        	controller: 'configurationDetailController'
        })

        .when('/graph', {
            templateUrl: '/assets/pages/graph.html',
            controller: 'graphController'
        })

        .otherwise('/tasks')
    ;
});