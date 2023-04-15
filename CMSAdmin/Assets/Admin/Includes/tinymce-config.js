var tinyBrowseHeight = 300;
var tinyBrowseWidth = 500;
var tinyBrowseResize = false;

function TinyMCEParamInit(winWidth, winHeight, allowResize) {
	tinyBrowseHeight = parseInt(winHeight);
	tinyBrowseWidth = parseInt(winWidth);
	tinyBrowseResize = allowResize;

	tinyMCE.init({
		//		mode : "textareas",
		//		theme : "advanced",
		//		theme_advanced_toolbar_location : "top",
		//		theme_advanced_toolbar_align : "left",
		//		document_base_url : "http://www.site.com/path1/"

		mode: "textareas",
		theme: "advanced",
		editor_selector: "mceEditor",
		skin: "o2k7",
		skin_variant: "silver",
		plugins: "advimage,advlink,advlist,media,inlinepopups,searchreplace,visualblocks,paste,table,preview,insertpreformattedtext",
		file_browser_callback: "cmsFileBrowserCallback",
		theme_advanced_buttons1: "bold,italic,underline,strikethrough,sub,sup,|,justifyleft,justifycenter,justifyright,justifyfull,|,formatselect,forecolor,backcolor,|,outdent,indent,blockquote,|,bullist,numlist,|,fileupbtn,cleanup,removeformat,help",
		theme_advanced_buttons2: "search,replace,|,undo,redo,|,tablecontrols,|,pastetext,pasteword,|,link,unlink,anchor,image,media,|,insertpreformattedtext,code,preview,visualblocks",
		theme_advanced_buttons3: "",
		theme_advanced_toolbar_location: "top",
		theme_advanced_toolbar_align: "left",
		theme_advanced_statusbar_location: "bottom",
		//theme_advanced_resize_horizontal: true,
		theme_advanced_resizing: tinyBrowseResize,
		theme_advanced_source_editor_width: tinyBrowseWidth,
		theme_advanced_source_editor_height: tinyBrowseHeight,
		flash_video_player_url: false,
		relative_urls: false,
		remove_script_host: true,
		content_css: "/Assets/Admin/includes/richedit.css",

		// Add a custom button
		setup: function (ed) {
			ed.addButton('fileupbtn', {
				title: 'File Upload',
				image: '/Assets/tiny_mce/insertfile.gif',
				onclick: function () {
					ed.focus();
					var x = cmsFileBrowserCallback(ed, '', '', this);
				}
			});
		}
	});
}

// http://wiki.moxiecode.com/index.php/TinyMCE:Custom_filebrowser
function cmsFileBrowserCallback(field_name, url, type, win) {
	var sURL = adminUri + "FileBrowser?useTiny=1&viewmode=file&fldrpath=/";
	setTimeout("tinyResetFileBrowserOpenStatus();", 500);

	// block multiple file browser windows
	if (!tinyMCE.selectedInstance.fileBrowserAlreadyOpen) {
		tinyMCE.selectedInstance.fileBrowserAlreadyOpen = true; // but now it will be

		tinyMCE.activeEditor.windowManager.open({
			file: sURL,
			title: 'File Browser',
			width: tinyBrowseWidth,
			height: tinyBrowseHeight,
			resizable: "no",
			scrollbars: "yes",
			status: "yes",
			inline: "yes",
			close_previous: "yes"
		}, {
			window: win,
			input: field_name
		});
	}

	setTimeout("tinyResetFileBrowserOpenStatus();", 1000);

	return false;
}

function tinyResetFileBrowserOpenStatus() {
	tinyMCE.selectedInstance.fileBrowserAlreadyOpen = false;
}

function cmsPreSaveTrigger() {
	var tgr = tinyMCE.triggerSave();
	return true;
}

function cmsToggleTinyMCE(id) {
	if (!tinyMCE.get(id)) {
		$('#' + id).addClass("mceEditor");
		$('#' + id).removeClass("rawEditor");
		tinyMCE.execCommand('mceAddControl', false, id);
	} else {
		$('#' + id).addClass("rawEditor");
		$('#' + id).removeClass("mceEditor");
		tinyMCE.execCommand('mceFocus', false, id);
		tinyMCE.execCommand('mceRemoveControl', false, id);
	}
}

var fldName = '';
var winBrowse = null;
function cmsFileBrowserOpen(fldN) {
	fldN = '#' + fldN;
	var fld = $(fldN);
	fldName = fld.attr('id');

	if (winBrowse != null) {
		winBrowse.close();
	}
	//winBrowse = window.open(adminUri + 'FileBrowser?useTiny=0&viewmode=file&fldrpath=/', '_winBrowse', 'resizable=yes,location=no,menubar=no,scrollbars=yes,status=yes,toolbar=no,fullscreen=no,dependent=yes,width=650,height=500,left=50,top=50');

	ShowWindowNoRefresh(adminUri + 'FileBrowser?useTiny=0&viewmode=file&fldrpath=/');

	return false;
}

function cmsSetFileName(v) {
	var fldN = '#' + fldName;
	var fld = $(fldN);
	fld.val(v);

	winBrowse.close();
	winBrowse = null;
}

var adminUri = "/c3-admin/";

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