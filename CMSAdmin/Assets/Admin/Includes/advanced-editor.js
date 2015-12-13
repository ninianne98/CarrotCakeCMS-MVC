if (typeof jQuery === 'undefined') {
	throw new Error('Advanced Editor JavaScript requires jQuery')
}

var cmsIsPageLocked = true;

function cmsSetPageStatus(stat) {
	cmsIsPageLocked = stat;
}

var webSvc = "/Assets/Admin/CMS.asmx";
var thisPage = ""; // used in escaped fashion
var thisPageNav = "";  // used non-escaped (redirects)
var thisPageNavSaved = "";  // used non-escaped (redirects)
var thisPageID = "";
var timeTick = 9999;

function cmsSetServiceParms(serviceURL, pagePath, pageID) {
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

setTimeout("cmsResetToolbarScroll()", 500);

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
}

function cmsMenuFixImages() {
	$(".cmsWidgetBarIconCog").each(function (i) {
		cmsFixGeneralImage(this, 'cog.png');
	});
	$(".cmsWidgetBarIconCross").each(function (i) {
		cmsFixGeneralImage(this, 'cross.png');
	});
	$(".cmsWidgetBarIconDup").each(function (i) {
		cmsFixGeneralImage(this, 'shape_ungroup.png');
	});
	$(".cmsWidgetBarIconTime").each(function (i) {
		cmsFixGeneralImage(this, 'clock_edit.png');
	});
	$(".cmsWidgetBarIconCopy").each(function (i) {
		cmsFixGeneralImage(this, 'table_go.png');
	});
	$(".cmsWidgetBarIconPencil").each(function (i) {
		cmsFixGeneralImage(this, 'pencil.png');
	});
	$(".cmsWidgetBarIconPencil2").each(function (i) {
		cmsFixGeneralImage(this, 'pencil.png');
	});
	$(".cmsWidgetBarIconActive").each(function (i) {
		cmsFixGeneralImage(this, 'tick.png');
	});
	$(".cmsWidgetBarIconWidget").each(function (i) {
		cmsFixGeneralImage(this, 'application_view_tile.png');
	});
	$(".cmsWidgetBarIconWidget2").each(function (i) {
		cmsFixGeneralImage(this, 'hourglass.png');
	});
	//$(".cmsWidgetBarIconShrink").each(function (i) {
	//cmsFixGeneralImage(this, 'Shrink', 'Shrink', 'arrow_in.png');
	//});
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
	var tabID = $('#cmsJQTabedToolbox').tabs("option", "active");

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

	window.frames["cmsFrameEditorPreview"].location.reload();
}

var cmsTemplateListPreviewer = "#cmsTemplateList"

function cmsWideMobile() {
	cmsWidePreview('475px');
}

function cmWideTablet() {
	cmsWidePreview('760px');
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
	var editIFrame = $('#cmsFrameEditor');

	var frmHgt = 440;
	var hgt = ($(window).height() * 0.75) - 95;

	if (hgt > frmHgt) {
		frmHgt = hgt;
	}

	$(editIFrame).attr('width', '100%');
	$(editIFrame).attr('height', frmHgt);

	var templateList = '';

	$(cmsTemplateDDL + " > option").each(function () {
		templateList += "<option value='" + this.value + "'>" + this.text + "</option>";
	});

	var btnWide1 = '<input type="radio" id="btnDeskTemplateCMS" name="btnWidthTemplateCMS" value="0" onclick="cmsWideDesk();" checked /><label for="btnDeskTemplateCMS">Desktop Size</label>';
	var btnWide2 = '<input type="radio" id="btnTabletTemplateCMS" name="btnWidthTemplateCMS" value="1" onclick="cmWideTablet();" /><label for="btnTabletTemplateCMS">Tablet Size</label>';
	var btnWide3 = '<input type="radio" id="btnMobileTemplateCMS" name="btnWidthTemplateCMS" value="2" onclick="cmsWideMobile();" /><label for="btnMobileTemplateCMS">Mobile Size</label>';

	var ddlPreview = ' <select id="cmsTemplateList">' + templateList + '</select>  <input type="button" value="Preview" id="btnPreviewCMS" onclick="cmsPreviewTemplate2();" /> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ';

	var btnClose = ' <input type="button" id="btnCloseTemplateCMS" value="Close" onclick="cmsCloseModalWin();" /> &nbsp;&nbsp;&nbsp; ';
	var btnApply = ' <input type="button" id="btnApplyTemplateCMS" value="Apply Template" onclick="cmsUpdateTemplate();" /> &nbsp;&nbsp;&nbsp; ';

	$(editFrame).append('<div id="cmsGlossySeaGreenID"><div id="cmsPreviewControls" class="cmsGlossySeaGreen cmsPreviewButtons"> ' + '<span class="jqradioset">' + btnWide1 + btnWide2 + btnWide3 + '</span> &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ' + ddlPreview + btnClose + btnApply + ' </div></div>');
	window.setTimeout("cmsLateBtnStyle();", 500);

	$("#cmsPreviewControls .jqradioset").buttonset();

	//	var list = $(cmsTemplateListPreviewer);
	//	$(cmsTemplateDDL + " > option").each(function () {
	//		list.append(new Option(this.text, this.value));
	//	});

	$(cmsTemplateListPreviewer).val(tmplReal);

	$('#cmsFrameEditor').attr('id', 'cmsFrameEditorPreview');
	$('#cmsAjaxMainDiv2').attr('id', 'cmsAjaxMainDiv3');

	setTimeout("cmsSetIframeRealSrc('cmsFrameEditorPreview');", 1000);
	cmsStyleButtons();
	cmsWideDesk();
}

