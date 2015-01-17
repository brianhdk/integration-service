integrationsApp.controller('taskExecutionDetailsController', function ($scope, $http, $routeParams, $filter, ngTableParams) {

    $http.get("/taskExecutionDetails?id=" + $routeParams.taskId).success(function (xhr) {
        var data = xhr;

        $scope.tableParams = new ngTableParams({
            page: 1,
            count: 10,
            sorting: {
                TimeStamp: 'asc'
            },
            filter: {
                name: ''       // initial filter
            }
        }, {
            getData: function ($defer, params) {
                var filteredData = params.filter() ?
                                   $filter('filter')(data, params.filter()) :
                                   data;

                var orderedData = params.sorting() ?
                                    $filter('orderBy')(filteredData, params.orderBy()) :
                                    filteredData;

                params.total(orderedData.length);

                $defer.resolve(orderedData.slice((params.page() - 1) * params.count(), params.page() * params.count()));
            }
        });

    });
});
