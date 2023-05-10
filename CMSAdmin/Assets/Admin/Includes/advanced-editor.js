if (typeof jQuery === 'undefined') {
	throw new Error('Advanced Editor JavaScript requires jQuery')
}

var cmsIsPageLocked = true;

function cmsSetPageStatus(stat) {
	cmsIsPageLocked = stat;
}

var webSvc = "/Assets/Admin/CMS.asmx";
var adminUri = "/c3-admin/";
var thisPage = ""; // used in escaped fashion
var thisPageNav = "";  // used non-escaped (redirects)
var thisPageNavSaved = "";  // used non-escaped (redirects)
var thisPageID = "";
var timeTick = 9999;

function cmsGetAdminPath() {
	var webMthd = webSvc + "/GetSiteAdminFolder";

	$.ajax({
		type: "POST",
		url: webMthd,
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		success: cmsSetAdminPath,
		error: cmsAjaxFailed
	});
}

function cmsSetAdminPath(data, status) {
	adminUri = data.d;
}

$(document).ready(function () {
	cmsGetAdminPath();
});

function cmsSetServiceParms(serviceURL, pagePath, pageID) {
	cmsGetAdminPath();

	webSvc = serviceURL;
	thisPageNav = pagePath;
	thisPage = cmsMakeStringSafe(pagePath);
	thisPageID = pageID;

	var d = new Date();
	timeTick = d.getTime();
}

function cmsOverridePageName(pagePath) {
	thisPageNavSaved = pagePath;
}

var cmsConfirmLeavingPage = true;

$(window).bind('beforeunload', function () {
	//cmsConfirmLeavingPage = false;
	if (!cmsIsPageLocked) {
		if (cmsConfirmLeavingPage) {
			return '>>Are you sure you want to navigate away<<';
		}
	}
});

function cmsGetPageStatus() {
	return cmsConfirmLeavingPage;
}

function cmsGetPageLock() {
	return cmsIsPageLocked;
}

// ===============================================
// do menu stuff / icons etc

var cmsMnuVis = true;

function cmsToggleMenu() {
	var t = $('#cmsMainToolbox');
	var s = $('#cmsToolboxSpacer');
	var m = $('#cmsMnuToggle');
	//alert(m.attr('id'));
	if (cmsMnuVis) {
		m.attr('class', 'ui-icon ui-icon-plusthick cmsFloatRight');
		t.css('display', 'none');
		s.css('display', 'block');
		cmsMnuVis = false;
	} else {
		m.attr('class', 'ui-icon ui-icon-minusthick cmsFloatRight');
		t.css('display', 'block');
		s.css('display', 'none');
		cmsMnuVis = true;
	}
}

var cmsToolbarMargin = 'L';
function cmsShiftPosition(p) {
	var toolbox = 'cmsToolBoxWrap';

	if (p == 'L') {
		cmsToolbarMargin = 'L';
		$("#" + toolbox).removeClass("cmsToolbarAlignmentR").addClass("cmsToolbarAlignmentL");
	} else {
		cmsToolbarMargin = 'R';
		$("#" + toolbox).removeClass("cmsToolbarAlignmentL").addClass("cmsToolbarAlignmentR");
	}
}

$(document).ready(function () {
	setTimeout("cmsResetToolbarScroll()", 1250);
});

var cmsToolTabIdx = 0;
var cmsScrollPos = 0;
var cmsScrollWPos = 0;

function cmsSetPrefs(tab, margin, scrollPos, scrollWPos, opStat) {
	cmsToolTabIdx = tab;
	cmsToolbarMargin = margin;
	cmsScrollPos = scrollPos;
	cmsScrollWPos = scrollWPos;
	cmsMnuVis = opStat;
}

function cmsResetToolbarScroll() {
	setTimeout("cmsShiftPosition('" + cmsToolbarMargin + "')", 100);

	if (!cmsMnuVis) {
		cmsMnuVis = true;
		cmsToggleMenu();
	}

	$(document).scrollTop(cmsScrollPos);
	$('#cmsToolBox').scrollTop(cmsScrollWPos);

	setTimeout(function () {
		$(document).scrollTop(cmsScrollPos);
		$('#cmsToolBox').scrollTop(cmsScrollWPos);
	}, 250);
}

function cmsMenuFixImages() {
	var styleIcons = [
		{ styleName: '.cmsWidgetBarIconCog', img: 'cog.png' },
		{ styleName: '.cmsWidgetBarIconCross', img: 'cross.png' },
		{ styleName: '.cmsWidgetBarIconDup', img: 'shape_ungroup.png' },
		{ styleName: '.cmsWidgetBarIconTime', img: 'clock_edit.png' },
		{ styleName: '.cmsWidgetBarIconCopy', img: 'table_go.png' },
		{ styleName: '.cmsWidgetBarIconPencil', img: 'pencil.png' },
		{ styleName: '.cmsWidgetBarIconPencil2', img: 'pencil.png' },
		{ styleName: '.cmsWidgetBarIconActive', img: 'tick.png' },
		{ styleName: '.cmsWidgetBarIconWidget', img: 'application_view_tile.png' },
		{ styleName: '.cmsWidgetBarIconWidget2', img: 'hourglass.png' },
		{ styleName: '.cmsWidgetBarIconShrink', img: 'arrow_in.png' }
	];

	$.each(styleIcons, function (i, o) {
		//console.log(styleIcons[i].styleName);
		var styleName = styleIcons[i].styleName;
		var imgIcon = styleIcons[i].img;

		$(styleName).each(function (j) {
			cmsFixGeneralImage(this, imgIcon);
		});
	});
}

