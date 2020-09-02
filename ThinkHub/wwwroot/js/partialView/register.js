function ValidateUserName(username)
{
	var regex = RegExp(/^[a-zA-Z0-9]{8,20}$/);
	return regex.test(username);
}
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
function ValidateName(name)
{
	var regex = /^[가-힣]{2,4}$/;
	return regex.test(name);
}
function ValidatePhone(phone)
{
	var regex = /(01[016789])([1-9]{1}[0-9]{2,3})([0-9]{4})$/;
	return regex.test(phone);
}
function ValidateForm()
{
	setTimeout(function ()
	{
		if ($("#UserName").hasClass("is-valid") &&
			$("#Email").hasClass("is-valid") &&
			$("#Password").hasClass("is-valid") &&
			$("#ConfirmPassword").hasClass("is-valid") &&
			$("#Name").hasClass("is-valid") &&
			$("#Birthday").hasClass("is-valid") &&
			$("#Phone").hasClass("is-valid") &&
			$("#Agreement").is(":checked"))
		{
			$("#Register").prop("disabled", false);
			console.log("validated");
		}
		else
		{
			$("#Register").prop("disabled", true);
			console.log("failed");
		}
	}, 100);
}

$(document).ready(() =>
{
	var gnb = $(".gnb").outerHeight();
	var bound = $(window).innerHeight();
	var height = $("#RegistrationForm").outerHeight();
	$("#RegistrationForm").css("top", gnb + ((bound - gnb - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $(window).innerHeight();
		var height = $("#RegistrationForm").outerHeight();
		$("#RegistrationForm").css("top", gnb + ((bound - gnb - height) / 2));
	});

	var code = "";
	$("#UserName").keyup(function ()
	{
		var value = $("#UserName").val();
		if (value.length > 0)
		{
			if (ValidateUserName(value))
			{
				$.ajax({
					url: "/User/CheckUserName",
					type: "POST",
					data: { "UserName": value },
					success: function (data)
					{
						if (data.result)
						{
							$("#UserName").removeClass("is-invalid");
							$("#UserName").addClass("is-valid");
							$("#UserName").siblings("label").html("사용 가능한 아이디입니다.");
							$("#UserName").siblings("label").removeClass("text-danger");
							$("#UserName").siblings("label").addClass("text-success");
						}
						else
						{
							$("#UserName").removeClass("is-valid");
							$("#UserName").addClass("is-invalid");
							$("#UserName").siblings("label").html("이미 가입된 사용자입니다.");
							$("#UserName").siblings("label").removeClass("text-success");
							$("#UserName").siblings("label").addClass("text-danger");
						}
					}
				});
			}
			else
			{
				$("#UserName").removeClass("is-valid");
				$("#UserName").addClass("is-invalid");
				$("#UserName").siblings("label").html("영문자, 숫자 포함 8자리 이상 필요합니다.");
				$("#UserName").siblings("label").removeClass("text-success");
				$("#UserName").siblings("label").addClass("text-danger");
			}
		}
		else
		{
			$("#UserName").removeClass("is-invalid");
			$("#UserName").removeClass("is-valid");
			$("#UserName").siblings("label").html("아이디");
			$("#UserName").siblings("label").removeClass("text-danger");
			$("#UserName").siblings("label").removeClass("text-success");
		}
		ValidateForm();
	});
	$("#Email").keyup(function ()
	{
		var value = $("#Email").val();
		if (ValidateEmail(value))
			$("#VerifyEmail").prop("disabled", false);
		else $("#VerifyEmail").prop("disabled", true);
		ValidateForm();
	});
	$("#VerifyEmail").click(function ()
	{
		var value = $("#Email").val();
		$.ajax({
			url: "/User/TryVerifyEmail",
			type: "POST",
			data:
			{
				"UserName": $("#UserName").val(),
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
						$("#Email").prop("disabled", true);
						$("#VerifyEmail").addClass("d-none");
						$("#CheckVerification").removeClass("d-none");
						$("#Email").removeClass("is-valid");
						$("#Email").removeClass("is-invalid");
					}, 0);
				}
				else if (response.result == "DOUBLE_REQUEST" || response.result == "EMAIL_CONFLICT")
				{
					setTimeout(function ()
					{
						alert("이미 사용 중인 이메일입니다.");
						$("#Email").removeClass("is-valid");
						$("#Email").addClass("is-invalid");
						$("#Email").prop("disabled", false);
					}, 0);
				}
			}
		});
	});
	$("#CheckVerification").click(function ()
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
					}, 0);
					$("#Email").removeClass("is-invalid");
					$("#Email").addClass("is-valid");
					$("#CheckVerification").addClass("d-none");
					$("#Verified").removeClass("d-none");
					$("#Verified").prop("disabled", true);
					ValidateForm();
				}
				else
				{
					setTimeout(function ()
					{
						alert("이메일이 인증되지 않았습니다.");
					}, 0);
					$("#Email").prop("disabled", false);
					$("#Email").removeClass("is-ialid");
					$("#Email").removeClass("is-invalid");
					$("#CheckVerification").addClass("d-none");
					$("#VerifyEmail").removeClass("d-none");
				}
			}
		});
	})
	$("#Password").keyup(function ()
	{
		var origin = $("#Password").val();
		if (origin.length > 0)
		{
			if (ValidatePassword(origin))
			{
				$("#Password").siblings("label").html("안전한 비밀번호입니다.");
				$("#Password").siblings("label").removeClass("text-danger");
				$("#Password").siblings("label").addClass("text-success");
				$("#Password").removeClass("is-invalid");
				$("#Password").addClass("is-valid");
			}
			else
			{
				$("#Password").siblings("label").html("영문자, 숫자 포함 8 ~ 32자리 입니다.");
				$("#Password").siblings("label").removeClass("text-success");
				$("#Password").siblings("label").addClass("text-danger");
				$("#Password").removeClass("is-valid");
				$("#Password").addClass("is-invalid");
			}
		}
		else
		{
			$("#Password").siblings("label").html("비밀번호");
			$("#Password").siblings("label").removeClass("text-danger");
			$("#Password").siblings("label").removeClass("text-success");
			$("#Password").removeClass("is-invalid");
			$("#Password").removeClass("is-valid");
		}
		ValidateForm();
		$("#ConfirmPassword").trigger("keyup");
	});
	$("#ConfirmPassword").keyup(function ()
	{
		var origin = $("#Password").val();
		var confirm = $("#ConfirmPassword").val();
		if (origin.length > 0)
		{
			if ($("#Password").hasClass("is-valid"))
			{
				if (origin == confirm)
				{
					$("#ConfirmPassword").siblings("label").html("비밀번호가 일치합니다.");
					$("#ConfirmPassword").siblings("label").removeClass("text-danger");
					$("#ConfirmPassword").siblings("label").addClass("text-success");
					$("#ConfirmPassword").removeClass("is-invalid");
					$("#ConfirmPassword").addClass("is-valid");
				}
				else
				{
					$("#ConfirmPassword").siblings("label").html("비밀번호가 일치하지 않습니다.");
					$("#ConfirmPassword").siblings("label").removeClass("text-success");
					$("#ConfirmPassword").siblings("label").addClass("text-danger");
					$("#ConfirmPassword").removeClass("is-valid");
					$("#ConfirmPassword").addClass("is-invalid");
				}
			}
			else
			{
				$("#ConfirmPassword").siblings("label").html("잘못된 비밀번호입니다.");
				$("#ConfirmPassword").siblings("label").removeClass("text-success");
				$("#ConfirmPassword").siblings("label").addClass("text-danger");
				$("#ConfirmPassword").removeClass("is-valid");
				$("#ConfirmPassword").addClass("is-invalid");
			}
		}
		else
		{
			$("#ConfirmPassword").siblings("label").html("비밀번호 확인");
			$("#ConfirmPassword").siblings("label").removeClass("text-danger");
			$("#ConfirmPassword").siblings("label").removeClass("text-success");
			$("#ConfirmPassword").removeClass("is-invalid");
			$("#ConfirmPassword").removeClass("is-valid");
		}
		ValidateForm();
	});
	$("#Name").keyup(function ()
	{
		var value = $("#Name").val();
		if (value.length > 0)
		{
			if (ValidateName(value))
			{
				$("#Name").siblings("label").html("유효한 이름입니다.");
				$("#Name").siblings("label").removeClass("text-danger");
				$("#Name").siblings("label").addClass("text-success");
				$("#Name").removeClass("is-invalid");
				$("#Name").addClass("is-valid");
			}
			else
			{
				$("#Name").siblings("label").html("유효하지 않은 이름입니다.");
				$("#Name").siblings("label").removeClass("text-success");
				$("#Name").siblings("label").addClass("text-danger");
				$("#Name").removeClass("is-valid");
				$("#Name").addClass("is-invalid");
			}
		}
		else
		{
			$("#Name").removeClass("is-invalid");
			$("#Name").removeClass("is-valid");
			$("#Name").siblings("label").html("이름");
			$("#Name").siblings("label").removeClass("text-danger");
			$("#Name").siblings("label").removeClass("text-success");
		}
		ValidateForm();
	});
	$("#Birthday").change(function ()
	{
		var value = $("#Birthday").val();
		if (value.length > 0)
		{
			$("#Birthday").siblings("label").html("유효한 날짜입니다.");
			$("#Birthday").siblings("label").removeClass("text-danger");
			$("#Birthday").siblings("label").addClass("text-success");
			$("#Birthday").removeClass("is-invalid");
			$("#Birthday").addClass("is-valid");
		}
		else
		{
			$("#Birthday").siblings("label").html("유효하지 않은 날짜입니다.");
			$("#Birthday").siblings("label").removeClass("text-success");
			$("#Birthday").siblings("label").addClass("text-danger");
			$("#Birthday").removeClass("is-valid");
			$("#Birthday").addClass("is-invalid");
		}
		ValidateForm();
	});
	$("#Phone").keyup(function ()
	{
		var value = $("#Phone").val();
		if (value.length > 0)
		{
			if (ValidatePhone(value))
			{
				$("#Phone").siblings("label").html("유효한 번호입니다.");
				$("#Phone").siblings("label").removeClass("text-danger");
				$("#Phone").siblings("label").addClass("text-success");
				$("#Phone").removeClass("is-invalid");
				$("#Phone").addClass("is-valid");
			}
			else
			{
				$("#Phone").siblings("label").html("유효하지 않은 번호입니다.");
				$("#Phone").siblings("label").removeClass("text-success");
				$("#Phone").siblings("label").addClass("text-danger");
				$("#Phone").removeClass("is-valid");
				$("#Phone").addClass("is-invalid");
			}
		}
		else
		{
			$("#Phone").removeClass("is-invalid");
			$("#Phone").removeClass("is-valid");
			$("#Phone").siblings("label").html("휴대폰 번호");
			$("#Phone").siblings("label").removeClass("text-danger");
			$("#Phone").siblings("label").removeClass("text-success");
		}
		ValidateForm();
	});
	$("#Agreement").change(function ()
	{
		ValidateForm();
	});
	$("#Register").click(function ()
	{
		$("#Email").prop("disabled", false);
	});
});