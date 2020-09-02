var IS_MOBILE = navigator.maxTouchPoints > 1;
var SCROLL_RATIO = 80; // 스크롤 배수
var DURATION = 300; // 애니메이션 동작길이
var DELAY = 100; // 동작 지연시간

var MOUSEDOWN = false // 마우스 클릭상태

var START_MOUSE_X = 0; // 마우스 시작좌표 X
var START_MOUSE_Y = 0; // 마우스 시작좌표 Y

var END_MOUSE_X = 0; // 마우스 종점좌표 X
var END_MOUSE_Y = 0; // 마우스 종점좌표 Y

var CURRENT_PAGE = 1; // 현재 페이지
var PAGE_PER_SCROLL = 1; // 페이지 당 로딩할 파일 갯수
var EXPLORER_LIST = []; // 현재 탐색기에 표시될 목록
var EXPLORER_COUNT = 0; // 현재 탐색기에 표시 중인 파일의 갯수
var SCROLL_UPDATE = true; // 스크롤 시 다음 데이터를 받아올 지 결정

var IS_DRAGGING = false;

var CURRENT_PATH; // 현재 보여지고 있는 경로

var PREV_STACK = []; // 이전 기록 스택
var NEXT_STACK = []; // 다음 기록 스택

var UPLOAD_HANDLES = []; // 인터벌 함수의 핸들 목록
var UPLOAD_LIST = []; // 업로드 중인 파일 목록
var UPLOAD_DICT = {}; // 파일 목록에 대한 해쉬테이블
var UPLOAD_QUEUE = 0; // 현재 전송 중인 파일의 갯수
var MAX_QUEUE = 6; // 최대 동시에 전송 가능한 파일의 갯수

var BANDWIDTH = 0; // 업로드 대역폭을 측정
var TOTAL_SIZE = 0; // 업로드할 파일의 전체 용량

var CHUNK_SIZE = 500 * 1024; // 분할 전송할 조각의 크기 지정 = 500KB