function cmsBlockImageEdits(elm) {
	$(elm).attr('style', 'display: none;');
}

function cmsBlockContentEdits(elm) {
	var txt = $(elm).text();
	$(elm).attr('style', 'display: none;');
	$(elm).parent().text(txt);
}

function cmsFixGeneralImage(elm, img) {
	var id = $(elm).attr('id');
	var title = $(elm).attr('title');
	var alt = $(elm).attr('alt');

	$(elm).html(" <img class='cmsWidgetBarImgReset' border='0' src='/Assets/Admin/images/" + img + "' alt='" + alt + "' title='" + alt + "' />" + title);
}

function cmsPageLockCheck() {
	if (!cmsIsPageLocked) {
		setTimeout("cmsMenuFixImages();", 250);
		setTimeout("cmsMenuFixImages();", 2500);
		setTimeout("cmsMenuFixImages();", 5000);

		// these click events because of stoopid IE9 navigate away behavior
		$('#cmsEditMenuList a').each(function (i) {
			$(this).click(function () {
				cmsMakeOKToLeave();
				setTimeout("cmsMakeNotOKToLeave();", 250);
			});
		});

		$('#cmsEditMenuList').each(function (i) {
			$(this).click(function () {
				cmsMakeOKToLeave();
				setTimeout("cmsMakeNotOKToLeave();", 250);
			});
		});

		$('.cmsContentContainerInnerTitle a').each(function (i) {
			$(this).click(function () {
				cmsMakeOKToLeave();
				setTimeout("cmsMakeNotOKToLeave();", 250);
			});
		});

		$('.cmsContentContainerInnerTitle').each(function (i) {
			$(this).click(function () {
				cmsMakeOKToLeave();
				setTimeout("cmsMakeNotOKToLeave();", 250);
			});
		});
	} else {
		$("#cmsEditMenuList ul").each(function (i) {
			cmsBlockImageEdits(this);
		});

		$(".cmsContentContainerTitle a").each(function (i) {
			cmsBlockContentEdits(this);
		});
	}
}

function cmsMakeOKToLeave() {
	cmsConfirmLeavingPage = false;
}

function cmsMakeNotOKToLeave() {
	cmsConfirmLeavingPage = true;
}

function cmsRequireConfirmToLeave(confirmLeave) {
	cmsConfirmLeavingPage = confirmLeave;
}

function cmsGetOKToLeaveStatus() {
	return cmsConfirmLeavingPage;
}

//=============================================

function cmsMakeStringSafe(val) {
	val = Base64.encode(val);
	return val;
}

function cmsEditHB() {
	if (!cmsIsPageLocked) {
		setTimeout("cmsEditHB();", 25 * 1000);

		var webMthd = webSvc + "/RecordHeartbeat";

		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ PageID: thisPageID }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: cmsUpdateHeartbeat,
			error: cmsAjaxFailed
		});
	}
}

function cmsSaveToolbarPosition() {
	var scrollTopPos = $(document).scrollTop();
	var scrollWTopPos = $('#cmsToolBox').scrollTop();
	var tabID = $('#cmsTabbedToolbox').tabs("option", "active");

	var webMthd = webSvc + "/RecordEditorPosition";

	$.ajax({
		type: "POST",
		url: webMthd,
		data: JSON.stringify({ ToolbarState: cmsMnuVis, ToolbarMargin: cmsToolbarMargin, ToolbarScroll: scrollTopPos, WidgetScroll: scrollWTopPos, SelTabID: tabID }),
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		success: cmsAjaxGeneralCallback,
		error: cmsAjaxFailed
	});
}

function cmsSaveContent(val, zone) {
	cmsSaveToolbarPosition();

	var webMthd = webSvc + "/CacheContentZoneText";

	val = cmsMakeStringSafe(val);
	zone = cmsMakeStringSafe(zone);

	$.ajax({
		type: "POST",
		url: webMthd,
		data: JSON.stringify({ ZoneText: val, Zone: zone, ThisPage: thisPageID }),
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		success: cmsSaveContentCallback,
		error: cmsAjaxFailed
	});
}

function cmsSaveGenericContent(val, key) {
	cmsSaveToolbarPosition();

	var webMthd = webSvc + "/CacheGenericContent";

	val = cmsMakeStringSafe(val);

	$.ajax({
		type: "POST",
		url: webMthd,
		data: JSON.stringify({ ZoneText: val, DBKey: key, ThisPage: thisPageID }),
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		success: cmsSaveContentCallback,
		error: cmsAjaxFailed
	});
}

var cmsTemplatePreview = "";

function cmsSetPreviewFileName(tmplName) {
	cmsTemplatePreview = tmplName;
}

function cmsPreviewTemplate2() {
	var tmpl = $(cmsTemplateListPreviewer).val();
	tmpl = cmsMakeStringSafe(tmpl);

	var srcURL = cmsTemplatePreview + "?carrot_templatepreview=" + tmpl;

	var editIFrame = $('#cmsFrameEditorPreview');
	$(editIFrame).attr('src', srcURL);
	$(editIFrame).attr('realsrc', srcURL);

	setTimeout("cmsSetIframeRealSrc('cmsFrameEditorPreview');", 500);

	$('#cmsFrameEditorPreview')[0].contentWindow.location.reload(true);
}

