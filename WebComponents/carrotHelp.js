/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, 2023, 2024 Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
* Generated: [[TIMESTAMP]]
*/

//==================== rudimentary callback functions
function __carrotOnAjaxRequestBegin() {
	console.log("This is the __carrotOnAjaxRequestBegin Callback.");
}
function __carrotOnAjaxRequestSuccess(data, status, xhr) {
	console.log("This is the __carrotOnAjaxRequestSuccess Callback: " + status);
}
function __carrotOnAjaxRequestFailure(xhr, status, error) {
	console.log("This is the __carrotOnAjaxRequestFailure Callback: " + status);
	console.log(error);
	console.log(xhr.responseText);
}
function __carrotOnAjaxRequestFailureAlert(xhr, status, error) {
	//alert("This is the __carrotOnAjaxRequestFailure Callback: \n" + error + "\n------------------\n" + xhr.responseText);
	__carrotSimpleOkAlert("<div style='font-size:12pt;'>This is the __carrotOnAjaxRequestFailureAlert Callback: \n <br />" + error + "<br />\n <hr /> \n <pre>" + xhr.responseText + "</pre></div>");

	console.log("This is the __carrotOnAjaxRequestFailureAlert Callback: " + status);
	console.log(error);
	console.log(xhr.responseText);
}

function __carrotOnAjaxRequestComplete(xhr, status) {
	console.log("This is the __carrotOnAjaxRequestComplete Callback: " + status);
}

function __carrotUserAjaxFunction(funcName) {
	//if blank/undefined, create a dummy function
	if (funcName == undefined || !funcName.length) {
		return Function.constructor.apply(null, ['', '']);
	}
	// if actually a function, use it
	if (typeof funcName === 'function') return funcName;

	var wndw = window;
	var func = funcName.split('.');

	//walk window for the specified function
	if (wndw && func.length > 0) {
		while (func.length) {
			wndw = wndw[func.shift()];
		}
	}
	// if found the function, return
	if (typeof (wndw) === 'function') return wndw;

	// not a function, report it
	console.log(funcName + ": not a function");
	return null;
}

/*
$(document).on("click", "form[data-ajax=true] :submit", function (e) {
	e.preventDefault();

	var theForm = this.form;
	//console.log("the form id:  " + $(theForm).attr('id'));

	var frm = $(theForm);
	var postUri = frm.attr('action');

	// check if client side validated, if not present, assume left to server side
	var validated = (frm.attr('data-ajax-valid') || 'true') == 'true';

	if (!validated) {
		// client side validation failed
		return;
	}

	var formType = frm.attr('data-ajax-method').toUpperCase();
	var placeholder = frm.attr('data-ajax-update');
	var fillMode = frm.attr('data-ajax-mode').toUpperCase();

	var onBegin = frm.attr('data-ajax-begin') || '';
	var onSuccess = frm.attr('data-ajax-success') || '';
	var onComplete = frm.attr('data-ajax-complete') || '';
	var onFailure = frm.attr('data-ajax-failure') || '';

	var frmData = frm.serialize();
	//console.log(frmData);

	// https://api.jquery.com/jQuery.ajax/
	// The jqXHR.success(), jqXHR.error(), and jqXHR.complete() callbacks are removed as of jQuery 3.0.
	// You can use jqXHR.done(), jqXHR.fail(), and jqXHR.always() instead.
	// major version check to avoid the deprecated/obsolete methods
	var arr = $.fn.jquery.split('.');

	if (arr[0] < 3) {
		//console.log("jQuery < 3");
		$.ajax({
			type: formType,
			url: postUri,
			contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
			data: frmData,

			beforeSend: function (xhr) {
				__carrotUserAjaxFunction(onBegin).apply(frm, arguments);
			},
			complete: function () {
				__carrotUserAjaxFunction(onComplete).apply(frm, arguments);
			},
			success: function (result, status, xhr) {
				switch (fillMode) {
					case "BEFORE":
						$(placeholder).prepend(result);
						break;
					case "AFTER":
						$(placeholder).append(result);
						break;
					default:
						$(placeholder).html(result);
						break;
				};

				__carrotUserAjaxFunction(onSuccess).apply(frm, arguments);
			},
			error: function (xhr, status, error) {
				console.log('Failed Carrot Form Post: ' + error);
				__carrotUserAjaxFunction(onFailure).apply(frm, arguments);
			}
		});
	} else {
		//console.log("jQuery >= 3");
		$.ajax({
			type: formType,
			url: postUri,
			contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
			data: frmData,

			beforeSend: function (xhr) {
				__carrotUserAjaxFunction(onBegin).apply(frm, arguments);
			}
		}).done(function (result, status, xhr) {
			//console.log("done/success");
			switch (fillMode) {
				case "BEFORE":
					$(placeholder).prepend(result);
					break;
				case "AFTER":
					$(placeholder).append(result);
					break;
				default:
					$(placeholder).html(result);
					break;
			};
			__carrotUserAjaxFunction(onSuccess).apply(frm, arguments);
		}).fail(function (xhr, status, error) {
			//console.log("fail/error");
			__carrotUserAjaxFunction(onFailure).apply(frm, arguments);
		}).always(function (parm1, status, parm3) {
			//console.log("always/complete");
			__carrotUserAjaxFunction(onComplete).apply(frm, arguments);
		});
	}
});
*/

