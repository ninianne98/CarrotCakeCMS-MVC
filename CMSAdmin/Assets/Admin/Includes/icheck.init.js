$(document).ready(function () {
	$('input').iCheck({
		checkboxClass: 'icheckbox_grey-alt2',
		radioClass: 'iradio_grey-alt2'
	});

	$('.iradio_grey-alt2, .icheckbox_grey-alt2').each(function () {
		var chk = $(this).find("input");
		var onclick = $(chk).attr("onclick");

		if (chk.length > 0 && onclick != undefined && onclick.length > 0) {
			$(chk).on('ifClicked', function (event) {
				if ($(chk).attr("type") == 'radio') {
					setTimeout(function () { $(chk).click(); }, 200);
				} else {
					$(chk).trigger("click");
				}
			});
		}
	});
});