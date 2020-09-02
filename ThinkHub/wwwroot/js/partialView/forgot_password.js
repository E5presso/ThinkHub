var DURATION = 300; // 애니메이션 동작길이

function ValidateEmail(email)
{
	var regex = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
	return regex.test(email);
}

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
	var height = $("#ForgotForm").outerHeight();
	$("#ForgotForm").css("top", gnb + ((bound - gnb - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $(window).innerHeight();
		var height = $("#ForgotForm").outerHeight();
		$("#ForgotForm").css("top", gnb + ((bound - gnb - height) / 2));
	});
	InitCurtain();

	$("#Email").keyup(function ()
	{
		var value = $("#Email").val();
		if (ValidateEmail(value))
			$("#ResetPassword").prop("disabled", false);
		else $("#ResetPassword").prop("disabled", true);
	});
	$("#ResetPassword").click(function ()
	{
		var email = $("#Email").val();
		$(this).prop("disabled", "true");
		ShowCurtain();
		ShowLoading();
		$.ajax({
			url: "/User/ResetPassword",
			type: "POST",
			data: {
				email: email
			},
			success: function (data)
			{
				$(this).prop("disabled", "false");
				HideLoading();
				HideCurtain();
				switch (data.result)
				{
					case "SUCCESS": {
						setTimeout(function ()
						{
							alert("재설정된 비밀번호가 이메일로 발송되었습니다.\n이메일에 기재된 비밀번호로 로그인해주세요.");
							window.location.href = "/User/Login";
						}, 0);
						break;
					}
					case "NOT_REGISTERED": {
						setTimeout(function ()
						{
							alert("가입되지 않은 이메일입니다.");
						}, 0);
						break;
					}
				}
			}
		});
		event.preventDefault();
	});
});