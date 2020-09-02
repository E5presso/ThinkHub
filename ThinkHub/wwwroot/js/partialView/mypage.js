var DURATION = 300; // 애니메이션 동작길이

function ValidateEmail(email)
{
	var regex = /^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$/i;
	return regex.test(email);
}
function ValidatePassword(passwd)
{
	var regex = /^(?=.*[a-zA-Z])(?=.*[0-9]).{8,32}$/;
	return regex.test(passwd);
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
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var padding = parseInt($("#PageContainer").css("paddingTop"));
	$("#PageContainer").css("top", gnb);
	$("#PageContainer").height($(window).height() - gnb - (padding * 2));
	$(window).resize(function ()
	{
		var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
		var padding = parseInt($("#PageContainer").css("paddingTop"));
		$("#PageContainer").css("top", gnb);
		$("#PageContainer").height($(window).height() - gnb - (padding * 2));
	});
	InitCurtain();

	$("#EditProfile").click(function ()
	{
		window.location.href = "/User/Profile";
	});

	$("#Change_Email").keyup(function ()
	{
		var value = $("#Change_Email").val();
		if (ValidateEmail(value))
			$("#Change_VerifyEmail").prop("disabled", false);
		else $("#Change_VerifyEmail").prop("disabled", true);
	});
	$("#Change_VerifyEmail").click(function ()
	{
		var value = $("#Change_Email").val();
		$.ajax({
			url: "/User/TryVerifyEmail",
			type: "POST",
			data:
			{
				"UserName": $("#UserName").html(),
				"Email": value
			},
			success: function (response)
			{
				if (response.result == "SENDED")
				{
					code = response.code;
					setTimeout(function ()
					{
						alert("이메일 인증 요청을 발송했습니다.\n이메일 인증 후 확인 버튼을 눌러주세요.");
						$("#Change_Email").prop("disabled", true);
						$("#Change_VerifyEmail").addClass("d-none");
						$("#Change_CheckVerification").removeClass("d-none");
						$("#Change_Email").removeClass("is-valid");
						$("#Change_Email").removeClass("is-invalid");
					}, 0);
				}
				else if (response.result == "DOUBLE_REQUEST" || response.result == "EMAIL_CONFLICT")
				{
					setTimeout(function ()
					{
						alert("이미 사용 중인 이메일입니다.");
						$("#Change_Email").removeClass("is-valid");
						$("#Change_Email").addClass("is-invalid");
						$("#Change_Email").prop("disabled", false);
					}, 0);
				}
			}
		});
	});
	$("#Change_CheckVerification").click(function ()
	{
		$.ajax({
			url: "/User/CheckVerification",
			type: "GET",
			data: { "Code": code },
			success: function (result)
			{
				if (result.verified)
				{
					setTimeout(function ()
					{
						alert("이메일 인증이 완료되었습니다.");
						$("#Change_Email").removeClass("is-invalid");
						$("#Change_Email").addClass("is-valid");
						$("#Change_CheckVerification").addClass("d-none");
						$("#Change_Verified").removeClass("d-none");
						$("#Change_Verified").prop("disabled", true);
						$("#Change_EmailButton").prop("disabled", false);
					}, 0);
				}
				else
				{
					setTimeout(function ()
					{
						alert("이메일이 인증되지 않았습니다.");
						$("#Change_Email").prop("disabled", false);
						$("#Change_Email").removeClass("is-ialid");
						$("#Change_Email").removeClass("is-invalid");
						$("#Change_CheckVerification").addClass("d-none");
						$("#Change_VerifyEmail").removeClass("d-none");
					}, 0);
				}
				if ($("#Change_Email").hasClass("is-valid"))
				{
					$("#Change_EmailButton").prop("disabled", false);
				}
				else
				{
					$("#Change_EmailButton").prop("disabled", true);
				}
			}
		});
	});
	$("#Change_EmailButton").click(function ()
	{
		var email = $("#Change_Email").val();
		$(this).prop("disabled", true);
		ShowCurtain();
		ShowLoading();
		$.ajax({
			url: "/User/ChangeEmail",
			type: "POST",
			data: {
				email: email,
			},
			success: function (data)
			{
				HideLoading();
				HideCurtain();
				$(this).prop("disabled", false);
				switch (data.result)
				{
					case "SUCCESS": {
						setTimeout(function ()
						{
							alert("이메일 주소가 변경되었습니다.");
							$("#Change_Email").prop("disabled", false);
							$("#Change_VerifyEmail").removeClass("d-none");
							$("#Change_CheckVerification").addClass("d-none");
							$("#Change_Verified").addClass("d-none");
							$("#Change_Email").removeClass("is-valid");
							$("#Change_Email").removeClass("is-invalid");
							$("#Change_CheckVerification").prop("disabled", false);
						}, 0);
						break;
					}
					case "NOT_LOGGEDIN": {
						setTimeout(function ()
						{
							alert("세션이 만료되었습니다.");
							window.location.href = "/Home/Index";
						}, 0);
					}
				}
			}
		});
	});

	$("#Change_Password").keyup(function ()
	{
		var origin = $("#Change_Password").val();
		if (origin.length > 0)
		{
			if (ValidatePassword(origin))
			{
				$("#Change_Password").siblings("label").html("안전한 비밀번호입니다.");
				$("#Change_Password").siblings("label").removeClass("text-danger");
				$("#Change_Password").siblings("label").addClass("text-success");
				$("#Change_Password").removeClass("is-invalid");
				$("#Change_Password").addClass("is-valid");
			}
			else
			{
				$("#Change_Password").siblings("label").html("영문자, 숫자 포함 8 ~ 32자리 입니다.");
				$("#Change_Password").siblings("label").removeClass("text-success");
				$("#Change_Password").siblings("label").addClass("text-danger");
				$("#Change_Password").removeClass("is-valid");
				$("#Change_Password").addClass("is-invalid");
			}
		}
		else
		{
			$("#Change_Password").siblings("label").html("비밀번호");
			$("#Change_Password").siblings("label").removeClass("text-danger");
			$("#Change_Password").siblings("label").removeClass("text-success");
			$("#Change_Password").removeClass("is-invalid");
			$("#Change_Password").removeClass("is-valid");
		}
		$("#Change_ConfirmPassword").trigger("keyup");
		if ($("#Change_Password").hasClass("is-valid") && $("#Change_ConfirmPassword").hasClass("is-valid"))
		{
			$("#Change_PasswordButton").prop("disabled", false);
		}
		else
		{
			$("#Change_PasswordButton").prop("disabled", true);
		}
	});
	$("#Change_ConfirmPassword").keyup(function ()
	{
		var origin = $("#Change_Password").val();
		var confirm = $("#Change_ConfirmPassword").val();
		if (origin.length > 0)
		{
			if ($("#Change_Password").hasClass("is-valid"))
			{
				if (origin == confirm)
				{
					$("#Change_ConfirmPassword").siblings("label").html("비밀번호가 일치합니다.");
					$("#Change_ConfirmPassword").siblings("label").removeClass("text-danger");
					$("#Change_ConfirmPassword").siblings("label").addClass("text-success");
					$("#Change_ConfirmPassword").removeClass("is-invalid");
					$("#Change_ConfirmPassword").addClass("is-valid");
				}
				else
				{
					$("#Change_ConfirmPassword").siblings("label").html("비밀번호가 일치하지 않습니다.");
					$("#Change_ConfirmPassword").siblings("label").removeClass("text-success");
					$("#Change_ConfirmPassword").siblings("label").addClass("text-danger");
					$("#Change_ConfirmPassword").removeClass("is-valid");
					$("#Change_ConfirmPassword").addClass("is-invalid");
				}
			}
			else
			{
				$("#Change_ConfirmPassword").siblings("label").html("잘못된 비밀번호입니다.");
				$("#Change_ConfirmPassword").siblings("label").removeClass("text-success");
				$("#Change_ConfirmPassword").siblings("label").addClass("text-danger");
				$("#Change_ConfirmPassword").removeClass("is-valid");
				$("#Change_ConfirmPassword").addClass("is-invalid");
			}
		}
		else
		{
			$("#Change_ConfirmPassword").siblings("label").html("비밀번호 확인");
			$("#Change_ConfirmPassword").siblings("label").removeClass("text-danger");
			$("#Change_ConfirmPassword").siblings("label").removeClass("text-success");
			$("#Change_ConfirmPassword").removeClass("is-invalid");
			$("#Change_ConfirmPassword").removeClass("is-valid");
		}
		if ($("#Change_Password").hasClass("is-valid") && $("#Change_ConfirmPassword").hasClass("is-valid"))
		{
			$("#Change_PasswordButton").prop("disabled", false);
		}
		else
		{
			$("#Change_PasswordButton").prop("disabled", true);
		}
	});
	$("#Change_PasswordButton").click(function ()
	{
		var oldpassword = $("#Change_Old_Password").val();
		var newpassword = $("#Change_Password").val();
		$(this).prop("disabled", true);
		ShowCurtain();
		ShowLoading();
		$.ajax({
			url: "/User/ChangePassword",
			type: "POST",
			data: {
				oldpassword: oldpassword,
				newpassword: newpassword
			},
			success: function (data)
			{
				HideLoading();
				HideCurtain();
				$(this).prop("disabled", false);
				switch (data.result)
				{
					case "SUCCESS": {
						setTimeout(function ()
						{
							alert("비밀번호가 변경되었습니다.");
							$("#Change_Old_Password").val("");
							$("#Change_Password").val("");
							$("#Change_ConfirmPassword").val("");
							$("#Change_Password").siblings("label").html("비밀번호");
							$("#Change_Password").siblings("label").removeClass("text-danger");
							$("#Change_Password").siblings("label").removeClass("text-success");
							$("#Change_Password").removeClass("is-invalid");
							$("#Change_Password").removeClass("is-valid");
							$("#Change_ConfirmPassword").siblings("label").html("비밀번호 확인");
							$("#Change_ConfirmPassword").siblings("label").removeClass("text-danger");
							$("#Change_ConfirmPassword").siblings("label").removeClass("text-success");
							$("#Change_ConfirmPassword").removeClass("is-invalid");
							$("#Change_ConfirmPassword").removeClass("is-valid");
						}, 0);
						break;
					}
					case "WRONG_PASSWORD": {
						setTimeout(function ()
						{
							alert("사용 중인 비밀번호가 다릅니다.");
							$("#Change_Old_Password").val("");
							$("#Change_Password").val("");
							$("#Change_ConfirmPassword").val("");
							$("#Change_Password").siblings("label").html("비밀번호");
							$("#Change_Password").siblings("label").removeClass("text-danger");
							$("#Change_Password").siblings("label").removeClass("text-success");
							$("#Change_Password").removeClass("is-invalid");
							$("#Change_Password").removeClass("is-valid");
							$("#Change_ConfirmPassword").siblings("label").html("비밀번호 확인");
							$("#Change_ConfirmPassword").siblings("label").removeClass("text-danger");
							$("#Change_ConfirmPassword").siblings("label").removeClass("text-success");
							$("#Change_ConfirmPassword").removeClass("is-invalid");
							$("#Change_ConfirmPassword").removeClass("is-valid");
						}, 0);
						break;
					}
					case "NOT_LOGGEDIN": {
						setTimeout(function ()
						{
							alert("세션이 만료되었습니다.");
							window.location.href = "/Home/Index";
						}, 0);
						break;
					}
				}
			}
		});
	});

	$("#Delete_UserName").keyup(function ()
	{
		var id = $("#Delete_UserName").val();
		var pw = $("#Delete_Password").val();
		if (id.length > 0 && pw.length > 0)
			$("#DeleteButton").prop("disabled", false);
		else $("#DeleteButton").prop("disabled", true);
	});
	$("#Delete_Password").keyup(function ()
	{
		var id = $("#Delete_UserName").val();
		var pw = $("#Delete_Password").val();
		if (id.length > 0 && pw.length > 0)
			$("#DeleteButton").prop("disabled", false);
		else $("#DeleteButton").prop("disabled", true);
	});
	$("#DeleteButton").click(function ()
	{
		var username = $("#Delete_UserName").val();
		var password = $("#Delete_Password").val();
		$(this).prop("disabled", true);
		ShowCurtain();
		ShowLoading();
		$.ajax({
			url: "/User/DeleteAccount",
			type: "POST",
			data: {
				username: username,
				password: password
			},
			success: function (data)
			{
				HideLoading();
				HideCurtain();
				$(this).prop("disabled", false);
				switch (data.result)
				{
					case "SUCCESS": {
						setTimeout(function ()
						{
							alert("회원 탈퇴가 완료되었습니다.");
							window.location.href = "/Home/Index";
						}, 0);
						break;
					}
					case "WRONG_USERDATA": {
						setTimeout(function ()
						{
							alert("사용자 이름 또는 비밀번호가 일치하지 않습니다.");
							$("#Delete_UserName").val("");
							$("#Delete_Password").val("");
						}, 0);
						break;
					}
					case "NOT_LOGGEDIN": {
						setTimeout(function ()
						{
							alert("세션이 만료되었습니다.");
							window.location.href = "/Home/Index";
						}, 0);
						break;
					}
				}
			}
		});
	});
});