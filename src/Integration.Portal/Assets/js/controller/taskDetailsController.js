integrationsApp.controller('taskDetailsController', function ($scope, $http, $routeParams, $filter, ngTableParams) {

	$scope.init = function() {
		$http.get("/taskDetails").success(function(xhr) {
			var data = xhr;

			$scope.tableParams = new ngTableParams({
				page: 1,
				count: 100,
				sorting: {
					DisplayName: 'asc' // initial sorting
				},
				filter: {}
			}, {
				getData: function($defer, params) {
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
	}
	$scope.getTaskDetail = function () {
		$http.get("/taskDetails?displayname=" + $routeParams.displayname).success(function (xhr) {
			$scope.taskDetail = xhr;
		});
		$http.get("/taskDetails?displayname=" + $routeParams.displayname + "&count=10").success(function (xhr) {
			$scope.lastRun = xhr;
		});

	}
});