var cmsTemplateListPreviewer = "#cmsTemplateList"

function cmsWideMobile() {
	cmsWidePreview('475px');
}

function cmWideTablet() {
	cmsWidePreview('750px');
}

function cmsWideDesk() {
	cmsWidePreview('98%');
}

function cmsWidePreview(width) {
	$('#cmsFrameEditorPreview').attr('width', width);
	$('#cmsFrameEditorPreview').css('width', width);
}

function cmsPreviewTemplate() {
	var tmplReal = $(cmsTemplateDDL).val();
	tmpl = cmsMakeStringSafe(tmplReal);

	cmsLaunchWindowOnly(cmsTemplatePreview + "?carrot_templatepreview=" + tmpl);

	var editFrame = $('#cmsModalFrame');

	var templateList = '';

	$(cmsTemplateDDL + " > option").each(function () {
		templateList += "<option value='" + this.value + "'>" + this.text + "</option>";
	});

	var btnWide1 = '<a id="btnDeskTemplateCMS" class="cms-seagreen" name="btnWidthTemplateCMS" onclick="cmsWideDesk();"> Desktop Size </a>';
	var btnWide2 = '<a id="btnTabletTemplateCMS" class="cms-seagreen" name="btnWidthTemplateCMS" onclick="cmWideTablet();"> Tablet Size </a>';
	var btnWide3 = '<a id="btnMobileTemplateCMS" class="cms-seagreen" name="btnWidthTemplateCMS" onclick="cmsWideMobile();"> Mobile Size </a>';

	var ddlPreview = '<span> <select class="cms-seagreen" id="cmsTemplateList">' + templateList + '</select>  <input type="button" value="Preview" id="btnPreviewCMS" onclick="cmsPreviewTemplate2();" /> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; </span>';

	var btnClose = ' <input type="button" class="cms-seagreen" id="btnCloseTemplateCMS" value="Close" onclick="cmsCloseModalWin();" /> &nbsp;&nbsp;&nbsp; ';
	var btnApply = ' <input type="button" class="cms-seagreen" id="btnApplyTemplateCMS" value="Apply Template" onclick="cmsUpdateTemplate();" /> &nbsp;&nbsp;&nbsp; ';

	$(editFrame).append('<div id="cms-seagreen-id" class="cms-seagreen"><div id="cmsPreviewControls" class="cms-seagreen cmsPreviewButtons"> '
					+ '<div id="cmsPreviewTab" class="cmsTabs cmsPreview-Left cms-seagreen"><ul class="cms-seagreen"> <li>' + btnWide1 + '</li> <li>' + btnWide2 + '</li> <li>' + btnWide3 + '</li>  </ul> '
					+ ' \r\n <div id="cmsPreviewTabPanel" style="display: none;" >    </div>\r\n   </div> '
					+ ' <div class="cmsPreview-Right cms-seagreen">' + ddlPreview + btnClose + btnApply + '</div> </div>'
					+ ' </div>');

	window.setTimeout("cmsPreviewStyling();", 250);

	$(cmsTemplateListPreviewer).val(tmplReal);

	$('#cmsFrameEditor').attr('id', 'cmsFrameEditorPreview');
	$('#cmsAjaxMainDiv2').attr('id', 'cmsAjaxMainDiv3');

	setTimeout("cmsSetIframeRealSrc('cmsFrameEditorPreview');", 1000);
	cmsStyleButtons();

	cmsWideDesk();
	setTimeout("$('#btnDeskTemplateCMS').click();", 750);
	setTimeout("$('#btnDeskTemplateCMS').click();", 1500);
}

function cmsPreviewStyling() {
	cmsLateBtnStyle();

	$('#cmsPreviewTab').find('li > a').each(function (i) {
		var tabId = 'cms-pvtab-' + i;
		$(this).attr('href', '#' + tabId);
		$('#cmsPreviewTabPanel').append('<div id="' + tabId + '"> <p>' + tabId + '</p> </div>\r\n ');
	});

	$('#cmsPreviewTab').tabs();
	$('#cmsPreviewTab').addClass('ui-corner-all');
	$('#cmsPreviewTab li').addClass('ui-widget ui-state-default ui-corner-all');

	window.setTimeout("cmsSetPreviewSize()", 500);
	window.setTimeout("cmsSetPreviewSize()", 750);
	window.setTimeout("cmsSetPreviewSize()", 1500);

	window.setTimeout("$('#cmsPreviewTab').tabs('option', 'active', 0);", 250);
}

function cmsSetPreviewSize() {
	var modSel = '#cms-simplemodal-container';
	var frameSel = '#cmsModalFrame';

	var modH = $(modSel).css('height');
	var modW = $(modSel).css('width');

	var frmH = parseFloat(modH) - 85;
	var frmW = parseFloat(modW) - 30;

	$(frameSel).css('height', frmH);
	$(frameSel).attr('height', frmH);

	$(frameSel).css('width', frmW);
	$(frameSel).attr('width', frmW);
}

function cmsLateBtnStyle() {
	$(".cms-seagreen input:button, .cms-seagreen input:submit, .cms-seagreen input:reset").button();
	$("#cms-seagreen-id input:button, #cms-seagreen-id input:submit, #cms-seagreen-id input:reset").button();
}

var cmsTemplateDDL = "";

function cmsSetTemplateDDL(ddlName) {
	cmsTemplateDDL = ddlName;
}