function cmsLateBtnStyle() {
	$(".cmsGlossySeaGreen input:button, .cmsGlossySeaGreen input:submit, .cmsGlossySeaGreen input:reset").button();
	$("#cmsGlossySeaGreenID input:button, #cmsGlossySeaGreenID input:submit, #cmsGlossySeaGreenID input:reset").button();
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
	//if (data.d == "OK") {
	//	cmsSpinnerShort();
	//} else {
	//	cmsAlertModal(data.d);
	//}

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
		//window.setTimeout("location.href = \'" + thisPageNav + "\'", 10000);
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
		open: function () {
			$(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
		},

		resizable: false,
		height: 250,
		width: 400,
		modal: true,
		buttons: {
			"OK": function () {
				cmsMakeOKToLeave();
				window.setTimeout("location.href = \'" + thisPageNavSaved + "\'", 250);
				$(this).dialog("close");
			}
		}
	});

	cmsFixDialog('CMSsavedconfirmmsg');
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
		open: function () {
			$(this).parents('.ui-dialog-buttonpane button:eq(0)').focus();
		},

		resizable: false,
		height: 250,
		width: 400,
		modal: true,
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
	cmsLaunchWindow('/c3-admin/WidgetList/' + thisPageID + "?zone=cms-all-placeholder-zones");
}
function cmsManageWidgetList(zoneName) {
	//alert(zoneName);
	cmsLaunchWindow('/c3-admin/WidgetList/' + thisPageID + "?zone=" + zoneName);
}
function cmsManageWidgetHistory(widgetID) {
	//alert(widgetID);
	cmsLaunchWindow('/c3-admin/WidgetHistory/' + thisPageID + "?widgetid=" + widgetID);
}
function cmsManageWidgetTime(widgetID) {
	//alert(widgetID);
	cmsLaunchWindow('/c3-admin/WidgetTime/' + thisPageID + "?widgetid=" + widgetID);
}

function cmsShowEditPageInfo() {
	cmsLaunchWindow('/c3-admin/PageEdit/' + thisPageID);
}

function cmsShowEditPostInfo() {
	cmsLaunchWindow('/c3-admin/BlogPostEdit/' + thisPageID);
}
function cmsShowAddPage() {
	cmsLaunchWindow('/c3-admin/PageAddChild/' + thisPageID);
}
function cmsShowAddChildPage() {
	cmsLaunchWindow('/c3-admin/PageAddChild/' + thisPageID);
}
function cmsShowAddTopPage() {
	cmsLaunchWindow('/c3-admin/PageAddChild/' + thisPageID + '?addtoplevel=true');
}
function cmsEditSiteMap() {
	cmsLaunchWindow('/c3-admin/SiteMapPop');
}

function cmsSortChildren() {
	//cmsAlertModal("cmsSortChildren");
	cmsLaunchWindowOnly('/c3-admin/PageChildSort/' + thisPageID);
}

function cmsShowEditWidgetForm(w, m) {
	//cmsAlertModal("cmsShowEditWidgetForm");
	cmsLaunchWindow('/c3-admin/ContentEdit/' + thisPageID + '?widgetid=' + w + '&mode=' + m);
}

