if (typeof jQuery === 'undefined') {
	throw new Error('Tiny config JavaScript requires jQuery')
}

var tinyBrowseHeight = 400;
var tinyBrowseWidth = 600;
var tinyBrowseResize = false;

function cmsTinyMceInit(winWidth, winHeight, allowResize) {
	tinyBrowseHeight = parseInt(winHeight);
	tinyBrowseWidth = parseInt(winWidth);
	if (tinyBrowseWidth < 500) {
		tinyBrowseWidth = 500;
	}
	if (tinyBrowseWidth > 960) {
		tinyBrowseWidth = 960;
	}
	if (tinyBrowseHeight < 100) {
		tinyBrowseHeight = 100;
	}

	tinyBrowseResize = allowResize;

	if (tinymce) {
		// because ajax...
		tinymce.remove();
	}

	var d = new Date();
	cmsTimeTick = d.getTime();
	var contentCss = "/assets/admin/includes/richedit.css?ts=" + cmsTimeTick;

	tinymce.init({
		selector: "textarea.mceEditor",
		file_picker_types: 'file image media',
		file_picker_callback: cmsTinyFileBrowserCallback,
		promotion: false,
		convert_unsafe_embeds: true,
		plugins: 'image link lists media charmap searchreplace visualblocks table preview code codesample help',
		toolbar1: 'savebutton | bold italic underline strikethrough sub sup | blocks forecolor backcolor | blockquote alignleft aligncenter alignright alignjustify outdent indent | help | ',
		toolbar2: 'undo redo searchreplace | bullist numlist | removeformat pastetext | link unlink anchor image media customfilebrowser | charmap codesample code preview visualblocks',
		removed_menuitems: 'newdocument help',
		codesample_languages: [
			{ text: 'HTML', value: 'markup' },
			{ text: 'Markdown', value: 'markdown' },
			{ text: 'Plain Text', value: 'plaintext' },
			{ text: 'XML', value: 'xml' },
			{ text: 'JSON', value: 'json' },
			{ text: 'Bash', value: 'bash' },
			{ text: 'Shell', value: 'shell' },
			{ text: 'Access log', value: 'accesslog' },
			{ text: 'JavaScript', value: 'javascript' },
			{ text: 'TypeScript', value: 'typescript' },
			{ text: 'CSS', value: 'css' },
			{ text: 'SQL', value: 'sql' },
			{ text: 'PHP', value: 'php' },
			{ text: 'Ruby', value: 'ruby' },
			{ text: 'Python', value: 'python' },
			{ text: 'PowerShell', value: 'powershell' },
			{ text: 'Java', value: 'java' },
			{ text: 'C', value: 'c' },
			{ text: 'C#', value: 'csharp' },
			{ text: 'VB', value: 'vbnet' },
			{ text: 'ASP', value: 'vbscript-html' },
			{ text: 'VBS', value: 'vbscript' },
			{ text: 'C++', value: 'cpp' }
		],
		resize: tinyBrowseResize,
		width: tinyBrowseWidth,
		height: tinyBrowseHeight,
		relative_urls: false,
		remove_script_host: true,
		extended_valid_elements: "style,link[href|rel]",
		custom_elements: "style,link,~link",
		setup: function (editor) {
			editor.ui.registry.addButton('customfilebrowser', {
				icon: 'document-properties',
				tooltip: 'File Browser',
				onAction: function (_) {
					cmsTinyFileBrowser('0');
				}
			});

			editor.ui.registry.addButton('savebutton', {
				icon: 'save',
				tooltip: "Save Document",
				onAction: function (_) {
					cmsTinyMceSave();
				}
			});
		},
		content_css: contentCss
	});

	setTimeout(function () {
		cmsTinyMceSaveHide();
	}, 200);

	setTimeout(function () {
		cmsTinyMceSaveHide();
	}, 800);
}

function cmsTinyMceSave() {
	alert("not implemented");
}

var cmsTinySpecialCss = 'tiny-save-ok';

function cmsTinyMceSaveHide() {
	var tinySave = $('.tox-toolbar-overlord').find('button[aria-label="Save Document"]');

	$(tinySave).each(function () {
		if (!$(this).parent().hasClass(cmsTinySpecialCss)) {
			$(this).parent().hide();
		}
	});
}

function cmsTinyMceSaveShow() {
	setTimeout(function () {
		__cmsTinyMceSaveShow();
	}, 600);
	setTimeout(function () {
		__cmsTinyMceSaveShow();
	}, 1200);
}