function cmsUpdateTemplate() {
	cmsSaveToolbarPosition();

	var tmpl = '';

	if ($(cmsTemplateListPreviewer).length) {
		tmpl = $(cmsTemplateListPreviewer).val();
	} else {
		tmpl = $(cmsTemplateDDL).val();
	}

	tmpl = cmsMakeStringSafe(tmpl);

	var webMthd = webSvc + "/UpdatePageTemplate";

	$.ajax({
		type: "POST",
		url: webMthd,
		data: JSON.stringify({ TheTemplate: tmpl, ThisPage: thisPageID }),
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		success: cmsSaveContentCallback,
		error: cmsAjaxFailed
	});
}

var cmsWidgetUpdateInProgress = false;

function cmsUpdateWidgets() {
	cmsSaveToolbarPosition();
	cmsSpinnerLong();

	var webMthd = webSvc + "/CacheWidgetUpdate";

	if (!cmsWidgetUpdateInProgress) {
		cmsWidgetUpdateInProgress = true;

		var val = $("#cmsFullOrder").val();

		val = cmsMakeStringSafe(val);

		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ WidgetAddition: val, ThisPage: thisPageID }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: cmsSaveWidgetsCallback,
			error: cmsAjaxFailed
		});
	}
}

function cmsRemoveWidget(key) {
	cmsSaveToolbarPosition();
	cmsSpinnerLong();

	var webMthd = webSvc + "/RemoveWidget";

	if (!cmsWidgetUpdateInProgress) {
		cmsWidgetUpdateInProgress = true;

		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ DBKey: key, ThisPage: thisPageID }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: cmsSaveWidgetsCallback,
			error: cmsAjaxFailed
		});
	}
}

function cmsActivateWidgetLink(key) {
	cmsSaveToolbarPosition();
	cmsSpinnerLong();

	var webMthd = webSvc + "/ActivateWidget";

	if (!cmsWidgetUpdateInProgress) {
		cmsWidgetUpdateInProgress = true;

		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ DBKey: key, ThisPage: thisPageID }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: cmsSaveWidgetsCallback,
			error: cmsAjaxFailed
		});
	}
}

function cmsMoveWidgetZone(zone, val) {
	cmsSaveToolbarPosition();
	cmsSpinnerLong();

	var webMthd = webSvc + "/MoveWidgetToNewZone";

	if (!cmsWidgetUpdateInProgress) {
		cmsWidgetUpdateInProgress = true;

		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ WidgetTarget: zone, WidgetDropped: val, ThisPage: thisPageID }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: cmsSaveWidgetsCallback,
			error: cmsAjaxFailed
		});
	}
}

var IsPublishing = false;

function cmsApplyChanges() {
	var webMthd = webSvc + "/PublishChanges";

	// prevent multiple submissions
	if (!IsPublishing) {
		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ ThisPage: thisPageID }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: cmsSavePageCallback,
			error: cmsAjaxFailed
		});
	}

	IsPublishing = true;
}

function cmsUpdateHeartbeat(data, status) {
	var hb = $('#cmsHeartBeat');
	hb.empty().append('HB:  ');
	hb.append(data.d);
	//cmsSpinnerShort();
}

function cmsSaveContentCallback(data, status) {
	if (data.d == "OK") {
		cmsSpinnerShort();
		cmsDirtyPageRefresh();
	} else {
		cmsAlertModal(data.d);
	}
}

function cmsAjaxGeneralCallback(data, status) {
	if (data.d != "OK") {
		cmsAlertModal(data.d);
	}
}

function cmsSaveWidgetsCallback(data, status) {
	if (data.d == "OK") {
		cmsSpinnerShort();
		cmsDirtyPageRefresh();
	} else {
		cmsAlertModal(data.d);
	}

	cmsWidgetUpdateInProgress = false;
}

function cmsSavePageCallback(data, status) {
	if (data.d == "OK") {
		cmsSpinnerShort();
		cmsMakeOKToLeave();
		cmsNotifySaved();

		iCount = 10;
		cmsCountdownWindow();
	} else {
		cmsAlertModal(data.d);
	}
}

var iCount = 0;
function cmsCountdownWindow() {
	if (iCount > 0) {
		iCount--;
		$('#cmsSaveCountdown').html(iCount);

		setTimeout("cmsCountdownWindow();", 1025);
	} else {
		window.setTimeout("location.href = \'" + thisPageNavSaved + "\'", 250);
	}
}

