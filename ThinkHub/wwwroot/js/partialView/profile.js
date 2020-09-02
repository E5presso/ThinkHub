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
	if ($("#Name").hasClass("is-valid") &&
		$("#Birthday").hasClass("is-valid") &&
		$("#Phone").hasClass("is-valid") &&
		$("#Update").prop("disabled", false));
	else $("#Update").prop("disabled", true);
}

$(document).ready(function ()
{
	var gnb = $(".gnb").outerHeight();
	var bound = $(window).innerHeight();
	var height = $("#ProfileForm").outerHeight();
	$("#ProfileForm").css("top", gnb + ((bound - gnb - height) / 2));
	$(window).resize(function ()
	{
		var gnb = $(".gnb").outerHeight();
		var bound = $(window).innerHeight();
		var height = $("#ProfileForm").outerHeight();
		$("#ProfileForm").css("top", gnb + ((bound - gnb - height) / 2));
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
			$("#Birthday").removeClass("is-invalid");
			$("#Birthday").removeClass("is-valid");
			$("#Birthday").siblings("label").html("생년월일");
			$("#Birthday").siblings("label").removeClass("text-danger");
			$("#Birthday").siblings("label").removeClass("text-success");
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
	$(".custom-file-input").on("change", function ()
	{
		var fileName = $(this).val().split("\\").pop();
		$(this).siblings(".custom-file-label").addClass("selected").html(fileName);
		ValidateForm();
	});

	$("#Name").trigger("keyup");
	$("#Birthday").trigger("change");
	$("#Phone").trigger("keyup");
});