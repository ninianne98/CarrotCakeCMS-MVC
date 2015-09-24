var webSvc = cmsGetServiceAddress();

var menuValue = '';
var thisPage = '';

var menuOuter = 'menuitemsouter';
var menuInner = 'menuitemsinner';
var menuPath = 'menupath';

var bMoused = false;
var bLoad = true;

function InitDrillParms(currentid, fieldname) {
	thisPage = currentid;

	menuValue = fieldname;
	bLoad = true;
}

function AjaxLoadDrillMenu() {
	mouseNode();
	getCrumbs();

	setTimeout("hideMnu();", 250);

	$('#' + menuOuter).bind("mouseenter", function (e) {
		showMnu();
	});
	$('#' + menuOuter).bind("mouseleave", function (e) {
		hideMnu();
	});
}

function doesMenuExists() {
	if ($('#' + menuValue).length > 0) {
		return true;
	} else {
		return false;
	}
}

function getSelectedNodeValue() {
	var myVal = $('#' + menuValue).val();

	return myVal;
}

function getCrumbs() {
	if (doesMenuExists()) {
		var webMthd = webSvc + "/GetPageCrumbs";
		var myVal = getSelectedNodeValue();

		$.ajax({
			type: "POST",
			url: webMthd,
			data: JSON.stringify({ PageID: myVal, CurrPageID: thisPage }),
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			success: ajaxReturnCrumb,
			error: cmsAjaxFailed
		});
	}
}

function ajaxReturnCrumb(data, status) {
	var lstData = data.d;
	var val = '';
	var mnuName = '#' + menuPath;

	$(mnuName).text('');

	if (lstData.length > 0) {
		$.each(lstData, function (i, v) {
			var del = "<a href='javascript:void(0);' title='Remove' thevalue='" + val + "' onclick='selectDrillItem(this);'><div class='ui-icon ui-icon-closethick' style='float:left'></div></a>";
			if (i != (lstData.length - 1)) {
				del = '';
			}
			val = v.Root_ContentID;
			var bc = "<div class='pageNodeDrillDown2' thevalue='" + v.Root_ContentID + "' id='node' >" + v.NavMenuText + " </div>";
			$(mnuName).append("<div class='ui-widget-header ui-corner-all pageNodeDrillDown3' >" + bc + del + "<div  style='clear: both;'></div></div>");
		});
	}

	makeMenuClickable();
}

var bMoused = false;

function makeMenuClickable() {
	$('#PageContents a').each(function (i) {
		$(this).click(function () {
			cmsMakeOKToLeave();
			setTimeout("cmsMakeNotOKToLeave();", 500);
		});
	});
}

function hideMnu() {
	bHidden = true;
	$('#' + menuInner).attr('class', 'scroll scrollcontainerhide');
	$('#' + menuOuter).attr('class', 'scrollcontainer scrollcontainerheight');
}

var bHidden = true;
function showMnu() {
	if (bHidden == true) {
		$('#' + menuInner).attr('class', 'scroll');
		$('#' + menuOuter).attr('class', 'scrollcontainer ui-widget ui-widget-content ui-corner-all');
		bHidden = false;
	}
}

function mouseNode() {
	if (!bLoad) {
		showMnu();
	} else {
		hideMnu();
	}

	if (doesMenuExists()) {
		var webMthd = webSvc + "/GetChildPages";
		var myVal = $('#' + menuValue).val();

		if (bMoused != true) {
			hideMnu();

			$('#' + menuInner).html("<div style='width: 32px; height: 32px; margin: 0 auto;'><img src='/c3-admin/images/mini-spinner3-6F997D.gif' alt='spinner' /></div>");

			$.ajax({
				type: "POST",
				url: webMthd,
				data: JSON.stringify({ PageID: myVal, CurrPageID: thisPage }),
				contentType: "application/json; charset=utf-8",
				dataType: "json",
				success: ajaxReturnNode,
				error: cmsAjaxFailed
			});
		}
	}

	bLoad = false;
}

function ajaxReturnNode(data, status) {
	var lstData = data.d;
	var mnuName = '#' + menuInner;

	hideMnu();
	$(mnuName).html('');

	$.each(lstData, function (i, v) {
		$(mnuName).append("<div><a href='javascript:void(0);' onclick='selectDrillItem(this);' thevalue='" + v.Root_ContentID + "' id='node' >" + v.NavMenuText + "</a></div>");
	});

	if ($(mnuName).text().length < 2) {
		$(mnuName).append("<div><b>No Data</b></div>");
	}

	makeMenuClickable();

	bMoused = true;
}

function selectDrillItem(a) {
	bMoused = false;

	var tgt = $(a);
	var v = tgt.attr('thevalue');

	$('#' + menuValue).val(v);

	mouseNode();
	getCrumbs();
}

function resetDrill() {
	bMoused = false;
	bLoad = true;

	mouseNode();
	getCrumbs();

	setTimeout("hideMnu();", 250);
}