function __cmsTinyMceSaveShow() {
	var tinySave = $('.tox-toolbar-overlord').find('button[aria-label="Save Document"]');

	$(tinySave).each(function () {
		if (!$(this).parent().hasClass(cmsTinySpecialCss)) {
			$(this).parent().show();
			$(this).parent().addClass(cmsTinySpecialCss);
		}
	});
}

var lastMetaRequest = null;
var lastCallback = null;

function cmsTinyFileBrowserCallback(callback, value, meta) {
	lastMetaRequest = meta;
	lastCallback = callback;

	cmsTinyFileBrowser('1');
}

function cmsTinyFileBrowser(fld) {
	var sURL = cmsAdminUri + "FileBrowser?useTiny=1&returnvalue=" + fld + "&viewmode=file&fldrpath=/";

	tinymce.activeEditor.windowManager.openUrl({
		url: sURL,
		title: 'File Browser',
		resizable: "no",
		scrollbars: "yes",
		status: "yes",
		inline: "yes",
		close_previous: "yes"
	});

	return false;
}

function cmsFileBrowseClose() {
	tinymce.activeEditor.windowManager.close();
}

function cmsFileBrowseSetUri(uri, h, w) {
	if (lastCallback != null) {
		lastCallback(uri);

		lastCallback = null;
		lastMetaRequest = null;
	}

	cmsFileBrowseClose();
}

function cmsPreSaveTrigger() {
	if (tinymce) {
		var tgr = tinymce.triggerSave();
	}
	return true;
}

function cmsToggleTinyMCE(id) {
	if (!tinymce.get(id)) {
		$('#' + id).addClass("mceEditor");
		$('#' + id).removeClass("rawEditor");
		tinymce.execCommand('mceAddControl', false, id);
	} else {
		$('#' + id).addClass("rawEditor");
		$('#' + id).removeClass("mceEditor");
		tinymce.execCommand('mceFocus', false, id);
		tinymce.execCommand('mceRemoveControl', false, id);
	}
}

//==================================

var fldName = '';
var winBrowse = null;
function cmsFileBrowserOpen(fldN) {
	fldN = '#' + fldN;
	var fld = $(fldN);
	fldName = fld.attr('id');

	if (winBrowse != null) {
		winBrowse.close();
	}

	ShowWindowNoRefresh(cmsAdminUri + 'FileBrowser?useTiny=0&viewmode=file&fldrpath=/');

	return false;
}

function cmsSetFileName(v) {
	var fldN = '#' + fldName;
	var fld = $(fldN);
	fld.val(v);

	winBrowse.close();
	winBrowse = null;
}

function __cmsSynchTinyWidths() {
	$('textarea.mceEditor').each(function () {
		var id = $(this).attr('id');
		var wTxt = $(this).css("width");
		var w = parseInt(wTxt || '0');
		//console.log("tiny-text-id:  " + id);

		if ($(this).hasClass('tiny-resized') == false) {
			//console.log("tiny-text-wt:  " + wTxt);
			//console.log("tiny-text-w:  " + w);

			if (tinymce && w > 0) {
				var tinyInst = tinymce.get(id);
				if (tinyInst) {
					var tinyFrame = $('#' + id + '_ifr');
					if (tinyFrame) {
						$(this).addClass('tiny-resized');
						var tc = $(tinyFrame).closest('.tox-tinymce');
						//var tw = parseInt($(tc).css("width") || '0');
						//console.log("tiny-ed-w:  " + tw);

						$(tc).css("width", wTxt);
					}
				}
			}
		}
	});
}

function cmsSynchTinyWidthsInit() {
	setTimeout(function () {
		__cmsSynchTinyWidths();
	}, 500);
	setTimeout(function () {
		__cmsSynchTinyWidths();
	}, 1500);
}


function __cmsStripTinyWidths() {
	$('.tox-tinymce').css("width", '');
}

function cmsStripTinyWidthsInit() {
	setTimeout(function () {
		__cmsStripTinyWidths();
	}, 500);
	setTimeout(function () {
		__cmsStripTinyWidths();
	}, 800);
	setTimeout(function () {
		__cmsStripTinyWidths();
	}, 1200);
}

//===================

var cmsAdminUri = cmsAdminBasePath;  //  "/c3-admin/";
var cmsWebSvc = cmsWebServiceApi;