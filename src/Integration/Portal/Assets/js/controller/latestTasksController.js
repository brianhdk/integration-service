integrationsApp.controller('latestTasksController', function ($scope, $http) {

	$http.get("/latestTasks?count=10").success(function (xhr) {
        $scope.tasks = xhr;
	});

	$scope.getLatestTasks = function (count) {
		$http.get("/latestTasks?count=" + count).success(function (xhr) {
			$scope.tasks = xhr;
		});
	}

	$scope.getTaskNames = function () {
		$http.get("/latestTasks").success(function (xhr) {
			$scope.tasksNames = xhr;
		});
	}
});
