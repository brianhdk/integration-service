integrationsApp.controller('errorsController', function ($scope, $http, $filter,  $resource, ngTableParams) {

    var Api = $resource('/errors');

    $scope.tableParams = new ngTableParams({
        page: 1,
        count: 10
    }, {
        total: 0,
        getData: function ($defer, params) {

            Api.get(params.url(), function (data) {

                params.total(data.TotalItems);

                $defer.resolve(data.Items);
            });
        }
    });
});
