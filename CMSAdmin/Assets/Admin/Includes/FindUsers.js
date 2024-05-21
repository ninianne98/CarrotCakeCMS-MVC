var webSvc = cmsGetServiceAddress();
var resFld = "#spanResults";
var hdnFld = "#";
var fldSearch = "#";
var srchMthd = 'FindUsers';
var hasResults = false;

function resetSearchFields() {
	$(hdnFld).val('');
	$(resFld).html('&nbsp;');
}

function initFindUsers(hiddenField, searchBox) {
	hdnFld = '#' + hiddenField;
	fldSearch = '#' + searchBox;
	srchMthd = 'FindUsers';

	bindBehavior();
}

function initFindUsersMethod(hiddenField, searchBox, methodName) {
	hdnFld = '#' + hiddenField;
	fldSearch = '#' + searchBox;
	srchMthd = methodName;

	bindBehavior();
}

function loadResults(data, response) {
	response($.map(data.d, function (item) {
		return {
			value: item.UserName + " (" + item.Email + ")",
			id: item.UserId
		}
	}));

	if (data.d.length < 1) {
		$(resFld).attr('style', 'color: #990000;');
		$(resFld).text('  No Results  ');
		hasResults = false;
	} else {
		$(resFld).attr('style', 'color: #009900;');
		if (data.d.length == 1) {
			$(resFld).text('  ' + data.d.length + ' Result  ');
		} else {
			$(resFld).text('  ' + data.d.length + ' Results  ');
		}
		hasResults = true;
	}
}

function bindBehavior() {
	$(fldSearch).on('change keyup paste textchange', function () {
		if ($(fldSearch).val().length < 1) {
			resetSearchFields();
		}
	});

	$(fldSearch).focus(function () {
		if (hasResults && $('#ui-id-1').length > 0) {
			if ($(fldSearch).val().length >= 1) {
				$('#ui-id-1').css('display', '');
			}
		}
	});

	$.ajaxSetup({ cache: false });

	var webMthd = webSvc + "/" + srchMthd;

	$(fldSearch).autocomplete({
		source: function (request, response) {
			hasResults = false;
			resetSearchFields();
			var search = MakeStringSafe(request.term);

			$.ajax({
				url: webMthd,
				type: 'POST',
				dataType: 'json',
				contentType: "application/json; charset=utf-8",
				data: JSON.stringify({ searchTerm: search }),
				dataFilter: function (data) { return data; }
			}).done(function (data, status, xhr) {
				loadResults(data, response);
			}).fail(function (xhr, status, error) {
				resetSearchFields();
				cmsAjaxFailed(xhr);
			});
		},

		select: function (event, ui) {
			resetSearchFields();
			if (ui.item) {
				$(hdnFld).val(ui.item.id);
			}
			$(resFld).html('&nbsp;');
		},
		minLength: 1,
		delay: 1000
	});
}