function cmsShowEditContentForm(f, m) {
	//cmsAlertModal("cmsShowEditContentForm");
	cmsLaunchWindow('/c3-admin/ContentEdit/' + thisPageID + '?field=' + f + '&mode=' + m);
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
	cmsLaunchWindow('/c3-admin/DuplicateWidgetFrom/' + thisPageID + "?zone=" + zoneName);
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
		$('#cmsJQTabedToolbox').tabs();

		$('#cmsJQTabedToolbox').tabs("option", "active", cmsToolTabIdx);

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
	cmsDoStyleButtons('.cmsGlossySeaGreen input[type="button"]');
	cmsDoStyleButtons('.cmsGlossySeaGreen input[type="submit"]');
	cmsDoStyleButtons('.cmsGlossySeaGreen button');
	cmsDoStyleButtons('.ui-dialog-buttonpane button');
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
		$(this).wrap("<div class=\"cmsGlossySeaGreen\" />");
		$(this).css('zIndex', 9950001);
	});

	var d = $(dilg);
	$(d).wrap("<div class=\"cmsGlossySeaGreen\" />");
	$(d).css('zIndex', 9950005);

	//alert($(dilg).prop('id'));

	var dialogWrapper = 'cmsDlgWrap_' + dialogname;

	if ($(dilg).prop('id').length < 1) {
		$(dilg).prop('id', dialogWrapper);
	}

	dialogWrapper = '#' + dialogWrapper;

	//$(dilg).find('.ui-dialog-titlebar').addClass("cmsGlossySeaGreen");
	//$(dilg).find('ui-dialog-title').addClass("cmsGlossySeaGreen");

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
	if (c.indexOf("cmsGlossySeaGreen") < 0 || c.indexOf(xtra) < 0) {
		$(elm).attr('class', "cmsGlossySeaGreen " + xtra + " " + c);
	}
	$(elm).addClass("cmsGlossySeaGreen");
}

function cmsGenericEdit(PageId, WidgetId) {
	cmsLaunchWindow('/c3-admin/ControlPropertiesEdit?pageid=' + PageId + '&id=' + WidgetId);
}

function cmsLaunchWindow(theURL) {
	var TheURL = theURL;

	cmsSetiFrameSource(theURL);

	cmsSaveToolbarPosition();
	setTimeout("cmsLoadWindow();", 800);
}

function cmsLoadWindow() {
	cmsSaveToolbarPosition();

	$("#cms-basic-modal-content").modal({
		onClose: function (dialog) {
			//$.modal.close(); // must call this!
			setTimeout("$.modal.close();", 800);
			$('#cmsModalFrame').html('<div id="cmsAjaxMainDiv"></div>');
			cmsDirtyPageRefresh();
		}
	});

	$('#cms-basic-modal-content').modal();
	cmsStyleButtons();
	return false;
}

function cmsSetIframeRealSrc(theFrameID) {
	var theSRC = $('#' + theFrameID).attr('realsrc');
	$('#' + theFrameID).attr('src', theSRC);
}

function cmsSetiFrameSource(theURL) {
	var TheURL = theURL;

	$('#cmsModalFrame').html('<div id="cmsAjaxMainDiv2"> <iframe scrolling="auto" id="cmsFrameEditor" frameborder="0" name="cmsFrameEditor" width="90%" height="500" realsrc="' + TheURL + '" src="/Assets/Admin/includes/Blank.htm" /> </div>');

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

	$("#cms-basic-modal-content").modal({
		onClose: function (dialog) {
			//$.modal.close(); // must call this!
			setTimeout("$.modal.close();", 800);
			$('#cmsModalFrame').html('<div id="cmsAjaxMainDiv"></div>');
		}
	});

	$('#cms-basic-modal-content').modal();
	cmsStyleButtons();
	return false;
}

function cmsCloseModalWin() {
	cmsSaveToolbarPosition();
	setTimeout("$.modal.close();", 250);
	$('#cmsModalFrame').html('<div id="cmsAjaxMainDiv"></div>');
}

function cmsDirtyPageRefresh() {
	cmsSaveToolbarPosition();
	cmsMakeOKToLeave();
	window.setTimeout('cmsMakeOKToLeave();', 500);
	window.setTimeout('cmsMakeOKToLeave();', 700);
	window.setTimeout("location.href = \'" + thisPageNav + "?carrotedit=true&carrottick=" + timeTick + "\'", 800);
}