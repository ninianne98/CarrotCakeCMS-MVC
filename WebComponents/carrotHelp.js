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
	alert("This is the __OnAjaxRequestBegin Callback");
}
function __carrotOnAjaxRequestSuccess(data, status, xhr) {
	alert("This is the __OnAjaxRequestSuccess: " + data);
}
function __carrotOnAjaxRequestFailure(xhr, status, error) {
	alert("This is the __OnAjaxRequestFailure Callback:" + error + "\r\n------------------\r\n" + xhr.responseText);
}
function __carrotOnAjaxRequestComplete(xhr, status) {
	alert("This is the __OnAjaxRequestComplete Callback: " + status);
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

//================================================================