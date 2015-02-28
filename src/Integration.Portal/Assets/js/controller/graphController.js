integrationsApp.controller('graphController', function ($scope, $http, $filter, $resource, $location) {

	$scope.initGraph = function () {
		$http.get("/graph?id=1").success(function (xhr) {
			$scope.data = xhr;

			var array = [];
			angular.forEach($scope.data, function (value, key) {
				this.push([value.Date, value.Errors]);
			}, array);
			array.push(['Date', 'Errors']);

			// Some raw data (not necessarily accurate)
			var data = google.visualization.arrayToDataTable(array.reverse());

			var options = {
				title: 'Errors last 5 days',
				vAxis: { title: "Errors" },
				hAxis: { title: "Date" },
				seriesType: "bars",
				series: { 5: { type: "line" } }
			};

			var chart = new google.visualization.ComboChart(document.getElementById('chart_div'));
			chart.draw(data, options);
		});
	}
	
	$scope.isActive = function (route) {
		return route === $location.path();
	}


});
