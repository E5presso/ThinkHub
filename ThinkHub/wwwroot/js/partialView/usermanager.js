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

function InitUserManager()
{
	var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
	var padding = parseInt($("#UserManager").css("paddingTop"));

	$("#UserManager").css("top", gnb);
	$("#UserManager").height($(window).height() - gnb - (padding * 2));
	$(window).resize(function ()
	{
		var gnb = parseInt($(".gnb").height()) + (parseInt($(".gnb").css("paddingTop")) * 2);
		var padding = parseInt($("#UserManager").css("paddingTop"));

		$("#UserManager").height($(window).height() - gnb - (padding * 2));
	});
}

$(document).ready(function ()
{
	InitUserManager();
});