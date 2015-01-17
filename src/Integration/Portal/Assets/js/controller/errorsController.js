integrationsApp.controller('errorsController', function ($scope, $http, $filter, ngTableParams) {

    //this.tableParams = new ngTableParams({
    //    page: 1,
    //    count: 10
    //}, {
    //    getData: function ($defer, params) {
    //        $http.get('/errors', {
    //            params: {
    //                pageNumber: params.page() - 1,
    //                rangeStart: rangeStart,
    //                rangeStop: rangeStop
    //            }
    //        })
    //            .success(function (data) {

    //                params.total(data.TotalItems);

    //                $defer.resolve(data.Items);
    //            });
    //    }
    //});

    $http.get("/errors").success(function (xhr) {
        var data = xhr;

        $scope.tableParams = new ngTableParams({
            page: 1,
            count: 10,
            sorting: {
            	TimeStamp: 'desc'     // initial sorting
            },
            filter: {
                name: ''       // initial filter
            }
        }, {
            getData: function ($defer, params) {
                //data = params.filter() ? $filter('filter')(data, params.filter()) : data;
                //data = params.sorting() ? $filter('orderBy')(data, params.orderBy()) : data;

                params.total(data.TotalItems);

                $defer.resolve(data.Items.slice((params.page() - 1) * params.count(), params.page() * params.count()));
            }
        });

    });
});