function __carrotAjaxPostForm(formid, div, postUri, replace) {
	var data = $("#" + formid).serialize();
	//console.log(formid + ' ' + div + " ---------------------- ");
	//console.log(data);

	$.ajax({
		type: 'POST',
		url: postUri,
		contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
		data: data,
		success: function (result, status, xhr) {
			//console.log(result);
			if (replace == true) {
				$("#" + div).html(result);
			}
		},
		error: function (xhr, status, error) {
			console.log('Failed __carrotAjaxPostForm : ' + error);
			__carrotOnAjaxRequestFailure(xhr, status, error);
		}
	})
}

function __carrotAjaxPostFormData(data, div, postUri, replace) {
	//console.log(div + " ---------------------- ");
	//console.log(data);

	$.ajax({
		type: 'POST',
		url: postUri,
		contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
		data: data,
		success: function (result, status, xhr) {
			//console.log(result);
			if (replace == true) {
				$("#" + div).html(result);
			}
		},
		error: function (xhr, status, error) {
			console.log('Failed __carrotAjaxPostFormData : ' + error);
			__carrotOnAjaxRequestFailure(xhr, status, error);
		}
	})
}

function __OnAjaxRequestBegin() {
	__carrotOnAjaxRequestBegin();
}
function __OnAjaxRequestSuccess(data, status, xhr) {
	__carrotOnAjaxRequestSuccess(data, status, xhr);
}
function __OnAjaxRequestFailure(xhr, status, error) {
	__carrotOnAjaxRequestFailure(xhr, status, error);
}
function __OnAjaxRequestComplete(xhr, status) {
	__carrotOnAjaxRequestComplete(xhr, status);
}

//==================== dateTime stuff
function __carrotGetTimeFormat() {
	return "[[SHORTTIMEPATTERN]]";
}

function __carrotGetDateFormat() {
	return "[[SHORTDATEPATTERN]]";
}

function __carrotGetAMDateFormat() {
	return "[[AM_TIMEPATTERN]]";
}

function __carrotGetPMDateFormat() {
	return "[[PM_TIMEPATTERN]]";
}

function __carrotGetDateTemplate() {
	return "[[SHORTDATEFORMATPATTERN]]";
}

function __carrotGetDateTimeTemplate() {
	return "[[SHORTDATETIMEFORMATPATTERN]]";
}

//================================================================

function __carrotAlertModalBtns(request, title, buttonsOpts) {
	__carrotAlertModalHeightWidthBtns(request, title, 400, 550, buttonsOpts);
}
function __carrotAlertModalSmallBtns(request, title, buttonsOpts) {
	__carrotAlertModalHeightWidthBtns(request, title, 250, 400, buttonsOpts);
}
function __carrotAlertModalLargeBtns(request, title, buttonsOpts) {
	__carrotAlertModalHeightWidthBtns(request, title, 550, 700, buttonsOpts);
}

function __carrotSimpleOkAlert(message) {
	var opts = {
		"OK": function () { __carrotAlertModalClose(); }
	};

	__carrotAlertModalBtns(message, 'Alert', opts);

	return false;
}

function __carrotAlertModalClose() {
	$("#carrot-genericjqmodal").dialog("close");
	$("#carrot-genericjqmodal-msg").html('');
}

$(document).ready(function () {
	if ($('#carrot-genericjqmodal-zone').length === 0) {
		$("body").append('<div id="carrot-genericjqmodal-zone" style="display:none;"><div id="carrot-genericjqmodal" title="carrot dialog"><div id="carrot-genericjqmodal-msg">&nbsp;</div></div></div>');
	}
});

function __carrotAlertModalHeightWidthBtns(request, title, h, w, buttonsOpts) {
	if (title.length < 1) {
		title = "General Dialog";
	}
	if ($('#carrot-genericjqmodal-zone').length === 0) {
		$("body").append('<div id="carrot-genericjqmodal-zone" style="display:none;"><div id="carrot-genericjqmodal" title="carrot dialog"><div id="carrot-genericjqmodal-msg">&nbsp;</div></div></div>');
	} else {
		$("#carrot-genericjqmodal-msg").html('');
	}

	$("#carrot-genericjqmodal-msg").html(request);

	$("#carrot-genericjqmodal").dialog({
		open: function () {
			$(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
		},
		title: title,
		height: h,
		width: w,
		modal: true,
		buttons: buttonsOpts
	});
}

//================================================================