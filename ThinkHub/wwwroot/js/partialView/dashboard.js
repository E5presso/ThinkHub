var REFRESH = true; // 모니터 갱신
var SCROLL_RATIO = 80; // 스크롤 배수
var MAX_GRAPH = 60; // 최대 표시 가능한 그래프 길이
var DELAY = 100; // 동작 지연시간
var INTERVAL = 1000; // 반복 수행 간격

var cpu;
var mem;
var disk;
var net;
var capacity;
var user;

var cpu_chart;
var mem_chart;
var disk_chart;
var net_chart;
var capacity_chart;
var user_chart;

function Ajax(action, data, success)
{
	$.ajax({
		url: `/Admin/${action}`,
		type: "POST",
		data: data,
		success: function (result)
		{
			switch (result.success)
			{
				case "NOT_LOGGEDIN": {
					setTimeout(function ()
					{
						alert("세션이 만료되었습니다.");
						window.location.href = "https://www.thinkhub.ml/User/Login";
					}, DELAY);
					break;
				}
				default: {
					success(result);
					break;
				}
			}
		}
	});
}

function InitDashBoard()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var padding = parseInt($("#DashBoard").css("paddingTop"));

	$("#DashBoard").css("top", gnb);
	$("#DashBoard").height($(window).height() - gnb - (padding * 2));
	$(window).resize(function ()
	{
		var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
		var padding = parseInt($("#DashBoard").css("paddingTop"));

		$("#DashBoard").height($(window).height() - gnb - (padding * 2));
	});
}
function InitCharts()
{
	cpu_chart = {
		type: "line",
		data: {
			datasets: [{
				label: "CPU 사용량",
				pointRadius: 0,
				borderDash: [0, 7],
				backgroundColor: ["rgba(39, 149, 245, 0.8)"]
			}],
		},
		options: {
			tooltips: {
				enabled: false
			},
			scales: {
				xAxes: [{
					display: false
				}],
				yAxes: [{
					ticks: {
						min: 0,
						max: 100,
						beginAtZero: true
					}
				}]
			}
		}
	};
	mem_chart = {
		type: "line",
		data: {
			datasets: [{
				label: "메모리 사용량",
				pointRadius: 0,
				borderDash: [0, 7],
				backgroundColor: ["rgba(247, 72, 64, 0.8)"]
			}],
		},
		options: {
			tooltips: {
				enabled: false
			},
			scales: {
				xAxes: [{
					display: false
				}],
				yAxes: [{
					ticks: {
						min: 0,
						max: 100,
						beginAtZero: true
					}
				}]
			}
		}
	};
	disk_chart = {
		type: "line",
		data: {
			datasets:
			[
				{
					label: "디스크 읽기",
					pointRadius: 0,
					borderDash: [0, 7],
					backgroundColor: ["rgba(115, 194, 12, 0.5)"]
				},
				{
					label: "디스크 쓰기",
					pointRadius: 0,
					borderDash: [0, 6],
					borderCapStyle: 'round',
					backgroundColor: ["rgba(202, 224, 70, 0.5)"]
				}
			],
		},
		options: {
			tooltips: {
				enabled: false
			},
			scales: {
				xAxes: [{
					display: false
				}],
				yAxes: [{
					ticks: {
						beginAtZero: true
					}
				}]
			}
		}
	};
	net_chart = {
		type: "line",
		data: {
			datasets:
			[
				{
					label: "수신된 데이터",
					pointRadius: 0,
					borderDash: [0, 7],
					backgroundColor: ["rgba(255, 119, 37, 0.5)"]
				},
				{
					label: "전송된 데이터",
					pointRadius: 0,
					borderDash: [0, 6],
					borderCapStyle: 'round',
					backgroundColor: ["rgba(246, 121, 159, 0.5)"]
				}
			],
		},
		options: {
			tooltips: {
				enabled: false
			},
			scales: {
				xAxes: [{
					display: false
				}],
				yAxes: [{
					ticks: {
						beginAtZero: true
					}
				}]
			}
		}
	};
	capacity_chart = {
		type: "doughnut",
		data: {
			labels: ["사용 가능한 공간", "사용 중인 공간"],
			datasets:
			[
				{
					label: "디스크 사용량",
					pointRadius: 0,
					backgroundColor: [
						"rgba(24, 182, 222, 0.8)",
						"rgba(119, 119, 119, 0.8)"
					]
				}
			],
		},
		options: {
			tooltips: {
				enabled: false
			},
			scales: {
				xAxes: [{
					display: false
				}],
				yAxes: [{
					display: false,
					ticks: {
						beginAtZero: true
					}
				}]
			}
		}
	};
	user_chart = {
		type: "line",
		data: {
			datasets: [{
				label: "사용자 수",
				pointRadius: 0,
				borderColor: ["rgba(39, 149, 245, 0.8)"],
				fill: false
			}],
		},
		options: {
			tooltips: {
				enabled: false
			},
			scales: {
				xAxes: [{
					display: false
				}],
				yAxes: [{
					ticks: {
						beginAtZero: true
					}
				}]
			}
		}
	};

	cpu = new Chart($("#Cpu"), cpu_chart);
	mem = new Chart($("#Memory"), mem_chart);
	disk = new Chart($("#Disk"), disk_chart);
	net = new Chart($("#Network"), net_chart);
	capacity = new Chart($("#Capacity"), capacity_chart);
	user = new Chart($("#User"), user_chart);
}

