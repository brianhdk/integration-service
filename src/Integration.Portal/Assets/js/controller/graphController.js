integrationsApp.controller('graphController', function ($scope, $http) {

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
	
	$scope.initPie = function() {
			// Some raw data (not necessarily accurate)
			var data = google.visualization.arrayToDataTable([
				['Month', 'Bolivia', 'Ecuador', 'Madagascar', 'Papua New Guinea', 'Rwanda', 'Average'],
				['2004/05', 165, 938, 522, 998, 450, 614.6],
				['2005/06', 135, 1120, 599, 1268, 288, 682],
				['2006/07', 157, 1167, 587, 807, 397, 623],
				['2007/08', 139, 1110, 615, 968, 215, 609.4],
				['2008/09', 136, 691, 629, 1026, 366, 569.6]
			]);

			var options = {
				title: 'Monthly Coffee Production by Country',
				vAxis: { title: "Cups" },
				hAxis: { title: "Month" },
				seriesType: "bars",
				series: { 5: { type: "line" } }
			};

			var chart = new google.visualization.ComboChart(document.getElementById('chart_div'));
			chart.draw(data, options);
	}	
});