function cmsNotifySaved() {
	$("#CMSsavedconfirm").dialog({
		dialogClass: "no-close",
		closeOnEscape: false,
		resizable: false,
		height: 250,
		width: 400,
		modal: true,
		open: function () {
			$(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
		},
		buttons: {
			"OK": function () {
				cmsMakeOKToLeave();
				window.setTimeout("location.href = \'" + thisPageNavSaved + "\'", 250);
				$(this).dialog("close");
			}
		}
	});

	cmsFixDialog('CMSsavedconfirmmsg');
	return false;
}

function cmsRecordCancellation() {
	var webMthd = webSvc + "/CancelEditing";

	$.ajax({
		type: "POST",
		url: webMthd,
		data: JSON.stringify({ ThisPage: thisPageID }),
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		success: cmsAjaxGeneralCallback,
		error: cmsAjaxFailed
	});
}

function cmsCancelEdit() {
	$("#CMScancelconfirm").dialog({
		closeOnEscape: true,
		resizable: false,
		height: 250,
		width: 400,
		modal: true,
		open: function () {
			$(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
		},
		buttons: {
			"No": function () {
				$(this).dialog("close");
			},
			"Yes": function () {
				cmsRecordCancellation();
				cmsMakeOKToLeave();
				window.setTimeout("location.href = \'" + thisPageNav + "\'", 800);
				$(this).dialog("close");
			}
		}
	});

	cmsFixDialog('CMScancelconfirmmsg');
	return false;
}

//function cmsSendTrackbackBatch() {
//	var webMthd = webSvc + "/SendTrackbackBatch";

//	if (!cmsGetOKToLeaveStatus()) {
//		$.ajax({
//			type: "POST",
//			url: webMthd,
//			contentType: "application/json; charset=utf-8",
//			dataType: "json",
//			success: cmsAjaxGeneralCallback,
//			error: cmsAjaxFailedSwallow
//		});
//	}

//	setTimeout("cmsSendTrackbackBatch();", 15000);
//}

//setTimeout("cmsSendTrackbackBatch();", 5000);

//function cmsSendTrackbackPageBatch() {
//	var webMthd = webSvc + "/SendTrackbackPageBatch";

//	if (!cmsGetOKToLeaveStatus()) {
//		$.ajax({
//			type: "POST",
//			url: webMthd,
//			data: JSON.stringify({ ThisPage: thisPageID }),
//			contentType: "application/json; charset=utf-8",
//			dataType: "json",
//			success: cmsAjaxGeneralCallback,
//			error: cmsAjaxFailedSwallow
//		});
//	}

//	setTimeout("cmsSendTrackbackPageBatch();", 3000);
//}

//setTimeout("cmsSendTrackbackPageBatch();", 1500);

function cmsAjaxFailedSwallow(request) {
	var s = "";
	if (request.status > 0) {
		s = s + "cmsAjaxFailedSwallow(request) \r\n";
		s = s + "status: " + request.status + ' \r\n';
		s = s + "statusText:  " + request.statusText + ' \r\n';
		s = s + "responseText:  " + request.responseText + ' \r\n';
		if (typeof (console) !== "undefined" && console.log) {
			console.log(s);
		}
	}
}

function cmsAjaxFailed(request) {
	var s = "";
	if (request.status > 0) {
		s = s + "cmsAjaxFailed(request) \r\n";
		s = s + "<b>status: </b>" + request.status + '<br />\r\n';
		s = s + "<b>statusText: </b>" + request.statusText + '<br />\r\n';
		s = s + "<b>responseText: </b>" + request.responseText + '<br />\r\n';
		cmsAlertModal(s);
	}
}

function cmsAlertModalHeightWidth(request, h, w) {
	$("#CMSmodalalertmessage").html('');
	$("#CMSmodalalertmessage").html(request);

	$("#CMSmodalalert").dialog({
		height: h,
		width: w,
		modal: true,
		buttons: {
			"OK": function () {
				$(this).dialog("close");
			}
		}
	});

	cmsFixDialog('CMSmodalalertmessage');
}

function cmsAlertModal(request) {
	cmsAlertModalHeightWidth(request, 425, 550);
}
function cmsAlertModalSmall(request) {
	cmsAlertModalHeightWidth(request, 250, 400);
}
function cmsAlertModalLarge(request) {
	cmsAlertModalHeightWidth(request, 550, 700);
}
function cmsAlertModalXLarge(request) {
	cmsAlertModalHeightWidth(request, 600, 800);
}

//===========================
// do a lot of iFrame magic

function cmsShowWidgetList() {
	cmsLaunchWindow(adminUri + 'WidgetList/' + thisPageID + "?zone=cms-all-placeholder-zones");
}
function cmsManageWidgetList(zoneName) {
	//alert(zoneName);
	cmsLaunchWindow(adminUri + 'WidgetList/' + thisPageID + "?zone=" + zoneName);
}
function cmsManageWidgetHistory(widgetID) {
	//alert(widgetID);
	cmsLaunchWindow(adminUri + 'WidgetHistory/' + thisPageID + "?widgetid=" + widgetID);
}
function cmsManageWidgetTime(widgetID) {
	//alert(widgetID);
	cmsLaunchWindow(adminUri + 'WidgetTime/' + thisPageID + "?widgetid=" + widgetID);
}

function cmsShowEditPageInfo() {
	cmsLaunchWindow(adminUri + 'PageEdit/' + thisPageID);
}

function cmsShowEditPostInfo() {
	cmsLaunchWindow(adminUri + 'BlogPostEdit/' + thisPageID);
}
function cmsShowAddPage() {
	cmsLaunchWindow(adminUri + 'PageAddChild/' + thisPageID);
}
function cmsShowAddChildPage() {
	cmsLaunchWindow(adminUri + 'PageAddChild/' + thisPageID);
}
function cmsShowAddTopPage() {
	cmsLaunchWindow(adminUri + 'PageAddChild/' + thisPageID + '?addtoplevel=true');
}
function cmsEditSiteMap() {
	cmsLaunchWindow(adminUri + 'SiteMapPop');
}

function cmsSortChildren() {
	//cmsAlertModal("cmsSortChildren");
	cmsLaunchWindowOnly(adminUri + 'PageChildSort/' + thisPageID);
}

function cmsShowEditWidgetForm(w, m) {
	//cmsAlertModal("cmsShowEditWidgetForm");
	cmsLaunchWindow(adminUri + 'ContentEdit/' + thisPageID + '?widgetid=' + w + '&mode=' + m);
}

function cmsShowEditContentForm(f, m) {
	//cmsAlertModal("cmsShowEditContentForm");
	cmsLaunchWindow(adminUri + 'ContentEdit/' + thisPageID + '?field=' + f + '&mode=' + m);
}

function cmsFixSpinner() {
	$(".blockMsg img").addClass('cmsImageSpinner');
	$(".blockMsg table").addClass('cmsImageSpinnerTbl');
}

var cmsHtmlSpinner = '<table width="100%" class="cmsImageSpinnerTbl" border="0"><tr><td align="center" id="cmsSpinnerZone"><img id="cmsImageSpinnerImage" class="cmsImageSpinner" border="0" src="/Assets/Admin/images/ani-smallbar.gif"/></td></tr></table>';

function cmsSpinnerShort() {
	$("#cmsDivActive").block({
		message: cmsHtmlSpinner,
		css: { border: 'none', backgroundColor: 'transparent' },
		fadeOut: 500,
		timeout: 750,
		overlayCSS: { backgroundColor: '#FFFFFF', opacity: 0.6, border: '0px solid #000000' }
	});
	cmsFixSpinner();
}

function cmsSpinnerLong() {
	$("#cmsDivActive").block({
		message: cmsHtmlSpinner,
		css: { border: 'none', backgroundColor: 'transparent' },
		fadeOut: 10000,
		timeout: 12000,
		overlayCSS: { backgroundColor: '#FFFFFF', opacity: 0.6, border: '0px solid #000000' }
	});
	cmsFixSpinner();
}

function cmsBuildOrderAndUpdateWidgets() {
	var ret = cmsBuildOrder();

	if (ret) {
		cmsUpdateWidgets();
	}

	return true;
}

function cmsBuildOrder() {
	cmsSpinnerShort();

	$("#cmsFullOrder").val('');

	$(".cmsTargetArea").find('#cmsControl.cmsWidgetControlItem').each(function (i) {
		var txt = $(this).find('#cmsCtrlOrder');
		$(txt).val('');

		var p = $(this).parent().parent().attr('id');
		var key = $(this).find('#cmsCtrlID').val();
		txt.val(i + '\t' + p + '\t' + key);
		//alert(txt.val());

		$("#cmsFullOrder").val($("#cmsFullOrder").val() + '\r\n ' + txt.val());
	});

	return true;
}

function cmsCopyWidgetFrom(zoneName) {
	cmsLaunchWindow(adminUri + 'DuplicateWidgetFrom/' + thisPageID + "?zone=" + zoneName);
}

function cmsCopyWidget(key) {
	cmsSaveToolbarPosition();
	cmsSpinnerLong();

	var webMthd = webSvc + "/CopyWidget";

	if (!cmsWidgetUpdateInProgress) {
		cmsWidgetUpdateInProgress = true;

		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ DBKey: key, ThisPage: thisPageID }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: cmsSaveWidgetsCallback,
			error: cmsAjaxFailed
		});
	}
}