function usage_result(result)
{
	REFRESH = true;
	if (result.result == "SUCCESS")
	{
		cpu_chart.data.labels.push("");
		if (cpu_chart.data.labels.length > MAX_GRAPH) cpu_chart.data.labels.shift();
		cpu_chart.data.datasets.forEach(function (dataset)
		{
			dataset.data.push(result.cpu);
			if (dataset.data.length > MAX_GRAPH) dataset.data.shift();
		});
		cpu.update();

		mem_chart.data.labels.push("");
		if (mem_chart.data.labels.length > MAX_GRAPH) mem_chart.data.labels.shift();
		mem_chart.data.datasets.forEach(function (dataset)
		{
			dataset.data.push(result.mem);
			if (dataset.data.length > MAX_GRAPH) dataset.data.shift();
		});
		mem.update();

		disk_chart.data.labels.push("");
		if (disk_chart.data.labels.length > MAX_GRAPH) disk_chart.data.labels.shift();
		disk_chart.data.datasets.forEach(function (dataset, index)
		{
			if (index == 0) dataset.data.push(result.diskRead);
			else dataset.data.push(result.diskWrite);
			if (dataset.data.length > MAX_GRAPH) dataset.data.shift();
		});
		disk.update();

		net_chart.data.labels.push("");
		if (net_chart.data.labels.length > MAX_GRAPH) net_chart.data.labels.shift();
		net_chart.data.datasets.forEach(function (dataset, index)
		{
			if (index == 0) dataset.data.push(result.netReceived);
			else dataset.data.push(result.netSent);
			if (dataset.data.length > MAX_GRAPH) dataset.data.shift();
		});
		net.update();

		capacity_chart.data.datasets.forEach(function (dataset)
		{
			dataset.data[0] = result.diskFree;
			dataset.data[1] = result.diskUsed;
		});
		capacity.update();

		user_chart.data.labels.push("");
		if (user_chart.data.labels.length > MAX_GRAPH) user_chart.data.labels.shift();
		user_chart.data.datasets.forEach(function (dataset)
		{
			dataset.data.push(result.connectedUser);
			if (dataset.data.length > MAX_GRAPH) dataset.data.shift();
		});
		user.update();
	}
}
function userlist_result(result)
{
	if (result.result == "SUCCESS")
	{
		var users = result.users;
		for (var i = 0; i < users.length; i++)
		{
			$("#User .table tbody").append(`<tr id="user-${users[i].id}"></tr>`);
			$(`#user-${users[i].id}`).append(`<td>${users[i].id}</td>`);
			$(`#user-${users[i].id}`).append(`<td>${users[i].userName}</td>`);
			$(`#user-${users[i].id}`).append(`<td class="d-none d-md-table-cell">${users[i].email}</td>`);
			$(`#user-${users[i].id}`).append(`<td>${users[i].registrationDate}</td>`);
		}
	}
}

$(document).ready(function ()
{
	InitDashBoard();
	InitCharts();

	setInterval(function ()
	{
		if (REFRESH) Ajax("Monitor", {}, usage_result);
		REFRESH = false;
	}, INTERVAL);
	$(".conf-btn").on("click", function ()
	{
		window.location.href = "/Admin/UserManager";
	});
});