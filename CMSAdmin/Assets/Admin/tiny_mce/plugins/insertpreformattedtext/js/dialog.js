tinyMCEPopup.requireLangPack();

var InsertPreformatTextDialog = {
	init: function () {

		var src = tinyMCEPopup.editor.selection.getContent({ format: 'text' });

		if (src.trim().length > 0) {
			document.forms[0].txtSource.value = src.trim();
		}
	},

	insert: function () {
		var txt = document.forms[0].txtSource.value.trim();
		var lang = document.forms[0].ddlSoftwareLang.value;

		txt = txt.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

		var preBrush = '<pre>' + txt + '</pre>';

		if (lang != "-none-") {
			preBrush = '<pre class="brush: ' + lang + ';">' + txt + '</pre>';
		}

		tinyMCEPopup.editor.execCommand('mceInsertContent', false, preBrush);
		tinyMCEPopup.close();
	}
};

String.prototype.trim = function () {
	return this.replace(/^\s*/, "").replace(/\s*$/, "");
}

tinyMCEPopup.onInit.add(InsertPreformatTextDialog.init, InsertPreformatTextDialog);