function cmsRemoveWidgetLink(v) {
	$("#CMSremoveconfirm").dialog({
		open: function () {
			$(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
		},
		closeOnEscape: true,
		resizable: false,
		height: 250,
		width: 400,
		modal: true,
		buttons: {
			"No": function () {
				$(this).dialog("close");
			},
			"Yes": function () {
				cmsRemoveWidget(v);
				$(this).dialog("close");
			}
		}
	});

	cmsFixDialog('CMSremoveconfirmmsg');
}

function cmsSetOrder(fld) {
	var id = $(fld).attr('id');
	$("#cmsMovedItem").val(id);
}

function cmsInitWidgets() {
	// make sure sort only fires once
	var cmsSortWidgets = false;

	if (!cmsIsPageLocked) {
		$('#cmsTabbedToolbox').tabs();

		$('#cmsTabbedToolbox').tabs("option", "active", cmsToolTabIdx);

		$("#cmsToolBox div.cmsToolItem").draggable({
			connectToSortable: ".cmsTargetArea",
			helper: "clone",
			revert: "invalid",
			handle: "p.cmsToolItem",
			placeholder: "cmsHighlightPH"
		});

		$(".cmsWidgetTitleBar").mousedown(function () {
			var target = $(this).parent().parent().attr("id");
			cmsMoveWidgetResizer(target, 1);
		});

		$(".cmsWidgetTitleBar").mouseup(function () {
			var target = $(this).parent().parent().attr("id");
			cmsMoveWidgetResizer(target, 0);
		});

		$(".cmsTargetMove").sortable({
			stop: function (event, ui) {
				var id = $(ui.item).attr('id');
				cmsMoveWidgetResizer(id, 0);
			},
			handle: ".cmsWidgetTitleBar",
			connectWith: ".cmsTargetMove",
			revert: true,
			dropOnEmpty: true,
			distance: 25,
			placeholder: "cmsHighlightPH",
			hoverClass: "cmsHighlightPH"
		}).disableSelection();

		$(".cmsTargetArea").bind("sortupdate", function (event, ui) {
			if (!cmsSortWidgets) {
				var id = $(ui.item).attr('id');
				var val = $(ui.item).find('#cmsCtrlID').val();
				$("#cmsMovedItem").val(val);
				setTimeout("cmsBuildOrderAndUpdateWidgets();", 500);
				cmsSortWidgets = true;
			}
		});
	}

	$("div#cmsToolBox").disableSelection();
	$("#cmsContentArea a").enableSelection();
	$("#cmsWidgetHead a").enableSelection();
}

function cmsSetValue(u) {
	var id = $(u).attr('id');
	var val = $(u).find('#cmsCtrlOrder').val();
	//alert(val);
	$("#cmsMovedItem").val(val);
	setTimeout("cmsBuildOrder();", 500);
}

function cmsShrinkWidgetHeight(key) {
	var item = $("#" + key);
	var id = $(item).attr('id');
	//alert(id);

	var zone = $(item).find('#cmsControl');
	var st = $(zone).attr('style');

	if (st == undefined || st.length < 10) {
		$(zone).attr('style', 'border: solid 0px #000000; padding: 1px; height: 100px; max-width: 650px; overflow: auto;');
	} else {
		$(zone).attr('style', '');
	}
}

function cmsMoveWidgetResizer(key, state) {
	var item = $("#" + key);
	var id = $(item).attr('id');
	var zone = $(item).find('#cmsControl');

	if (state != 0) {
		$(zone).attr('style', 'border: solid 0px #000000; padding: 1px; height: 75px; max-width: 650px; overflow: auto;');
	} else {
		$(zone).attr('style', '');
	}
}

setTimeout("cmsStyleButtons();", 500);

function cmsStyleButtons() {
	cmsDoStyleButtons('.cms-seagreen input[type="button"]');
	cmsDoStyleButtons('.cms-seagreen input[type="submit"]');
	cmsDoStyleButtons('.cms-seagreen button');
	cmsDoStyleButtons('.ui-dialog-buttonpane button');

	$(".cmsWidgetControlItem").on("dblclick", function () {
		cmsDblClickWidget(this);
	});

	$(".cmsWidgetControlTitle").on("dblclick", function () {
		cmsDblClickWidgetTarget(this);
	});
}

var cmsLastWidget = '';
var cmsLastWidgetName = '';
var cmsLastWidgetTarget = '';

function cmsDblClickWidget(item) {
	cmsLastWidget = $(item).find('#cmsCtrlID').val();
	cmsLastWidgetName = $.trim($(item).find('.cmsToolItem').text());

	$('#CMSaddconfirmmsg .cms-widget-name').text(cmsLastWidgetName);

	//console.log("cmsDblClickWidget:  " + cmsLastWidgetName);
	cmsClickAddWidget();
}

function cmsDblClickWidgetTarget(item) {
	cmsLastWidgetTarget = $.trim($(item).find('#cmsWidgetContainerName').text());

	$('#CMSaddconfirmmsg .cms-widget-target').text(cmsLastWidgetTarget);

	//console.log("cmsDblClickWidgetTarget:  " + cmsLastWidgetTarget);
	cmsClickAddWidget();
}

function cmsCreateNewWidget() {
	var widget = '<div id="cmsToolItemDiv" class="cmsToolItem cmsToolItemWrapper cms-seagreen"> \r\n ' +
					'<div class="cmsWidgetControlItem cmsWidgetToolboxItem cmsWidgetCtrlPath cms-seagreen" id="cmsControl"> \r\n ' +
					'<p class="cmsToolItem ui-widget-header cms-seagreen"> ' + cmsLastWidgetName + ' </p> \r\n ' +
					'<input type="hidden" id="cmsCtrlID" value="' + cmsLastWidget + '" /> \r\n ' +
					'<input type="hidden" id="cmsCtrlOrder" value="-1" /> \r\n ' +
					'</div> \r\n ' +
				 '</div>';

	var zone = $('#cms_' + cmsLastWidgetTarget);

	zone.prepend(widget);

	setTimeout("cmsBuildOrderAndUpdateWidgets();", 500);

	// reset once "dropped"
	cmsLastWidget = '';
	cmsLastWidgetTarget = '';
}

function cmsClickAddWidget() {
	if (cmsLastWidget.length > 1
			&& cmsLastWidgetTarget.length > 1) {
		$("#CMSaddconfirm").dialog({
			open: function () {
				$(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
			},
			closeOnEscape: true,
			resizable: false,
			height: 250,
			width: 400,
			modal: true,
			buttons: {
				"No": function () {
					$(this).dialog("close");
				},
				"Yes": function () {
					cmsCreateNewWidget();
					$(this).dialog("close");
				}
			}
		});

		cmsFixDialog('CMSaddconfirmmsg');
	}
}

function cmsDoStyleButtons(fltPrefix) {
	if ($(fltPrefix + ' .ui-button').length < 1) {
		$(fltPrefix).addClass('ui-button ui-widget ui-state-default ui-corner-all');

		$(fltPrefix).hover(function () {
			$(this).addClass("ui-state-hover");
		}, function () {
			$(this).removeClass("ui-state-hover");
		});
	}
}

function cmsFixDialog(dialogname) {
	var dilg = $("#" + dialogname).parent().parent();

	cmsOverrideCSSScope(dilg, "");

	$(".ui-widget-overlay").each(function (i) {
		$(this).wrap("<div class=\"cms-seagreen\" />");
		$(this).css('zIndex', 9950001);
	});

	var d = $(dilg);
	$(d).wrap("<div class=\"cms-seagreen\" />");
	$(d).css('zIndex', 9950005);

	//alert($(dilg).prop('id'));

	var dialogWrapper = 'cmsDlgWrap_' + dialogname;

	if ($(dilg).prop('id').length < 1) {
		$(dilg).prop('id', dialogWrapper);
	}

	dialogWrapper = '#' + dialogWrapper;

	//$(dilg).find('.ui-dialog-titlebar').addClass("cms-seagreen");
	//$(dilg).find('ui-dialog-title').addClass("cms-seagreen");

	if ($(dialogWrapper + " .ui-dialog-titlebar .ui-dialog-titlebar-close .ui-icon-closethick").length < 1) {
		var closeBtn = $(dialogWrapper + ' .ui-dialog-titlebar-close');
		closeBtn.addClass('ui-button ui-widget ui-state-default ui-corner-all ui-button-icon-only');
		closeBtn.append('<span class="ui-button-icon-primary ui-icon ui-icon-closethick"></span><span class="ui-button-text"> </span>');

		closeBtn.hover(function () {
			$(this).addClass("ui-state-hover");
		}, function () {
			$(this).removeClass("ui-state-hover");
		});
	}

	if ($(dialogWrapper + " .ui-dialog-buttonpane .ui-dialog-buttonset btn").length < 1) {
		var btns = $(dialogWrapper + ' .ui-dialog-buttonset button');
		btns.addClass('btn btn-default');
		var btn = $(dialogWrapper + ' .ui-dialog-buttonset button:first-child');
		btn.removeClass('btn-default').addClass('btn-primary ui-state-focus');
	}

	cmsStyleButtons();
}

function cmsOverrideCSSScope(elm, xtra) {
	var c = $(elm).attr('class');
	if (c.indexOf("cms-seagreen") < 0 || c.indexOf(xtra) < 0) {
		$(elm).attr('class', "cms-seagreen " + xtra + " " + c);
	}
	$(elm).addClass("cms-seagreen");
}

function cmsGenericEdit(PageId, WidgetId) {
	cmsLaunchWindow(adminUri + 'ControlPropertiesEdit?pageid=' + PageId + '&id=' + WidgetId);
}

function cmsLaunchWindow(theURL) {
	var TheURL = theURL;

	cmsSetiFrameSource(theURL);

	cmsSaveToolbarPosition();
	setTimeout("cmsLoadWindow();", 800);
}

function cmsLoadWindow() {
	cmsSaveToolbarPosition();

	$("#cms-basic-modal-content").simplemodal({
		onClose: function (dialog) {
			//$.simplemodal.close(); // must call this!
			setTimeout("$.simplemodal.close();", 800);
			cmsResetIframe();
			cmsDirtyPageRefresh();
		}
	});

	$('#cms-basic-modal-content').simplemodal();
	cmsStyleButtons();
	return false;
}

function cmsSetIframeRealSrc(theFrameID) {
	var theSRC = $('#' + theFrameID).attr('realsrc');
	$('#' + theFrameID).attr('src', theSRC);
}

function cmsSetiFrameSource(theURL) {
	var TheURL = theURL;

	$('#cmsModalFrame').html('<div id="cmsAjaxMainDiv2"> <iframe scrolling="auto" id="cmsFrameEditor" frameborder="0" name="cmsFrameEditor" width="96%" height="500" realsrc="' + TheURL + '" src="/Assets/Admin/includes/Blank.htm" /> </div>');

	setTimeout("cmsSetIframeRealSrc('cmsFrameEditor');", 750);

	$("#cmsAjaxMainDiv2").block({
		message: '<table><tr><td><img class="cmsAjaxModalSpinner" src="/Assets/Admin/images/Ring-64px-A7B2A0.gif"/></td></tr></table>',
		css: { width: '98%', height: '98%' },
		fadeOut: 1000,
		timeout: 1200,
		overlayCSS: { backgroundColor: '#FFFFFF', opacity: 0.6, border: '0px solid #000000' }
	});

	cmsStyleButtons();
}

function cmsLaunchWindowOnly(theURL) {
	var TheURL = theURL;

	cmsSetiFrameSource(theURL);

	cmsSaveToolbarPosition();
	setTimeout("cmsLoadWindowOnly();", 800);
}

function cmsLoadWindowOnly() {
	cmsSaveToolbarPosition();

	$("#cms-basic-modal-content").simplemodal({
		onClose: function (dialog) {
			//$.simplemodal.close(); // must call this!
			setTimeout("$.simplemodal.close();", 800);
			cmsResetIframe();
		}
	});

	$('#cms-basic-modal-content').simplemodal();
	cmsStyleButtons();
	return false;
}

function cmsResetIframe() {
	var frameSel = '#cmsModalFrame';

	$(frameSel).css('width', '98%');
	$(frameSel).attr('width', '98%');
	$(frameSel).css('height', '99%');
	$(frameSel).attr('height', '99%');
	$(frameSel).html('<div id="cmsAjaxMainDiv"></div>');
}

function cmsCloseModalWin() {
	cmsSaveToolbarPosition();
	cmsResetIframe();
	setTimeout("$.simplemodal.close();", 250);
}

function cmsDirtyPageRefresh() {
	cmsSaveToolbarPosition();
	cmsMakeOKToLeave();
	window.setTimeout('cmsMakeOKToLeave();', 500);
	window.setTimeout('cmsMakeOKToLeave();', 700);
	window.setTimeout("location.href = \'" + thisPageNav + "?carrotedit=true&carrottick=" + timeTick + "\'", 800);
}