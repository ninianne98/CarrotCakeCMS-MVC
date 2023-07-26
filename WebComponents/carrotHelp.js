/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
* Generated: [[TIMESTAMP]]
*/

//==================== rudimentary callback functions
function __carrotOnAjaxRequestBegin(xhr) {
	console.log("This is the __OnAjaxRequestBegin Callback");
}
function __carrotOnAjaxRequestSuccess(data, status, xhr) {
	console.log("This is the __OnAjaxRequestSuccess: " + data);
}
function __carrotOnAjaxRequestFailure(xhr, status, error) {
	alert("This is the __OnAjaxRequestFailure Callback:" + error + "\r\n------------------\r\n" + xhr.responseText);
	console.log(error);
	console.log(xhr.responseText);
}
function __carrotOnAjaxRequestComplete(xhr, status) {
	console.log("This is the __OnAjaxRequestComplete Callback: " + status);
}

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
			console.log('Failed __carrotAjaxPostForm');
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
			console.log('Failed __carrotAjaxPostForm');
			__carrotOnAjaxRequestFailure(xhr, status, error);
		}
	})
}

function __OnAjaxRequestBegin(xhr) {
	__carrotOnAjaxRequestBegin(xhr);
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