function Ajax(action, data, success)
{
	ShowCurtain();
	ShowLoading();
	setTimeout(function ()
	{
		$.ajax({
			url: `/File/${action}`,
			type: "POST",
			data: data,
			success: function (result)
			{
				HideLoading();
				HideCurtain();
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
					case "ACCESS_DENIED": {
						setTimeout(function ()
						{
							alert("접근이 거부되었습니다.");
							GoToHome();
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
	}, DELAY * 3);
}
function Jax(action, data, success)
{
	ShowCurtain();
	ShowLoading();
	$.ajax({
		url: `/File/${action}`,
		async: false,
		type: "POST",
		data: data,
		success: function (result)
		{
			HideLoading();
			HideCurtain();
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
				case "ACCESS_DENIED": {
					setTimeout(function ()
					{
						alert("접근이 거부되었습니다.");
						GoToHome();
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

function AjaxSilent(action, data, success)
{
	setTimeout(function ()
	{
		$.ajax({
			url: `/File/${action}`,
			type: "POST",
			data: data,
			success: function (result)
			{
				switch (result.success)
				{
					case "NOT_LOGGEDIN": {
						alert("세션이 만료되었습니다.");
						window.location.href = "https://www.thinkhub.ml/User/Login";
						break;
					}
					case "ACCESS_DENIED": {
						alert("접근이 거부되었습니다.");
						break;
					}
					default: {
						success(result);
						break;
					}
				}
			}
		});
	}, DELAY * 3);
}
function JaxSilent(action, data, success)
{
	$.ajax({
		url: `/File/${action}`,
		async: false,
		type: "POST",
		data: data,
		success: function (result)
		{
			switch (result.success)
			{
				case "NOT_LOGGEDIN": {
					alert("세션이 만료되었습니다.");
					window.location.href = "https://www.thinkhub.ml/User/Login";
					break;
				}
				case "ACCESS_DENIED": {
					alert("접근이 거부되었습니다.");
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

function ValidateDirectoryName(dir)
{
	var regex = /[\\\/:*?\"<>|#&+]/gi;
	return !regex.test(dir);
}

function AddPermissionList(username, create, read, write, remove)
{
	if ($(`#Permission-${username}`).length)
		alert("이미 추가된 사용자입니다.");
	else
	{
		$(`#ShareManager .list-group`).append(`<li id="Permission-${username}" class="list-group-item"></li>`);
		$(`#ShareManager #Permission-${username}`).append(`<span class="user-name">${username}</span>`);
		$(`#ShareManager #Permission-${username}`).append(`<div class="form-group"></div>`);
		$(`#ShareManager #Permission-${username} .form-group`).append(`<div id="CreateCheckBox" class="custom-control custom-checkbox"></div>`);
		$(`#ShareManager #Permission-${username} #CreateCheckBox`).append(`<input type="checkbox" class="custom-control-input" id="CreatePermission-${username}">`);
		$(`#ShareManager #Permission-${username} #CreateCheckBox`).append(`<label class="custom-control-label" for="CreatePermission-${username}">생성</label>`);
		$(`#ShareManager #Permission-${username} .form-group`).append(`<div id="ReadCheckBox" class="custom-control custom-checkbox"></div>`);
		$(`#ShareManager #Permission-${username} #ReadCheckBox`).append(`<input type="checkbox" class="custom-control-input" id="ReadPermission-${username}">`);
		$(`#ShareManager #Permission-${username} #ReadCheckBox`).append(`<label class="custom-control-label" for="ReadPermission-${username}">읽기</label>`);
		$(`#ShareManager #Permission-${username} .form-group`).append(`<div id="WriteCheckBox" class="custom-control custom-checkbox"></div>`);
		$(`#ShareManager #Permission-${username} #WriteCheckBox`).append(`<input type="checkbox" class="custom-control-input" id="WritePermission-${username}">`);
		$(`#ShareManager #Permission-${username} #WriteCheckBox`).append(`<label class="custom-control-label" for="WritePermission-${username}">쓰기</label>`);
		$(`#ShareManager #Permission-${username} .form-group`).append(`<div id="RemoveCheckBox" class="custom-control custom-checkbox"></div>`);
		$(`#ShareManager #Permission-${username} #RemoveCheckBox`).append(`<input type="checkbox" class="custom-control-input" id="RemovePermission-${username}">`);
		$(`#ShareManager #Permission-${username} #RemoveCheckBox`).append(`<label class="custom-control-label" for="RemovePermission-${username}">삭제</label>`);
		if (create) $(`#Permission-${username} #CreatePermission-${username}`).prop("checked", true);
		if (read) $(`#Permission-${username} #ReadPermission-${username}`).prop("checked", true);
		if (write) $(`#Permission-${username} #WritePermission-${username}`).prop("checked", true);
		if (remove) $(`#Permission-${username} #RemovePermission-${username}`).prop("checked", true);
		$(`#ShareManager #Permission-${username} .form-group`).append(`<button class="delete-btn btn" type="button">`);
		$(`#ShareManager #Permission-${username} .delete-btn`).append(`<span class="btn-logo fa fa-times"></span>`);
		$(`#ShareManager #Permission-${username} .delete-btn`).each(function ()
		{
			$this = $(this);
			propagating(new Hammer(this)).on("tap", function (e)
			{
				$(`#ShareManager #Permission-${username}`).remove();
				e.preventDefault();
				e.stopPropagation();
			});
		});
	}
}
function AddProgressBar(id, name)
{
	$("#Progress").append(`<div class="container" id="progress-${id}">`);
	$(`#progress-${id}`).append(`<span class="progress-name">${name}</span>`);
	$(`#progress-${id}`).append(`<div class="progress-group">`);
	$(`#progress-${id} .progress-group`).append(`<div class="progress">`);
	$(`#progress-${id} .progress-group .progress`).append(`<div class="progress-bar" style="width: 0%;">`);
	$(`#progress-${id} .progress-group`).append(`<button class="btn cancel-btn">`);
	$(`#progress-${id} .progress-group .cancel-btn`).append(`<span class="cancel-btn-logo fas fa-stop"></span>`);
}
function RemoveProgressBar(id)
{
	$(`#progress-${id}`).remove();
}
function UpdateProgressBar(id, name, percent)
{
	$(`#progress-${id} .progress-name`).text(name);
	$(`#progress-${id} .progress-group .progress .progress-bar`).css("width", `${percent}%`);
	$(`#progress-${id} .progress-group .progress .progress-bar`).text(`${Math.floor(percent)}%`);
}

function InitPathFinder()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	$("#PathFinder").css("top", gnb);
}
function InitExplorer()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var snb = parseInt($("#PathFinder").height()) + (parseInt($("#PathFinder").css("paddingTop")) * 2);
	var padding = parseInt($("#Explorer").css("paddingTop"));

	$("#Explorer").css("top", gnb + snb);
	$("#Explorer").height($(window).height() - (gnb + snb) - (padding * 2));
	var width = $("#Explorer").innerWidth();
	var gap = parseInt($("#Explorer").css("grid-gap"));
	var e_width = 130 + gap;
	PAGE_PER_SCROLL = Math.floor(width / e_width);
	$(window).resize(function ()
	{
		var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
		var snb = parseInt($("#PathFinder").height()) + (parseInt($("#PathFinder").css("paddingTop")) * 2);
		var padding = parseInt($("#Explorer").css("paddingTop"));

		$("#Explorer").css("top", gnb + snb);
		$("#Explorer").height($(window).height() - (gnb + snb) - (padding * 2));
		var x = $("#Explorer").prop("scrollHeight");
		var y = $("#Explorer").innerHeight();
		if (x - 50 <= y) LoadNext();
	});
}
function InitCurtain()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var padding = parseInt($("#Curtain").css("paddingTop"));

	$("#Curtain").css("top", gnb);
	$("#Curtain").height($(window).height() - gnb - (padding * 2));
	$(window).resize(function ()
	{
		var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
		var padding = parseInt($("#Curtain").css("paddingTop"));

		$("#Curtain").height($(window).height() - gnb - (padding * 2));
	});
}
function InitGuide()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var snb = parseInt($("#PathFinder").height()) + (parseInt($("#PathFinder").css("paddingTop")) * 2);
	var padding = parseInt($("#Guide").css("paddingTop"));

	$("#Guide").css("top", gnb + snb);
	$("#Guide").height($(window).height() - (gnb + snb) - (padding * 2));
	$(window).resize(function ()
	{
		var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
		var padding = parseInt($("#Guide").css("paddingTop"));

		$("#Guide").height($(window).height() - gnb - (padding * 2));
	});
}
function InitUploadButton()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var snb = parseInt($("#PathFinder").height()) + (parseInt($("#PathFinder").css("paddingTop")) * 2);

	var width = $("#upload-btn").width();
	var height = $("#upload-btn").height();
	var bound_x = $("#Explorer").width();
	var bound_y = $("#Explorer").height();

	$("#upload-btn").css({
		display: "fixed",
		left: bound_x - width,
		top: bound_y - height + (gnb + snb)
	});
	$(window).resize(function ()
	{
		var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
		var snb = parseInt($("#PathFinder").height()) + (parseInt($("#PathFinder").css("paddingTop")) * 2);

		var width = $("#upload-btn").width();
		var height = $("#upload-btn").height();
		var bound_x = $("#Explorer").width();
		var bound_y = $("#Explorer").height();

		$("#upload-btn").css({
			display: "fixed",
			left: bound_x - width,
			top: bound_y - height + (gnb + snb)
		});
	});
}
function InitFileViewer()
{
	var gnb = $(".gnb").outerHeight();
	var bound = $("#Curtain").innerHeight();
	var height = $("#FileViewer").outerHeight();
	$("#FileViewer").css("top", gnb / 2 + ((bound - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $("#Curtain").innerHeight();
		var height = $("#FileViewer").outerHeight();
		$("#FileViewer").css("top", gnb / 2 + ((bound - height) / 2));
	});
}
function InitShareModeSelector()
{
	var gnb = $(".gnb").outerHeight();
	var bound = $("#Curtain").innerHeight();
	var height = $("#ShareModeSelector").outerHeight();
	$("#ShareModeSelector").css("top", gnb / 2 + ((bound - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $("#Curtain").innerHeight();
		var height = $("#ShareModeSelector").outerHeight();
		$("#ShareModeSelector").css("top", gnb / 2 + ((bound - height) / 2));
	});
}
function InitShareManager()
{
	var gnb = $(".gnb").outerHeight();
	var bound = $("#Curtain").innerHeight();
	var height = $("#ShareManager").outerHeight();
	$("#ShareManager").css("top", gnb / 2 + ((bound - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $("#Curtain").innerHeight();
		var height = $("#ShareManager").outerHeight();
		$("#ShareManager").css("top", gnb / 2 + ((bound - height) / 2));
	});
}
function InitCodeViewer()
{
	var gnb = $(".gnb").outerHeight();
	var bound = $("#Curtain").innerHeight();
	var height = $("#CodeViewer").outerHeight();
	$("#CodeViewer").css("top", gnb / 2 + ((bound - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $("#Curtain").innerHeight();
		var height = $("#CodeViewer").outerHeight();
		$("#CodeViewer").css("top", gnb / 2 + ((bound - height) / 2));
	});
}

function ShowCurtain()
{
	$("#Curtain").fadeIn(DURATION);
	$("#Curtain").css("display", "flex");
}
function ShowLoading()
{
	$("#Loading").fadeIn(DURATION);
	$("#Loading").css("display", "flex");
}
function ShowProgress()
{
	$("#Progress").fadeIn(DURATION);
	$("#Progress").css("display", "flex");
	$("#progress-info").fadeIn(DURATION);
	$("#progress-info").css("display", "flex");
}
function ShowUploadForm()
{
	$(".custom-file-input").val("").trigger("change");
	$(".upload-form").fadeIn(DURATION);
	$(".upload-form").css("display", "flex");
	$("#upload-file").addClass("disabled");
	if (IS_MOBILE) $("#upload-btn").fadeOut(DURATION);
}
function ShowExplorerContextMenu(x, y)
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var snb = parseInt($("#PathFinder").height()) + (parseInt($("#PathFinder").css("paddingTop")) * 2);

	var bound_x = $("#Explorer").width();
	var bound_y = $("#Explorer").height();

	var width = $("#context-menu").width();
	var height = $("#context-menu").height();

	if ((x + width) > bound_x) x -= ((x + width) - bound_x);
	if ((y + height - (gnb + snb)) > bound_y) y -= ((y + height - (gnb + snb)) - bound_y);

	$("#context-menu").css({
		display: "fixed",
		top: y,
		left: x
	});
	$("#selected-path").val(CURRENT_PATH);
	$("#context-menu").fadeIn(DURATION);
	$("#file-context-menu").hide();
}
function ShowFileContextMenu(selector, x, y)
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var snb = parseInt($("#PathFinder").height()) + (parseInt($("#PathFinder").css("paddingTop")) * 2);

	var bound_x = $("#Explorer").width();
	var bound_y = $("#Explorer").height();

	var width = $("#file-context-menu").width();
	var height = $("#file-context-menu").height();

	if ((x + width) > bound_x) x -= ((x + width) - bound_x);
	if ((y + height - (gnb + snb)) > bound_y) y -= ((y + height - (gnb + snb)) - bound_y);

	$("#file-context-menu").css({
		display: "fixed",
		top: y,
		left: x
	});
	$("#file-context-menu").fadeIn(DURATION);
	$("#selected-item-path").val(selector.children(".file-path").val());
	$("#selected-item-name").val(selector.children(".file-name").html());
	$("#selected-item-type").val(selector.children(".file-type").val());
	switch ($("#selected-item-type").val())
	{
		case "DIR": {
			$("#share").show();
			$("#code").hide();
			$("#manage").hide();
			$("#unshare").hide();
			$("#unlink").hide();
			$("#preview").hide();
			$("#download").hide();
			$("#lock").hide();
			$("#unlock").hide();
			break;
		}
		case "PUBLIC_SHARED": {
			$("#share").hide();
			$("#code").show();
			$("#manage").hide();
			$("#unshare").show();
			$("#unlink").hide();
			$("#preview").hide();
			$("#download").hide();
			$("#lock").hide();
			$("#unlock").hide();
			break;
		}
		case "PRIVATE_SHARED": {
			$("#share").hide();
			$("#code").show();
			$("#manage").show();
			$("#unshare").show();
			$("#unlink").hide();
			$("#preview").hide();
			$("#download").hide();
			$("#lock").hide();
			$("#unlock").hide();
			break;
		}
		case "LINKED": {
			$("#share").hide();
			$("#code").hide();
			$("#manage").hide();
			$("#unshare").hide();
			$("#unlink").show();
			$("#preview").hide();
			$("#download").hide();
			$("#lock").hide();
			$("#unlock").hide();
			break;
		}
		case "LINKED_SUB": {
			$("#share").hide();
			$("#code").hide();
			$("#manage").hide();
			$("#unshare").hide();
			$("#unlink").hide();
			$("#preview").hide();
			$("#download").hide();
			$("#lock").hide();
			$("#unlock").hide();
			break;
		}
		case "FILE": {
			$("#share").hide();
			$("#code").hide();
			$("#manage").hide();
			$("#unshare").hide();
			$("#unlink").hide();
			var extension = $("#selected-item-name").val().split(".").pop().toLowerCase();
			switch (extension)
			{
				case "jpg": case "bmp": case "png": case "jpeg": {
					$("#preview").show();
					$("#download").show();
					break;
				}
				case "mov": case "avi": case "mp4": case "mpg": case "mkv": {
					$("#preview").show();
					$("#download").show();
					break;
				}
				case "mp3": case "flac": case "wma": case "wav": case "aac": case "m4a": case "ogg": {
					$("#preview").show();
					$("#download").show();
					break;
				}
				default: {
					$("#preview").hide();
					$("#download").show();
					break;
				}
			}
			if (extension == "locked")
			{
				$("#lock").hide();
				$("#unlock").show();
			}
			else
			{
				$("#lock").show();
				$("#unlock").hide();
			}
			break;
		}
	}
	$("#context-menu").hide();
}
function ShowFileViewer(path, type)
{
	switch (type)
	{
		case "IMG": {
			$("#FileViewer").append(`<img src="Download?path=${path}" />`);
			break;
		}
		case "VID": {
			$("#FileViewer").append(`<video src="Stream?path=${path}" controls controlslist="nodownload" autoplay>`);
			$("#FileViewer video").prop("volume", "0.5");
			break;
		}
		case "AUD": {
			$("#FileViewer").append(`<audio src="Stream?path=${path}" controls controlslist="nodownload" autoplay>`);
			$("#FileViewer audio").prop("volume", "0.5");
			break;
		}
	}
	$("#FileViewer").fadeIn(DURATION);
	$("#FileViewer").css("display", "flex");
	InitFileViewer();
}
function ShowShareModeSelector()
{
	$("#ShareModeSelector").fadeIn(DURATION);
	$("#ShareModeSelector").css("display", "flex");
	InitShareModeSelector();
}
function ShowShareManager(list)
{
	if (list)
	{
		list.forEach(function (item)
		{
			AddPermissionList(item.username, item.create, item.read, item.write, item.remove);
		});
	}
	$("#ShareManager").fadeIn(DURATION);
	$("#ShareManager").css("display", "flex");
	InitShareManager();
}
function ShowCodeViewer(qr, url)
{
	$("#CodeViewer #InnerViewer #QRCode").append(`<img src="${qr}" />`);
	$("#CodeViewer #InnerViewer #UrlArea #Url").val(url);
	$("#CodeViewer").fadeIn(DURATION);
	$("#CodeViewer").css("display", "flex");
	InitCodeViewer();
}

function HideCurtain()
{
	$("#Curtain").fadeOut(DURATION);
	$("#Curtain").css("display", "none");
}
function HideLoading()
{
	$("#Loading").fadeOut(DURATION);
	$("#Loading").css("display", "none");
}
function HideProgress()
{
	$("#Progress").fadeOut(DURATION);
	$("#Progress").css("display", "none");
	$("#progress-info").fadeOut(DURATION);
	$("#progress-info").css("display", "none");
}
function HideContextMenu()
{
	$("#context-menu").hide();
}
function HideFileContextMenu()
{
	$("#file-context-menu").hide();
}
function HideAllContextMenu()
{
	$("#context-menu").hide();
	$("#file-context-menu").hide();
}
function HideUploadForm()
{
	$(".upload-form").fadeOut(DURATION);
	$("#upload-file").removeClass("disabled");
	if (IS_MOBILE) $("#upload-btn").fadeIn(DURATION);
}
function HideFileViewer()
{
	$("#FileViewer img").remove();
	$("#FileViewer video").remove();
	$("#FileViewer audio").remove();
	$("#FileViewer").fadeOut(DURATION);
	$("#FileViewer").css("display", "none");
}
function HideShareModeSelector()
{
	$("#ShareModeSelector").fadeOut(DURATION);
	$("#ShareModeSelector").css("display", "none");
}
function HideShareManager()
{
	$("#ShareManager .search-text").val('');
	$("#ShareManager .list-group").html('');
	$("#ShareManager").fadeOut(DURATION);
	$("#ShareManager").css("display", "none");
}
function HideCodeViewer()
{
	$("#CodeViewer").fadeOut(DURATION);
	$("#CodeViewer").css("display", "none");
	$("#CodeViewer #InnerViewer #QRCode").html("");
	$("#CodeViewer #InnerViewer #UrlArea #Url").val("");
}

function UpdateIndicator(list, link)
{
	$(".indicator").css("display", "none");
	$(".indicator").html('');
	list.forEach(function (item, index)
	{
		$(".indicator").append(`<li class="indicator-path" id="indicator-path-${index}">`);
		$(`#indicator-path-${index}`).append(`<span class="path-icon fa fa-folder"></span>`);
		$(`#indicator-path-${index}`).append(`<span class="path-name">${item}</span>`);
		$(`#indicator-path-${index}`).append(`<input class="path-data" type="hidden" value="${link[index]}">`);
		$(".indicator").append(`<span class="path-separator">></span>`);
		if (index < list.length - 1)
		{
			$(`#indicator-path-${index}`).each(function ()
			{
				$this = $(this);
				var mc = new Hammer.Manager(this);
				mc.add(new Hammer.Tap());
				propagating(mc).on("tap", function (e)
				{
					HideAllContextMenu();
					var path = $(`#indicator-path-${index} .path-data`).val();
					if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
					if (NEXT_STACK.length > 0) NEXT_STACK = [];
					update_btn_status();
					Ajax("List", {
						path: path
					}, result_list);
					e.preventDefault();
					e.preventDefault();
				});
			});
		}
		$(".indicator-path").droppable({
			over: function (event, ui)
			{
				$(this).addClass("selected");
				$(this).addClass("dropped");
			},
			out: function (event, ui)
			{
				$(this).removeClass("selected");
				$(this).removeClass("dropped");
			},
			drop: function (event, ui)
			{
				$(this).removeClass("selected");
				$(this).removeClass("dropped");
				var srcPath = $(ui.draggable).find(".file-path").val();
				var dstPath = $(this).find(".path-data").val();
				var type = $(ui.draggable).find(".file-type").val();
				Ajax("Move", {
					srcPath: srcPath,
					dstPath: dstPath,
					type: type
				}, result_move);
			}
		});
	});
	$(".indicator").css("display", "flex");
}
function UpdateExplorer(list)
{
	if (list.length > 0)
	{
		list.forEach(function (item)
		{
			var key = item.path;
			switch (item.type)
			{
				case "DIR": {
					$("#Explorer").append(`<li class="folder" id="folder-${md5(key)}">`);
					$(`#folder-${md5(key)}`).append(`<div class="folder-icon-group"></div>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon1 fas fa-folder"></span>`);
					$(`#folder-${md5(key)}`).append(`<span class="file-name">${item.name}</span>`);
					$(`#folder-${md5(key)}`).append(`<input class="file-path" type="hidden" value="${item.path}" />`);
					$(`#folder-${md5(key)}`).append(`<input class="file-type" type="hidden" value=${item.type} />`);
					$(`#folder-${md5(key)}`).each(function ()
					{
						$this = $(this);
						var mc = new Hammer.Manager(this);
						$(this).droppable({
							over: function (event, ui)
							{
								$(this).addClass("selected");
							},
							out: function (event, ui)
							{
								$(this).removeClass("selected");
							},
							drop: function (event, ui)
							{
								$(this).removeClass("selected");
								var srcPath = $(ui.draggable).find(".file-path").val();
								var dstPath = $(this).find(".file-path").val();
								var type = $(ui.draggable).find(".file-type").val();
								Ajax("Move", {
									srcPath: srcPath,
									dstPath: dstPath,
									type: type
								}, result_move);
							}
						});
						if (IS_MOBILE)
						{
							mc.add(new Hammer.Tap());
							mc.add(new Hammer.Press({ event: "press", time: 300, threshold: 30 }));
							mc.add(new Hammer.Press({ event: "longpress", time: 1000, threshold: 30 }));
							propagating(mc).on("tap", function (e)
							{
								HideAllContextMenu();
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("longpress", function (e)
							{
								HideAllContextMenu();
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								$(`#folder-${md5(key)}`).addClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("pressup", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("press", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								ShowFileContextMenu($(`#folder-${md5(key)}`), e.center.x, e.center.y);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									if (!$(this).hasClass("dragged")) return false;
									else
									{
										$(".folder").removeClass("selected");
										$(".file").removeClass("selected");
										$(this).addClass("selected");
										$(this).addClass("dragged");
										event.stopPropagation();
									}
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
						else
						{
							mc.add(new Hammer.Tap({ event: "doubletap", taps: 2 }));
							propagating(mc).on("tap", function (e)
							{
								if (!$(`#folder-${md5(key)}`).hasClass("selected"))
								{
									$(`.folder`).removeClass("selected");
									$(`#folder-${md5(key)}`).addClass("selected");
								}
								HideAllContextMenu();
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("doubletap", function (e)
							{
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).mouseover(function ()
							{
								$(this).addClass("hovered");
							});
							$(this).mouseout(function ()
							{
								$(this).removeClass("hovered");
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									$(".folder").removeClass("selected");
									$(".file").removeClass("selected");
									$(this).addClass("selected");
									$(this).addClass("dragged");
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
					});
					break;
				}
				case "PUBLIC_SHARED": {
					$("#Explorer").append(`<li class="folder" id="folder-${md5(key)}">`);
					$(`#folder-${md5(key)}`).append(`<div class="folder-icon-group"></div>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon1 fas fa-folder"></span>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon2 fas fa-user-friends"></span>`);
					$(`#folder-${md5(key)}`).append(`<span class="file-name">${item.name}</span>`);
					$(`#folder-${md5(key)}`).append(`<input class="file-path" type="hidden" value="${item.path}" />`);
					$(`#folder-${md5(key)}`).append(`<input class="file-type" type="hidden" value=${item.type} />`);
					$(`#folder-${md5(key)}`).each(function ()
					{
						$this = $(this);
						$(this).droppable({
							over: function (event, ui)
							{
								$(this).addClass("selected");
							},
							out: function (event, ui)
							{
								$(this).removeClass("selected");
							},
							drop: function (event, ui)
							{
								$(this).removeClass("selected");
								var srcPath = $(ui.draggable).find(".file-path").val();
								var dstPath = $(this).find(".file-path").val();
								var type = $(ui.draggable).find(".file-type").val();
								Ajax("Move", {
									srcPath: srcPath,
									dstPath: dstPath,
									type: type
								}, result_move);
							}
						});
						var mc = new Hammer.Manager(this);
						if (IS_MOBILE)
						{
							mc.add(new Hammer.Tap());
							mc.add(new Hammer.Press({ event: "press", time: 300, threshold: 30 }));
							mc.add(new Hammer.Press({ event: "longpress", time: 1000, threshold: 30 }));
							propagating(mc).on("tap", function (e)
							{
								HideAllContextMenu();
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("longpress", function (e)
							{
								HideAllContextMenu();
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								$(`#folder-${md5(key)}`).addClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("pressup", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("press", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								ShowFileContextMenu($(`#folder-${md5(key)}`), e.center.x, e.center.y);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									if (!$(this).hasClass("dragged")) return false;
									else
									{
										$(".folder").removeClass("selected");
										$(".file").removeClass("selected");
										$(this).addClass("selected");
										$(this).addClass("dragged");
										event.stopPropagation();
									}
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
						else
						{
							mc.add(new Hammer.Tap({ event: "doubletap", taps: 2 }));
							propagating(mc).on("tap", function (e)
							{
								if (!$(`#folder-${md5(key)}`).hasClass("selected"))
								{
									$(`.folder`).removeClass("selected");
									$(`#folder-${md5(key)}`).addClass("selected");
								}
								HideAllContextMenu();
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("doubletap", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("selected");
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).mouseover(function ()
							{
								$(this).addClass("hovered");
							});
							$(this).mouseout(function ()
							{
								$(this).removeClass("hovered");
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									$(".folder").removeClass("selected");
									$(".file").removeClass("selected");
									$(this).addClass("selected");
									$(this).addClass("dragged");
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
					});
					break;
				}
				case "PRIVATE_SHARED": {
					$("#Explorer").append(`<li class="folder" id="folder-${md5(key)}">`);
					$(`#folder-${md5(key)}`).append(`<div class="folder-icon-group"></div>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon1 fas fa-folder"></span>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon2 fas fa-user"></span>`);
					$(`#folder-${md5(key)}`).append(`<span class="file-name">${item.name}</span>`);
					$(`#folder-${md5(key)}`).append(`<input class="file-path" type="hidden" value="${item.path}" />`);
					$(`#folder-${md5(key)}`).append(`<input class="file-type" type="hidden" value=${item.type} />`);
					$(`#folder-${md5(key)}`).each(function ()
					{
						$this = $(this);
						$(this).droppable({
							over: function (event, ui)
							{
								$(this).addClass("selected");
							},
							out: function (event, ui)
							{
								$(this).removeClass("selected");
							},
							drop: function (event, ui)
							{
								$(this).removeClass("selected");
								var srcPath = $(ui.draggable).find(".file-path").val();
								var dstPath = $(this).find(".file-path").val();
								var type = $(ui.draggable).find(".file-type").val();
								Ajax("Move", {
									srcPath: srcPath,
									dstPath: dstPath,
									type: type
								}, result_move);
							}
						});
						var mc = new Hammer.Manager(this);
						if (IS_MOBILE)
						{
							mc.add(new Hammer.Tap());
							mc.add(new Hammer.Press({ event: "press", time: 300, threshold: 30 }));
							mc.add(new Hammer.Press({ event: "longpress", time: 1000, threshold: 30 }));
							propagating(mc).on("tap", function (e)
							{
								HideAllContextMenu();
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("longpress", function (e)
							{
								HideAllContextMenu();
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								$(`#folder-${md5(key)}`).addClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("pressup", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("press", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								ShowFileContextMenu($(`#folder-${md5(key)}`), e.center.x, e.center.y);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									if (!$(this).hasClass("dragged")) return false;
									else
									{
										$(".folder").removeClass("selected");
										$(".file").removeClass("selected");
										$(this).addClass("selected");
										$(this).addClass("dragged");
										event.stopPropagation();
									}
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
						else
						{
							mc.add(new Hammer.Tap({ event: "doubletap", taps: 2 }));
							propagating(mc).on("tap", function (e)
							{
								if (!$(`#folder-${md5(key)}`).hasClass("selected"))
								{
									$(`.folder`).removeClass("selected");
									$(`#folder-${md5(key)}`).addClass("selected");
								}
								HideAllContextMenu();
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("doubletap", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("selected");
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).mouseover(function ()
							{
								$(this).addClass("hovered");
							});
							$(this).mouseout(function ()
							{
								$(this).removeClass("hovered");
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									$(".folder").removeClass("selected");
									$(".file").removeClass("selected");
									$(this).addClass("selected");
									$(this).addClass("dragged");
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
					});
					break;
				}
				case "LINKED": {
					$("#Explorer").append(`<li class="folder" id="folder-${md5(key)}">`);
					$(`#folder-${md5(key)}`).append(`<div class="folder-icon-group"></div>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon1 fas fa-folder"></span>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon2 fas fa-link"></span>`);
					$(`#folder-${md5(key)}`).append(`<span class="file-name">${item.name}</span>`);
					$(`#folder-${md5(key)}`).append(`<input class="file-path" type="hidden" value="${item.path}" />`);
					$(`#folder-${md5(key)}`).append(`<input class="file-type" type="hidden" value=${item.type} />`);
					$(`#folder-${md5(key)}`).each(function ()
					{
						$this = $(this);
						$(this).droppable({
							over: function (event, ui)
							{
								$(this).addClass("selected");
							},
							out: function (event, ui)
							{
								$(this).removeClass("selected");
							},
							drop: function (event, ui)
							{
								$(this).removeClass("selected");
								var srcPath = $(ui.draggable).find(".file-path").val();
								var dstPath = $(this).find(".file-path").val();
								var type = $(ui.draggable).find(".file-type").val();
								Ajax("Move", {
									srcPath: srcPath,
									dstPath: dstPath,
									type: type
								}, result_move);
							}
						});
						var mc = new Hammer.Manager(this);
						if (IS_MOBILE)
						{
							mc.add(new Hammer.Tap());
							mc.add(new Hammer.Press({ event: "press", time: 300, threshold: 30 }));
							mc.add(new Hammer.Press({ event: "longpress", time: 1000, threshold: 30 }));
							propagating(mc).on("tap", function (e)
							{
								HideAllContextMenu();
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("longpress", function (e)
							{
								HideAllContextMenu();
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								$(`#folder-${md5(key)}`).addClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("pressup", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("press", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								ShowFileContextMenu($(`#folder-${md5(key)}`), e.center.x, e.center.y);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									if (!$(this).hasClass("dragged")) return false;
									else
									{
										$(".folder").removeClass("selected");
										$(".file").removeClass("selected");
										$(this).addClass("selected");
										$(this).addClass("dragged");
										event.stopPropagation();
									}
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
						else
						{
							mc.add(new Hammer.Tap({ event: "doubletap", taps: 2 }));
							propagating(mc).on("tap", function (e)
							{
								if (!$(`#folder-${md5(key)}`).hasClass("selected"))
								{
									$(`.folder`).removeClass("selected");
									$(`#folder-${md5(key)}`).addClass("selected");
								}
								HideAllContextMenu();
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("doubletap", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("selected");
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).mouseover(function ()
							{
								$(this).addClass("hovered");
							});
							$(this).mouseout(function ()
							{
								$(this).removeClass("hovered");
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									$(".folder").removeClass("selected");
									$(".file").removeClass("selected");
									$(this).addClass("selected");
									$(this).addClass("dragged");
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
					});
					break;
				}
				case "LINKED_SUB": {
					$("#Explorer").append(`<li class="folder" id="folder-${md5(key)}">`);
					$(`#folder-${md5(key)}`).append(`<div class="folder-icon-group"></div>`);
					$(`#folder-${md5(key)} .folder-icon-group`).append(`<span class="folder-icon1 fas fa-folder"></span>`);
					$(`#folder-${md5(key)}`).append(`<span class="file-name">${item.name}</span>`);
					$(`#folder-${md5(key)}`).append(`<input class="file-path" type="hidden" value="${item.path}" />`);
					$(`#folder-${md5(key)}`).append(`<input class="file-type" type="hidden" value=${item.type} />`);
					$(`#folder-${md5(key)}`).each(function ()
					{
						$this = $(this);
						$(this).droppable({
							over: function (event, ui)
							{
								$(this).addClass("selected");
							},
							out: function (event, ui)
							{
								$(this).removeClass("selected");
							},
							drop: function (event, ui)
							{
								$(this).removeClass("selected");
								var srcPath = $(ui.draggable).find(".file-path").val();
								var dstPath = $(this).find(".file-path").val();
								var type = $(ui.draggable).find(".file-type").val();
								Ajax("Move", {
									srcPath: srcPath,
									dstPath: dstPath,
									type: type
								}, result_move);
							}
						});
						var mc = new Hammer.Manager(this);
						if (IS_MOBILE)
						{
							mc.add(new Hammer.Tap());
							mc.add(new Hammer.Press({ event: "press", time: 300, threshold: 30 }));
							mc.add(new Hammer.Press({ event: "longpress", time: 1000, threshold: 30 }));
							propagating(mc).on("tap", function (e)
							{
								HideAllContextMenu();
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("longpress", function (e)
							{
								HideAllContextMenu();
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								$(`#folder-${md5(key)}`).addClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("pressup", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("press", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("dragged");
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#folder-${md5(key)}`).addClass("selected");
								ShowFileContextMenu($(`#folder-${md5(key)}`), e.center.x, e.center.y);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									if (!$(this).hasClass("dragged")) return false;
									else
									{
										$(".folder").removeClass("selected");
										$(".file").removeClass("selected");
										$(this).addClass("selected");
										$(this).addClass("dragged");
										event.stopPropagation();
									}
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
						else
						{
							mc.add(new Hammer.Tap({ event: "doubletap", taps: 2 }));
							propagating(mc).on("tap", function (e)
							{
								if (!$(`#folder-${md5(key)}`).hasClass("selected"))
								{
									$(`.folder`).removeClass("selected");
									$(`#folder-${md5(key)}`).addClass("selected");
								}
								HideAllContextMenu();
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("doubletap", function (e)
							{
								$(`#folder-${md5(key)}`).removeClass("selected");
								var path = $(`#folder-${md5(key)} .file-path`).val();
								if (CURRENT_PATH) PREV_STACK.push(CURRENT_PATH);
								if (NEXT_STACK.length > 0) NEXT_STACK = [];
								update_btn_status();
								Ajax("List", {
									path: path
								}, result_list);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).mouseover(function ()
							{
								$(this).addClass("hovered");
							});
							$(this).mouseout(function ()
							{
								$(this).removeClass("hovered");
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									$(".folder").removeClass("selected");
									$(".file").removeClass("selected");
									$(this).addClass("selected");
									$(this).addClass("dragged");
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
					});
					break;
				}
				case "FILE": {
					var extension = key.split(".").pop();
					$("#Explorer").append(`<li class="file" id="file-${md5(key)}">`);
					$(`#file-${md5(key)}`).append(`<div class="file-preview"></div>`);
					switch (extension.toLowerCase())
					{
						case "jpg": case "bmp": case "png": case "jpeg": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-image"></span>`);
							AjaxSilent("Thumbnail", {
								path: item.path,
								media: "IMG"
							}, function (result) { thumbnail_onLoad(result, "IMG") });
							break;
						}
						case "mov": case "avi": case "mp4": case "mpg": case "mkv": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-video"></span>`);
							AjaxSilent("Thumbnail", {
								path: item.path,
								media: "VID"
							}, function (result) { thumbnail_onLoad(result, "VID") });
							break;
						}
						case "mp3": case "flac": case "wma": case "wav": case "aac": case "m4a": case "ogg": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-audio"></span>`);
							break;
						}
						case "doc": case "docx": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-word"></span>`);
							break;
						}
						case "xls": case "xlsx": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-excel"></span>`);
							break;
						}
						case "ppt": case "pptx": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-powerpoint"></span>`);
							break;
						}
						case "pdf": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-pdf"></span>`);
							break;
						}
						case "egg": case "alz": case "zip": case "rar": case "7z": case "cab": case "tar": case "gz": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-archive"></span>`);
							break;
						}
						case "cs": case "h": case "c": case "cpp": case "java": case "jsp": case "html": case "xml": case "css": case "js": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-code"></span>`);
							break;
						}
						case "csv": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-csv"></span>`);
							break;
						}
						case "exe": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file"></span>`);
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon2 fab fa-windows"></span>`);
							break;
						}
						case "iso": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file"></span>`);
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon2 fas fa-compact-disc"></span>`);
							break;
						}
						case "txt": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file-alt"></span>`);
							break;
						}
						case "locked": {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file"></span>`);
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon2 fas fa-lock"></span>`);
							break;
						}
						default: {
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon1 fas fa-file"></span>`);
							$(`#file-${md5(key)} .file-preview`).append(`<span class="file-icon2">.${extension}</span>`);
							break;
						}
					}
					$(`#file-${md5(key)}`).append(`<span class="file-name">${item.name}</span>`);
					$(`#file-${md5(key)}`).append(`<input class="file-size" type="hidden" value="${item.size}" />`);
					$(`#file-${md5(key)}`).append(`<input class="file-path" type="hidden" value="${item.path}" />`);
					$(`#file-${md5(key)}`).append(`<input class="file-type" type="hidden" value=${item.type} />`);
					$(`#file-${md5(key)}`).each(function ()
					{
						$this = $(this);
						var mc = new Hammer.Manager(this);
						if (IS_MOBILE)
						{
							mc.add(new Hammer.Tap());
							mc.add(new Hammer.Press({ event: "press", time: 300, threshold: 30 }));
							mc.add(new Hammer.Press({ event: "longpress", time: 1000, threshold: 30 }));
							propagating(mc).on("tap", function (e)
							{
								HideAllContextMenu();
								$(`#file-${md5(key)}`).removeClass("selected");
								var path = $(`#file-${md5(key)} .file-path`).val();
								var extension = path.split(".").pop().toLowerCase();
								switch (extension)
								{
									case "jpg": case "bmp": case "png": case "jpeg": {
										ShowCurtain();
										ShowFileViewer(path, "IMG");
										break;
									}
									case "mov": case "avi": case "mp4": case "mpg": case "mkv": {
										ShowCurtain();
										ShowFileViewer(path, "VID");
										break;
									}
									case "mp3": case "flac": case "wma": case "wav": case "aac": case "m4a": case "ogg": {
										ShowCurtain();
										ShowFileViewer(path, "AUD");
										break;
									}
									case "locked": {
										setTimeout(function ()
										{
											alert("파일이 잠겨있습니다.");
										}, 0);
										break;
									}
									default: {
										setTimeout(function ()
										{
											if (confirm("미리보기를 지원하지 않는 파일입니다.\n다운로드 받으시겠습니까?"))
												window.location.href = `Download?path=${path}`;
										}, 0);
										break;
									}
								}
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("longpress", function (e)
							{
								HideAllContextMenu();
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#file-${md5(key)}`).addClass("selected");
								$(`#file-${md5(key)}`).addClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("pressup", function (e)
							{
								$(`#file-${md5(key)}`).removeClass("dragged");
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("press", function (e)
							{
								$(`#file-${md5(key)}`).removeClass("dragged");
								$(".folder").removeClass("selected");
								$(".file").removeClass("selected");
								$(`#file-${md5(key)}`).addClass("selected");
								ShowFileContextMenu($(`#file-${md5(key)}`), e.center.x, e.center.y);
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									if (!$(this).hasClass("dragged")) return false;
									else
									{
										$(".folder").removeClass("selected");
										$(".file").removeClass("selected");
										$(this).addClass("selected");
										$(this).addClass("dragged");
										event.stopPropagation();
									}
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
						else
						{
							mc.add(new Hammer.Tap({ event: "doubletap", taps: 2 }));
							propagating(mc).on("tap", function (e)
							{
								if (!$(`#file-${md5(key)}`).hasClass("selected"))
								{
									$(`.file`).removeClass("selected");
									$(`#file-${md5(key)}`).addClass("selected");
								}
								HideAllContextMenu();
								e.preventDefault();
								e.stopPropagation();
							});
							propagating(mc).on("doubletap", function (e)
							{
								var path = $(`#file-${md5(key)} .file-path`).val();
								var extension = path.split(".").pop().toLowerCase();
								switch (extension)
								{
									case "jpg": case "bmp": case "png": case "jpeg": {
										ShowCurtain();
										ShowFileViewer(path, "IMG");
										break;
									}
									case "mov": case "avi": case "mp4": case "mpg": case "mkv": {
										ShowCurtain();
										ShowFileViewer(path, "VID");
										break;
									}
									case "mp3": case "flac": case "wma": case "wav": case "aac": case "m4a": case "ogg": {
										ShowCurtain();
										ShowFileViewer(path, "AUD");
										break;
									}
									case "locked": {
										setTimeout(function ()
										{
											alert("파일이 잠겨있습니다.");
										}, 0);
										break;
									}
									default: {
										setTimeout(function ()
										{
											if (confirm("미리보기를 지원하지 않는 파일입니다.\n다운로드 받으시겠습니까?"))
												window.location.href = `Download?path=${path}`;
										}, DELAY);
										break;
									}
								}
								e.preventDefault();
								e.stopPropagation();
							});
							$(this).mouseover(function ()
							{
								$(this).addClass("hovered");
							});
							$(this).mouseout(function ()
							{
								$(this).removeClass("hovered");
							});
							$(this).draggable({
								containment: "window",
								distance: 30,
								revert: function (event, ui)
								{
									if (event == false)
									{
										isRevert = false;
										return true;
									}
									else isRevert = true;
								},
								start: function (event, ui)
								{
									$(".folder").removeClass("selected");
									$(".file").removeClass("selected");
									$(this).addClass("selected");
									$(this).addClass("dragged");
								},
								stop: function (event, ui)
								{
									$(this).removeClass("dragged");
								}
							});
						}
					});
					break;
				}
			}
		});
	}
}
function ClearExplorer()
{
	$("#Explorer").html('');
}

function thumbnail_onLoad(result, media)
{
	if (result.success == "SUCCESS")
	{
		$(`#file-${md5(result.file.path)} .file-preview .file-icon1`).css("display", "none");
		$(`#file-${md5(result.file.path)} .file-preview .file-icon2`).css("display", "none");
		$(`#file-${md5(result.file.path)} .file-preview:not(:has(img))`).append(`<img class="file-image" src="${result.file.thumbnail}" ondragstart="return false" />`);
		if (media == "VID") $(`#file-${md5(result.file.path)} .file-preview:has(img)`).append(`<span class="file-playable far fa-play-circle"></span>`);
	}
}

function GoToHome()
{
	Ajax("List", {
		path: "Home"
	}, result_list);
}
function Refresh()
{
	Ajax("List", {}, result_list);
}
function EnablePaste()
{
	$("#paste").removeClass("disabled");
}
function DisablePaste()
{
	$("#paste").addClass("disabled");
}

function uploadFiles(action, files, second, success, progress, error, canceled)
{
	UPLOAD_QUEUE = 0;
	TOTAL_SIZE = 0;
	UPLOAD_HANDLES.push(window.setInterval(function ()
	{
		second();
	}, 1000));
	UPLOAD_HANDLES.push(window.setInterval(function ()
	{
		if (UPLOAD_QUEUE < MAX_QUEUE)
		{
			var available = MAX_QUEUE - UPLOAD_QUEUE;
			var count = 0;
			for (var i = 0; i < UPLOAD_LIST.length; i++)
			{
				if (UPLOAD_LIST[i].status == "READY")
				{
					UPLOAD_LIST[i].status = "GO";
					UPLOAD_QUEUE++;
					upload("Upload", UPLOAD_LIST[i].file, upload_onCompleted, upload_onProgress, upload_onError, upload_onCanceled);
					count++;
				}
				if (count >= available) break;
			}
		}
	}, 100));
	for (var i = 0; i < files.length; i++)
	{
		if (files[i].name.split(".").pop() == "locked")
		{
			setTimeout(function ()
			{
				alert("locked 확장자를 가진 파일은 업로드할 수 없습니다.");
				HideLoading();
				HideCurtain();
			}, 0);
		}
		else if (files[i].size == 0)
		{
			setTimeout(function ()
			{
				alert("크기가 0인 파일은 업로드할 수 없습니다.");
				HideLoading();
				HideCurtain();
			}, 0);
		}
		else
		{
			JaxSilent("StartUpload", {
				path: CURRENT_PATH,
				file: files[i].name,
				overwrite: false
			},
				function (result)
				{
					switch (result.success)
					{
						case "SUCCESS": {
							HideLoading();
							ShowProgress();
							UPLOAD_DICT[`${files[i].name.normalize("NFC")}`] = i;
							UPLOAD_LIST.push({ file: files[i], status: "READY" });
							TOTAL_SIZE += files[i].size;
							AddProgressBar(i, files[i].name.normalize("NFC"));
							if (UPLOAD_QUEUE < MAX_QUEUE)
							{
								UPLOAD_QUEUE++;
								UPLOAD_LIST[i].status = "GO";
							}
							upload(action, files[i], success, progress, error, canceled);
							break;
						}
						case "ALREADY_EXISTS": {
							if (confirm(`${files[i].name} 파일이 이미 존재합니다.\n덮어 쓰시겠습니까?`))
							{
								JaxSilent("StartUpload", {
									path: CURRENT_PATH,
									file: files[i].name,
									overwrite: true
								},
									function ()
									{
										HideLoading();
										ShowProgress();
										UPLOAD_DICT[`${files[i].name.normalize("NFC")}`] = i;
										UPLOAD_LIST.push({ file: files[i], status: "READY" });
										TOTAL_SIZE += files[i].size;
										AddProgressBar(i, files[i].name.normalize("NFC"));
										if (UPLOAD_QUEUE < MAX_QUEUE)
										{
											UPLOAD_QUEUE++;
											UPLOAD_LIST[i].status = "GO";
										}
										upload(action, files[i], success, progress, error, canceled);
									});
							}
							else
							{
								HideProgress();
								HideLoading();
								HideCurtain();
							}
							break;
						}
						case "UPLOAD_EXISTS": {
							if (confirm(`${files[i].name} 파일에 대해 이전에 중지된 업로드가 있습니다.\n이어서 진행하시겠습니까?`))
							{
								JaxSilent("StartUpload", {
									path: CURRENT_PATH,
									file: files[i].name,
									overwrite: true
								},
									function ()
									{
										HideLoading();
										ShowProgress();
										UPLOAD_DICT[`${files[i].name.normalize("NFC")}`] = i;
										UPLOAD_LIST.push({ file: files[i], status: "READY" });
										TOTAL_SIZE += files[i].size;
										AddProgressBar(i, files[i].name.normalize("NFC"));
										if (UPLOAD_QUEUE < MAX_QUEUE)
										{
											UPLOAD_QUEUE++;
											UPLOAD_LIST[i].status = "GO";
										}
										upload(action, files[i], success, progress, error, canceled);
									});
							}
							else
							{
								HideProgress();
								HideLoading();
								HideCurtain();
							}
							break;
						}
					}
				});
		}
	}
}
function upload(action, file, success, progress, error, canceled)
{
	if (UPLOAD_LIST[UPLOAD_DICT[`${file.name.normalize("NFC")}`]].status == "GO")
	{
		var chunks = [];
		var currentPosition = 0;
		var endPosition = CHUNK_SIZE;
		var size = file.size;

		while (currentPosition < size)
		{
			chunks.push(file.slice(currentPosition, endPosition));
			currentPosition = endPosition;
			endPosition = currentPosition + CHUNK_SIZE;
		}
		uploadChunk(action, chunks, file.name, 1, chunks.length, success, progress, error, canceled);
	}
}
function uploadChunk(action, chunks, name, current, total, success, progress, error, canceled)
{
	name = name.normalize("NFC");
	if (UPLOAD_LIST[UPLOAD_DICT[`${name}`]].status == "GO")
	{
		var formData = new FormData();
		formData.append("file", chunks[current - 1], name);
		$.ajax({
			url: `/File/${action}`,
			type: "POST",
			contentType: false,
			processData: false,
			data: formData,
			success: function (result)
			{
				if (current <= total)
				{
					if (result.status)
					{
						if (total == current) success(result);
						else
						{
							progress(name, chunks[current].size, current, total);
							uploadChunk(action, chunks, name, current + 1, total, success, progress, error, canceled);
						}
					}
					else error(name);
				}
			}
		});
	}
	else if (UPLOAD_LIST[UPLOAD_DICT[`${name}`]].status == "STOP") canceled(name);
}

function upload_onFinished()
{
	UPLOAD_LIST = [];
	UPLOAD_HANDLES.forEach(function (item)
	{
		window.clearInterval(item);
	});
	HideProgress();
	HideCurtain();
	setTimeout(function ()
	{
		Refresh();
	}, DELAY);
}
function upload_onCompleted(result)
{
	result.name = result.name.normalize("NFC");
	UPLOAD_QUEUE--;
	RemoveProgressBar(UPLOAD_DICT[`${result.name}`])
	delete UPLOAD_DICT[`${result.name}`];
	AjaxSilent("FinishUpload", {
		path: CURRENT_PATH,
		file: result.name
	}, function (result)
	{
		switch (result.success)
		{
			case "NO_SUCH_DIRECTORY": {
				setTimeout(function ()
				{
					alert("삭제되었거나 존재하지 않는 경로입니다.");
				}, 0);
			}
		}
	});
	if (Object.keys(UPLOAD_DICT).length == 0) upload_onFinished();
}
function upload_onSecond()
{
	$("#upload-remain").text(`남은 파일 : ${Object.keys(UPLOAD_DICT).length}`);
	if (BANDWIDTH != 0)
	{
		TOTAL_SIZE -= BANDWIDTH;
		var time = Math.floor(TOTAL_SIZE / BANDWIDTH);
		var sec = time % 60;
		var min = Math.floor(time / 60);
		var hour = Math.floor(min / 60);

		if (BANDWIDTH >= (1024 * 1024)) $("#upload-speed").text(`전송 속도 : ${(BANDWIDTH / 1024 / 1024).toFixed(1)} MB/s`);
		else if (BANDWIDTH >= (1024)) $("#upload-speed").text(`전송 속도 : ${(BANDWIDTH / 1024).toFixed(1)} KB/s`);

		if (hour == 0 && min == 0) $("#upload-eta").text(`남은 시간 : ${sec}초`);
		else if (hour == 0) $("#upload-eta").text(`남은 시간 : ${min}분 ${sec}초`);
		else $("#upload-eta").text(`남은 시간 : ${hour}시간 ${min}분 ${sec}초`);
		BANDWIDTH = 0;
	}
}
function upload_onProgress(name, chunk, current, total)
{
	name = name.normalize("NFC");
	ShowProgress();
	BANDWIDTH += chunk;

	var percent = (current / total) * 100;
	UpdateProgressBar(UPLOAD_DICT[`${name}`], name, percent);
}
function upload_onError(name)
{
	name = name.normalize("NFC");
	UPLOAD_QUEUE--;
	setTimeout(function ()
	{
		alert(`${name}의 전송에 실패했습니다.`);
		RemoveProgressBar(UPLOAD_DICT[`${name}`]);
		delete UPLOAD_DICT[`${name}`];
		AjaxSilent("CancelUpload", {
			path: CURRENT_PATH,
			file: name
		}, function () { });
		if (Object.keys(UPLOAD_DICT).length == 0) upload_onFinished();
	}, 0);
}
function upload_onCanceled(name)
{
	name = name.normalize("NFC");
	UPLOAD_QUEUE--;
	TOTAL_SIZE -= UPLOAD_LIST[UPLOAD_DICT[`${name}`]].file.size;
	RemoveProgressBar(UPLOAD_DICT[`${name}`])
	delete UPLOAD_DICT[`${name}`];
	AjaxSilent("CancelUpload", {
		path: CURRENT_PATH,
		file: name
	}, function () { });
	if (Object.keys(UPLOAD_DICT).length == 0) upload_onFinished();
}

function result_list(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			CURRENT_PATH = result.current;
			SCROLL_UPDATE = true;
			CURRENT_PAGE = 0;
			EXPLORER_LIST = result.list;
			var pathList = result.pathList;
			if (result.list.length == 0) $("#Explorer").css("background", "rgba(252, 252, 252, 0.67)");
			else $("#Explorer").css("background", "#FCFCFC");

			// 컨텍스트 메뉴 가리기
			HideAllContextMenu();
			// 인디케이터 초기화
			UpdateIndicator(pathList, result.pathLink);
			EXPLORER_COUNT = 0;
			ClearExplorer();
			var width = $("#Explorer").innerWidth();
			var gap = parseInt($("#Explorer").css("grid-gap"));
			var e_width = 130 + gap;
			PAGE_PER_SCROLL = Math.floor(width / e_width);
			var list = EXPLORER_LIST.slice(0, PAGE_PER_SCROLL);
			UpdateExplorer(list);
			CURRENT_PAGE++;
			var x = $("#Explorer").prop("scrollHeight");
			var y = $("#Explorer").innerHeight();
			if (x - 50 <= y) LoadNext();
			break;
		}
		case "NO_SUCH_DIRECTORY": {
			setTimeout(function ()
			{
				alert("삭제되었거나 존재하지 않는 경로입니다.");
				GoToHome();
			}, 0);
			break;
		}
	}
}
function result_create(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			Refresh();
			break;
		}
		case "NO_SUCH_DIRECTORY": {
			setTimeout(function ()
			{
				alert("삭제되었거나 존재하지 않는 경로입니다.");
				Refresh();
			}, 0);
			break;
		}
		case "ALREADY_EXISTS": {
			setTimeout(function ()
			{
				alert("이미 존재하는 이름입니다.");
			}, 0);
			break;
		}
	}
}
function result_rename(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			if (!result.clipboard) DisablePaste();
			Refresh();
			break;
		}
		case "ALREADY_EXISTS": {
			setTimeout(function ()
			{
				alert("이미 존재하는 이름입니다.");
			}, 0);
			break;
		}
		case "FILE_LOCKED": {
			setTimeout(function ()
			{
				alert("현재 다른 세션에서 사용 중인 파일입니다.");
			}, 0);
			break;
		}
	}
}
function result_delete(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			if (!result.clipboard) DisablePaste();
			Refresh();
			break;
		}
		case "NO_SUCH_DIRECTORY": {
			setTimeout(function ()
			{
				alert("존재하지 않는 이름입니다.");
			}, 0);
			break;
		}
		case "FILE_LOCKED": {
			setTimeout(function ()
			{
				alert("현재 다른 세션에서 사용 중인 파일입니다.");
			}, 0);
			break;
		}
	}
}
function result_cut(result)
{
	if (result.success == "SUCCESS")
	{
		var name = $("#selected-item-name").val();
		var type = $("#selected-item-type").val();
		if (type == "FILE") $(`#file-${md5(name)}`).css("opacity", "0.5");
		else $(`#folder-${md5(name)}`).css("opacity", "0.5");
		EnablePaste();
	}
}
function result_copy(result)
{
	if (result.success == "SUCCESS") EnablePaste();
}
function result_paste(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			if (result.type == "CUT") DisablePaste();
			Refresh();
			break;
		}
		case "NO_SUCH_DIRECTORY": {
			setTimeout(function ()
			{
				alert("삭제되었거나 존재하지 않는 경로입니다.");
				Refresh();
			}, 0);
			break;
		}
		case "ALREADY_EXISTS": {
			setTimeout(function ()
			{
				alert("이미 존재하는 이름입니다.");
				var name = $("#selected-item-name").val();
				var type = $("#selected-item-type").val();
				if (type == "DIR") $(`#folder-${md5(name)}`).css("opacity", "1");
				else $(`#file-${md5(name)}`).css("opacity", "1");
				DisablePaste();
			}, 0);
			break;
		}
		case "IS_SUBDIRECTORY": {
			setTimeout(function ()
			{
				alert("하위 디렉터리에는 수행할 수 없습니다.");
				var name = $("#selected-item-name").val();
				var type = $("#selected-item-type").val();
				if (type == "DIR") $(`#folder-${md5(name)}`).css("opacity", "1");
				else $(`#file-${md5(name)}`).css("opacity", "1");
				DisablePaste();
			}, 0);
			break;
		}
		case "SAME_DIRECTORY": {
			setTimeout(function ()
			{
				alert("원본 경로와 대상 경로가 같습니다.");
				var name = $("#selected-item-name").val();
				var type = $("#selected-item-type").val();
				if (type == "DIR") $(`#folder-${md5(name)}`).css("opacity", "1");
				else $(`#file-${md5(name)}`).css("opacity", "1");
				DisablePaste();
			}, 0);
			break;
		}
		case "FILE_LOCKED": {
			setTimeout(function ()
			{
				alert("현재 다른 세션에서 사용 중인 파일입니다.");
				var name = $("#selected-item-name").val();
				var type = $("#selected-item-type").val();
				if (type == "DIR") $(`#folder-${md5(name)}`).css("opacity", "1");
				else $(`#file-${md5(name)}`).css("opacity", "1");
				DisablePaste();
			}, 0);
			break;
		}
	}
}
function result_move(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			Refresh();
			break;
		}
		case "NO_SUCH_DIRECTORY": {
			setTimeout(function ()
			{
				alert("삭제되었거나 존재하지 않는 경로입니다.");
				Refresh();
			}, 0);
			break;
		}
		case "ALREADY_EXISTS": {
			setTimeout(function ()
			{
				alert("이미 존재하는 이름입니다.");
				Refresh();
			}, 0);
			break;
		}
		case "IS_SUBDIRECTORY": {
			setTimeout(function ()
			{
				alert("하위 디렉터리에는 수행할 수 없습니다.");
				Refresh();
			}, 0);
			break;
		}
		case "SAME_DIRECTORY": {
			setTimeout(function ()
			{
				alert("원본 경로와 대상 경로가 같습니다.");
				Refresh();
			}, 0);
			break;
		}
		case "FILE_LOCKED": {
			setTimeout(function ()
			{
				alert("현재 다른 세션에서 사용 중인 파일입니다.");
				Refresh();
			}, 0);
			break;
		}
	}
}

function lock_completed(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			setTimeout(function ()
			{
				alert("파일이 성공적으로 잠겼습니다.");
				Refresh();
			}, 0);
			break;
		}
		case "INVALID_ENCRYPTION": {
			setTimeout(function ()
			{
				alert("파일을 잠그는데 실패하였습니다.\n관리자에게 문의해주세요.");
			}, 0);
			break;
		}
	}
}
function unlock_completed(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			setTimeout(function ()
			{
				alert("파일의 잠금을 성공적으로 해제하였습니다.");
				Refresh();
			}, 0);
			break;
		}
		case "INVALID_ENCRYPTION": {
			setTimeout(function ()
			{
				alert("잘못된 비밀번호입니다.");
			}, 0);
			break;
		}
	}
}

function LoadNext()
{
	var start = CURRENT_PAGE++ * PAGE_PER_SCROLL;
	var end = CURRENT_PAGE * PAGE_PER_SCROLL;
	var list = EXPLORER_LIST.slice(start, end);
	if (list.length == 0) SCROLL_UPDATE = false;
	EXPLORER_COUNT += list.length;
	UpdateExplorer(list);
	var x = $("#Explorer").prop("scrollHeight");
	var y = $("#Explorer").innerHeight();
	if (x - 50 <= y && list.length != 0) LoadNext();
}

function share_result(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			setTimeout(function ()
			{
				alert(`폴더 공유가 시작되었습니다.`);
				Refresh();
			}, DELAY);
			break;
		}
		case "ALREADY_EXISTS": {
			setTimeout(function ()
			{
				alert(`이미 공유 중인 폴더입니다.`);
			}, DELAY);
			break;
		}
		case "SHARED_EXISTS": {
			setTimeout(function ()
			{
				alert(`하위 경로에서 공유 중인 폴더가 있습니다.`);
			}, DELAY);
			break;
		}
	}
}
function code_result(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			ShowCurtain();
			ShowCodeViewer(result.qr, result.url);
			break;
		}
		case "NO_SUCH_DIRECTORY": {
			setTimeout(function ()
			{
				alert(`존재하지 않거나 공유 중인 폴더가 아닙니다.`);
			}, DELAY);
			break;
		}
	}
}
function unshare_result(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			setTimeout(function ()
			{
				alert(`폴더 공유가 해제되었습니다.`);
				Refresh();
			}, DELAY);
			break;
		}
		case "NO_SUCH_DIRECTORY": {
			setTimeout(function ()
			{
				alert(`존재하지 않거나 공유 중인 폴더가 아닙니다.`);
			}, DELAY);
			break;
		}
	}
}
function unlink_result(result)
{
	switch (result.success)
	{
		case "SUCCESS": {
			alert("링크가 해제되었습니다.");
			Refresh();
			break;
		}
	}
}

function update_btn_status()
{
	if (PREV_STACK.length > 0) $("#prev").prop("disabled", false);
	else $("#prev").prop("disabled", true);
	if (NEXT_STACK.length > 0) $("#next").prop("disabled", false);
	else $("#next").prop("disabled", true);
}

$(document).ready(function ()
{
	InitPathFinder();
	InitExplorer();
	InitCurtain();
	InitGuide();
	InitUploadButton();
	InitShareModeSelector();
	InitFileViewer();

	Refresh();

	// 검색창
	$("#PathFinder .search-text").keyup(function ()
	{
		var text = $("#PathFinder .search-text").val();
		if (text.length > 0)
		{
			$("#PathFinder .search-btn .search-logo").removeClass('fa-search');
			$("#PathFinder .search-btn .search-logo").addClass('fa-times');
		}
		else
		{
			$("#PathFinder .search-btn .search-logo").removeClass('fa-times');
			$("#PathFinder .search-btn .search-logo").addClass('fa-search');
		}
		AjaxSilent("Search", { path: CURRENT_PATH, keyword: text }, result_list);
	});
	$("#PathFinder .search-btn").click(function ()
	{
		if ($(".search-text").val().length > 0)
		{
			$("#PathFinder .search-text").val("");
			$("#PathFinder .search-text").trigger("keyup");
		}
	});

	// 플랫폼 별 기능 제공
	if (IS_MOBILE)
	{
		$(".indicator").css("overflow-x", "scroll");
		$("#Explorer").css("overflow-y", "scroll");

		$("#GuideText1").html("파일을 업로드하려면 화면을 길게 누르거나");
		$("#GuideText2").html("오른쪽 아래의 업로드 버튼을 누르세요.");
		$('#upload-btn').each(function ()
		{
			$this = $(this);
			if (IS_MOBILE) propagating(new Hammer(this)).on("tap", function (e)
			{
				ShowCurtain();
				ShowUploadForm();
				HideAllContextMenu();
				e.preventDefault();
				e.stopPropagation();
			});
		});
	}
	else
	{
		$(".indicator").on("mousewheel DOMMouseScroll", function (event)
		{
			event.preventDefault();
			var e = event.originalEvent;
			var delta = (e.detail < 0 || e.wheelDelta > 0) ? 1 : -1;
			this.scrollLeft -= (delta * SCROLL_RATIO);
		});
		$("#Explorer").on("mousewheel DOMMouseScroll", function (event)
		{
			event.preventDefault();
			var e = event.originalEvent;
			var delta = (e.detail < 0 || e.wheelDelta > 0) ? 1 : -1;
			this.scrollTop -= (delta * SCROLL_RATIO);
		});

		$("#GuideText1").html("파일을 업로드하려면 오른쪽 마우스를 클릭하거나");
		$("#GuideText2").html("파일을 여기로 드래그하세요.");
		$("#upload-btn").css("display", "none");
	}

	// 인디케이터 버튼 기능
	$("#prev").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			if (PREV_STACK.length > 0)
			{
				HideAllContextMenu();
				var path = PREV_STACK.pop();
				NEXT_STACK.push(CURRENT_PATH);
				Ajax("List", {
					path: path
				}, result_list);
				update_btn_status();
			}
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$("#next").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			if (NEXT_STACK.length > 0)
			{
				HideAllContextMenu();
				var path = NEXT_STACK.pop();
				PREV_STACK.push(CURRENT_PATH);
				Ajax("List", {
					path: path
				}, result_list);
				update_btn_status();
			}
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$("#redo").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			HideAllContextMenu();
			Refresh();
			e.preventDefault();
			e.stopPropagation();
		});
	});

	// 스크롤 페이징
	$("#Explorer").on('mousewheel DOMMouseScroll scroll', function ()
	{
		var scroll = $(this).scrollTop();
		var height = $(this).innerHeight();
		var scrollHeight = $(this)[0].scrollHeight;
		if (scroll + height + 100 >= scrollHeight && SCROLL_UPDATE) LoadNext();
	});

	// 탐색기 컨텍스트 메뉴
	$("#Explorer").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			$(".folder").removeClass("selected");
			$(".file").removeClass("selected");
			HideAllContextMenu();
			e.preventDefault();
			e.stopPropagation();
		});
		if (IS_MOBILE) propagating(new Hammer(this)).on("press", function (e)
		{
			$(".folder").removeClass("selected");
			$(".file").removeClass("selected");
			ShowExplorerContextMenu(e.center.x, e.center.y);
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$(document).on("contextmenu", "#Explorer", function (e)
	{
		$(".folder").removeClass("selected");
		$(".file").removeClass("selected");
		ShowExplorerContextMenu(e.pageX, e.pageY);
		return false;
	});
	$("#context-menu div").hammer().on("tap", function ()
	{
		HideContextMenu();
		return false;
	});

	// 파일 컨텍스트 메뉴
	$(document).on("contextmenu", ".folder, .file", function (e)
	{
		$(".folder").removeClass("selected");
		$(".file").removeClass("selected");
		$(this).addClass("selected");
		ShowFileContextMenu($(this), e.pageX, e.pageY);
		return false;
	});
	$("#file-context-menu div").hammer().on("tap", function ()
	{
		HideFileContextMenu();
		return false;
	});
	$(".folder, .file").hammer().on("tap", function ()
	{
		HideAllContextMenu();
		return false;
	});

	// 컨텍스트 메뉴 기능
	$("#create-folder").hammer().on("tap", function ()
	{
		var name = prompt("생성할 폴더의 이름을 입력하세요");
		if (name != null && name.length != 0)
		{
			if (ValidateDirectoryName(name))
			{
				if (name.split(".").pop() == "locked") alert("폴더 이름의 끝에 .locked 를 사용할 수 없습니다.");
				else
				{
					Ajax("Create", {
						path: `${CURRENT_PATH}\\${name}`,
					}, result_create);
				}
			}
			else alert("폴더 이름에는 다음 문자를 사용할 수 없습니다.\n\\/:*?\"<>|#&+");
		}
	});
	$("#upload-file").hammer().on("tap", function ()
	{
		ShowCurtain();
		ShowUploadForm();
	});
	$("#paste").hammer().on("tap", function ()
	{
		var path = $("#selected-path").val();
		Ajax("Paste", {
			path: path
		}, result_paste);
	});
	$("#refresh").hammer().on("tap", function ()
	{
		Refresh();
	});

	// 파일 컨텍스트 메뉴 기능
	$("#share").hammer().on("tap", function ()
	{
		if (confirm(`경고 : 폴더를 공유하면 다른 사용자가 공유 코드를 통해\n사용자의 폴더를 열람할 수 있게됩니다.\n정말로 진행하시겠습니까?`))
		{
			ShowCurtain();
			ShowShareModeSelector();
		}
	});
	$("#code").hammer().on("tap", function ()
	{
		var path = $("#selected-item-path").val();
		Ajax("GetCode", {
			path: path
		}, function (result) { code_result(result); });
	});
	$("#manage").hammer().on("tap", function ()
	{
        var path = $("#selected-item-path").val();
        Ajax("GetPermission", {
			path: path
		}, function (result)
		{
			switch (result.success)
			{
				case "SUCCESS": {
					ShowCurtain();
					ShowShareManager(result.list);
					break;
				}
				case "NO_SUCH_DIRECTORY": {
					alert("존재하지 않거나 삭제된 경로입니다.");
					break;
				}
			}
		});
	});
	$("#unshare").hammer().on("tap", function ()
	{
		if (confirm(`경고 : 공유를 해제하면 해당 폴더에\n접근 중인 모든 사용자가 접근할 수 없게 됩니다.\n정말로 진행하시겠습니까?`))
		{
			var path = $("#selected-item-path").val();
			Ajax("Unshare", {
				path: path
			}, function (result) { unshare_result(result); });
		}
	});
	$("#unlink").hammer().on("tap", function ()
	{
		if (confirm(`링크를 해제하면 더 이상 해당 폴더에 접근할 수 없게 됩니다.\n정말로 진행하시겠습니까?`))
		{
			var path = $("#selected-item-path").val();
			Ajax("Unlink", {
				path
			}, unlink_result);
		}
	});
	$("#preview").hammer().on("tap", function ()
	{
		var path = $("#selected-item-path").val();
		var extension = path.split(".").pop().toLowerCase();
		switch (extension)
		{
			case "jpg": case "bmp": case "png": case "jpeg": {
				ShowCurtain();
				ShowFileViewer(path, "IMG");
				break;
			}
			case "mov": case "avi": case "mp4": case "mpg": case "mkv": {
				ShowCurtain();
				ShowFileViewer(path, "VID");
				break;
			}
			case "mp3": case "flac": case "wma": case "wav": case "aac": case "m4a": case "ogg": {
				ShowCurtain();
				ShowFileViewer(path, "AUD");
				break;
			}
			case "locked": {
				setTimeout(function ()
				{
					alert("파일이 잠겨있습니다.");
				}, 0);
				break;
			}
			default: {
				setTimeout(function ()
				{
					alert("지원하지 않는 형식입니다.");
				}, 0);
				break;
			}
		}
	});
	$("#download").hammer().on("tap", function ()
	{
		var path = $("#selected-item-path").val();
		var extension = path.split(".").pop().toLowerCase();
		if (extension == "locked")
		{
			setTimeout(function ()
			{
				alert("파일이 잠겨있습니다.");
			}, 0);
		}
		else window.location.href = `Download?path=${path}`;
		return false;
	});
	$("#lock").hammer().on("tap", function ()
	{
		var path = $("#selected-item-path").val();
		if (confirm("경고 : 파일을 잠그는데 사용한 비밀번호를 분실할 경우\n파일을 복구하는 것이 불가능합니다.\n계속 진행할까요?"))
		{
			var password = prompt("파일을 잠그는데 사용할 비밀번호를 입력하세요");
			Ajax("Lock", {
				path: path,
				password: password
			}, lock_completed);
		}
	});
	$("#unlock").hammer().on("tap", function ()
	{
		var path = $("#selected-item-path").val();
		var password = prompt("파일을 여는데 사용할 비밀번호를 입력하세요");
		if (password.length == 0) setTimeout(function ()
		{
			alert("비밀번호의 길이는 0이 될 수 없습니다.");
		}, 0);
		else Ajax("Unlock", {
			path: path,
			password: password
		}, unlock_completed);
	});
	$("#copy").hammer().on("tap", function ()
	{
		var path = $("#selected-item-path").val();
		var type = $("#selected-item-type").val();

		Ajax("Copy", {
			path: path,
			type: type
		}, result_copy);
	});
	$("#cut").hammer().on("tap", function ()
	{
		var path = $("#selected-item-path").val();
		var type = $("#selected-item-type").val();

		Ajax("Cut", {
			path: path,
			type: type
		}, result_cut);
	});
	$("#delete").hammer().on("tap", function ()
	{
		if (confirm("삭제된 파일은 복구할 수 없습니다.\n정말로 삭제하시겠습니까?"))
		{
			var path = $("#selected-item-path").val();
			var type = $("#selected-item-type").val();

			Ajax("Delete", {
				path: path,
				type: type
			}, result_delete);
		}
	});
	$("#rename").hammer().on("tap", function ()
	{
		var srcPath = $("#selected-item-path").val();
		var extension = srcPath.split(".").pop();
		var type = $("#selected-item-type").val();
		if (extension == "locked")
		{
			setTimeout(function ()
			{
				alert("파일이 잠겨있습니다.");
			}, 0);
		}
		else
		{
			var dst = prompt("변경할 이름을 입력해주세요.", srcPath.split("\\").pop());
			if (type == "FILE" && dst.split(".").pop() != extension)
			{
				if (confirm("확장자를 변경하면 파일을 사용할 수 없게 될 수도 있습니다.\n변경하시겠습니까?"))
				{
					if (dst.split(".").pop() == "locked")
					{
						setTimeout(function ()
						{
							alert("locked 확장자는 사용할 수 없습니다.");
						}, 0);
					}
					else if (dst != null && dst.length != 0)
					{
						if (ValidateDirectoryName(dst))
						{
							if (dst.split(".").pop() == "locked") alert("폴더 이름의 끝에 .locked 를 사용할 수 없습니다.");
							else
							{
								var temp = srcPath.split("\\");
								temp.pop();
								temp.push(dst);
								var dstPath = temp.join("\\");
								Ajax("Rename", {
									srcPath: srcPath,
									dstPath: dstPath,
									type: type
								}, result_rename);

							}
						}
						else alert("폴더 이름에는 다음 문자를 사용할 수 없습니다.\n\\/:*?\"<>|");
					}
				}
			}
			else
			{
				if (dst != null && dst.length != 0)
				{
					if (ValidateDirectoryName(dst))
					{
						if (dst.split(".").pop() == "locked") alert("폴더 이름의 끝에 .locked 를 사용할 수 없습니다.");
						else
						{
							var temp = srcPath.split("\\");
							temp.pop();
							temp.push(dst);
							var dstPath = temp.join("\\");
							Ajax("Rename", {
								srcPath: srcPath,
								dstPath: dstPath,
								type: type
							}, result_rename);
						}
					}
					else alert("폴더 이름에는 다음 문자를 사용할 수 없습니다.\n\\/:*?\"<>|#&+");
				}
			}
		}
	});

	// 코드 보기 폼 UI
	$("#CodeViewer .exit-btn").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			HideCodeViewer();
			HideCurtain();
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$("#CodeViewer #InnerViewer #UrlArea #CopyUrl").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			$("#CodeViewer #InnerViewer #UrlArea #Url").select();
			document.getElementById("Url").setSelectionRange(0, 9999);
			document.execCommand("copy");
			alert("클립보드에 복사되었습니다.");
			HideCodeViewer();
			HideCurtain();
			e.preventDefault();
			e.stopPropagation();
		});
	});

	// 공유 모드 선택 UI
	$("#ShareModeSelector .exit-btn").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			HideShareModeSelector();
			HideCurtain();
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$("#ShareModeSelector #SelectMode #PublicMode").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			var path = $("#selected-item-path").val();
			Ajax("Share", {
				path: path,
				isPublic: true
			}, share_result);
			HideShareModeSelector();
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$("#ShareModeSelector #SelectMode #PrivateMode").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			var path = $("#selected-item-path").val();
			Ajax("Share", {
				path: path,
				isPublic: false
			}, share_result);
			HideShareModeSelector();
			e.preventDefault();
			e.stopPropagation();
		});
	});

	// 공유 관리자 UI
	$("#ShareManager .exit-btn").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			HideShareManager();
			HideCurtain();
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$("#ShareManager .search-btn").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			var username = $("#ShareManager .search-text").val();
			if (username.length > 0)
			{
				AjaxSilent("FindUser", {
					username: username
				}, function (result)
				{
					if (result.success) AddPermissionList(username, false, false, false, false);
					else alert("존재하지 않는 사용자 입니다.");
					$("#ShareManager .search-text").val("");
				});
			}
			e.preventDefault();
			e.stopPropagation();
		});
	});
	$("#ShareManager #Save").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			var path = $("#selected-item-path").val();
			var list = [];
			$("#ShareManager .list-group .list-group-item").each(function (index, item)
			{
				var username = $(item).find(".user-name").html();
				var create = $(item).find(`#CreatePermission-${username}`).is(":checked");
				var read = $(item).find(`#ReadPermission-${username}`).is(":checked");
				var write = $(item).find(`#WritePermission-${username}`).is(":checked");
				var remove = $(item).find(`#RemovePermission-${username}`).is(":checked");

				list.push({
					username: username,
					create: create,
					read: read,
					write: write,
					remove: remove
				});
			});
			Ajax("SetPermission", {
				path: path,
				json: JSON.stringify(list)
			}, function (result)
			{
				switch (result.success)
				{
					case "SUCCESS": {
						alert("설정이 저장되었습니다.");
						HideShareManager();
						HideCurtain();
						break;
					}
					case "NO_SUCH_DIRECTORY": {
						alert("존재하지 않거나 삭제된 경로입니다.");
						HideShareManager();
						HideCurtain();
						break;
					}
				}
			});
			e.preventDefault();
			e.stopPropagation();
		});
	});

	//파일 뷰어 닫기 버튼
	$("#FileViewer .exit-btn").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			HideFileViewer();
			HideCurtain();
			e.preventDefault();
			e.stopPropagation();
		});
	});

	// 업로드 취소 버튼 이벤트 등록
	$(document).on("click", ".cancel-btn", function (e)
	{
		var name = $(this).parent().siblings(".progress-name").html().normalize("NFC");
		if (UPLOAD_LIST[UPLOAD_DICT[`${name}`]].status == "READY")
		{
			UPLOAD_LIST[UPLOAD_DICT[`${name}`]].status = "STOP";
			upload_onCanceled(UPLOAD_LIST[UPLOAD_DICT[`${name}`]].file.name);
		}
		else if (UPLOAD_LIST[UPLOAD_DICT[`${name}`]].status == "GO")
			UPLOAD_LIST[UPLOAD_DICT[`${name}`]].status = "STOP";
		e.preventDefault();
	});
	$("#Progress .exit-btn").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			if (confirm("현재 진행 중인 업로드를 모두 취소할까요?"))
				Object.values(UPLOAD_DICT).forEach(function (item)
				{
					if (UPLOAD_LIST[item].status == "READY")
					{
						UPLOAD_LIST[item].status = "STOP";
						upload_onCanceled(UPLOAD_LIST[item].file.name);
					}
					else if (UPLOAD_LIST[item].status == "GO")
						UPLOAD_LIST[item].status = "STOP";
				});
			e.preventDefault();
			e.stopPropagation();
		});
	});

	// 커텐 UI 이벤트 차단
	$("#Curtain").each(function ()
	{
		$this = $(this);
		propagating(new Hammer(this)).on("tap", function (e)
		{
			e.preventDefault();
			e.stopPropagation();
		});
	});
	if (!IS_MOBILE) $("#Curtain").on({
		"dragover dragenter": function (e)
		{
			e.preventDefault();
			e.stopPropagation();
		},
		"dragexit dragend dragleave": function (e)
		{
			e.preventDefault();
			e.stopPropagation();
		},
		"drop": function (e)
		{
			e.preventDefault();
			e.stopPropagation();
		}
	});

	// 업로드 폼 UI
	$(".custom-file-input").on("change", function ()
	{
		var fileName = $(this).val().split("\\").pop();
		if (fileName == "")
		{
			$("#upload").prop("disabled", true);
			fileName = "선택된 파일 없음";
		}
		else $("#upload").prop("disabled", false);
		$(this).siblings(".custom-file-label").html(fileName);
	});
	$("#cancel").hammer().on("tap", function ()
	{
		HideUploadForm();
		HideCurtain();
	});

	// 업로드 폼 파일 업로드
	$("#upload").on("click", function ()
	{
		HideUploadForm();
		var files = $("#File")[0].files;
		uploadFiles("Upload", files, upload_onSecond, upload_onCompleted, upload_onProgress, upload_onError, upload_onCanceled);
	});

	// 드래그 & 드롭 파일 업로드
	if (!IS_MOBILE) $("#Explorer").on({
		"dragover dragenter": function (e)
		{
			var d = e.originalEvent.dataTransfer;
			if (d.types && (d.types.indexOf ? d.types.indexOf('Files') != -1 : d.types.contains('Files')))
				$(this).addClass("activated");
			e.preventDefault();
			e.stopPropagation();
		},
		"dragexit dragend dragleave": function (e)
		{
			var d = e.originalEvent.dataTransfer;
			if (d.types && (d.types.indexOf ? d.types.indexOf('Files') != -1 : d.types.contains('Files')))
				$(this).removeClass("activated");
			e.preventDefault();
			e.stopPropagation();
		},
		"drop": function (e)
		{
			var d = e.originalEvent.dataTransfer;
			if (d && d.types && (d.types.indexOf ? d.types.indexOf('Files') != -1 : d.types.contains('Files')))
			{
				$(this).removeClass("activated");
				ShowCurtain();
				ShowLoading();
				var files = e.originalEvent.dataTransfer.files;
				setTimeout(function ()
				{
					uploadFiles("Upload", files, upload_onSecond, upload_onCompleted, upload_onProgress, upload_onError, upload_onCanceled);
				}, DELAY * 5);
			}
			e.preventDefault();
			e.stopPropagation();
		}
	});
});