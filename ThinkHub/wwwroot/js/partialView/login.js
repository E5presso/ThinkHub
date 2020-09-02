var DURATION = 300; // 애니메이션 동작길이

function InitCurtain()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var padding = parseInt($("#Curtain").css("paddingTop"));

	$("#Curtain").css("top", gnb);
	$("#Curtain").height($(window).height() - gnb - (padding * 2));
	$(window).resize(function ()
	{
		$("#Curtain").height($(window).height() - gnb - (padding * 2));
	});
}
function ValidateForm()
{
	var id = $("#UserName").val();
	var pw = $("#Password").val();
	if (id.length > 0 && pw.length > 0)
		$("#Login").prop("disabled", false);
	else $("#Login").prop("disabled", true);
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

$(document).ready(function ()
{
	var gnb = $(".gnb").outerHeight();
	var bound = $(window).innerHeight();
	var height = $("#LoginForm").outerHeight();
	$("#LoginForm").css("top", gnb + ((bound - gnb - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $(window).innerHeight();
		var height = $("#LoginForm").outerHeight();
		$("#LoginForm").css("top", gnb + ((bound - gnb - height) / 2));
	});
	InitCurtain();

	$("#UserName").keyup(function ()
	{
		ValidateForm();
	});
	$("#Password").keyup(function ()
	{
		ValidateForm();
	});
	$("#Login").click(function ()
	{
		var id = $("#UserName").val();
		var pw = $("#Password").val();
		$(this).prop("disabled", "true");
		ShowCurtain();
		ShowLoading();
		$.ajax({
			url: "/User/CheckAccount",
			type: "POST",
			data: {
				"UserName": id,
				"Password": pw
			},
			success: function (data)
			{
				$(this).prop("disabled", "false");
				HideLoading();
				HideCurtain();
				if (data.result)
				{
					var link = $("#LINK_URL").val();
					if (link.length != 0) window.location.href = `${link}`;
					else window.location.href = "/Home/Index";
				}
				else
				{
					setTimeout(function ()
					{
						alert("사용자 이름 또는 비밀번호가 일치하지 않습니다.");
						$("#UserName").removeClass("is-valid");
						$("#UserName").addClass("is-invalid");
						$("#Password").removeClass("is-valid");
						$("#Password").addClass("is-invalid");
					}, 0);
				}
			}
		});
		event.preventDefault();
	});
});