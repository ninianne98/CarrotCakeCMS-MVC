function initCheckboxStyle() {
	$('input').iCheck({
		checkboxClass: 'icheckbox_grey-alt4',
		radioClass: 'iradio_grey-alt4'
	});

	$('.iradio_grey-alt4, .icheckbox_grey-alt4').each(function () {
		var ipt = $(this).find("input");

		if (ipt.length > 0) {
			var onclick = $(ipt).attr("onclick");

			$(ipt).on('ifClicked', function (event) {
				if (onclick != undefined && onclick.length > 0) {
					if ($(ipt).attr("type") == 'radio') {
						setTimeout(function () { $(ipt).click(); }, 200);
					} else {
						$(ipt).trigger("click");
					}
				}
			});
		}
	});

	$('.icheckbox_grey-alt4 input[type=checkbox]').on('change', function () {
		if ($(this).prop('checked')) {
			$(this).parent().addClass("checked");
			setTimeout(function () { $(this).iCheck('check'); }, 150);
		} else {
			$(this).parent().removeClass("checked");
			setTimeout(function () { $(this).iCheck('uncheck'); }, 150);
		}
	});
}

$(document).ready(function () {
	initCheckboxStyle();
});