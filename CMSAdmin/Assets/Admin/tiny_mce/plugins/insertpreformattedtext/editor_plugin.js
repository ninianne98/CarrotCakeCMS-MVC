
(function () {
	tinymce.PluginManager.requireLangPack('insertpreformattedtext');

	tinymce.create('tinymce.plugins.InsertPreformatText', {

		init: function (ed, url) {
			ed.addCommand('mceInsertPreformatText', function () {
				ed.windowManager.open({
					file: url + '/dialog.htm',
					width: parseInt(ed.getParam("theme_advanced_source_editor_width", 720)),
					height: parseInt(ed.getParam("theme_advanced_source_editor_height", 580)),
					inline: true,
					resizable: true,
					maximizable: true
				}, {
					theme_url: this.url
				});
			});

			ed.addButton('insertpreformattedtext', {
				title: 'Insert Preformatted Text ',
				cmd: 'mceInsertPreformatText',
				image: url + '/img/insertpreformattedtext.gif'
			});

			ed.onNodeChange.add(function (ed, cm, n) {
				cm.setActive('insertpreformattedtext', n.nodeName == 'IMG');
			});
		},

		createControl: function (n, cm) {
			return null;
		},

		getInfo: function () {
			return {
				longname: 'Insert Source Code',
				author: 'CarrotWare',
				authorurl: 'http://www.carrotware.com',
				infourl: 'http://www.carrotware.com/carrotcake-cms.aspx',
				version: "0.2"
			};
		}
	});

	tinymce.PluginManager.add('insertpreformattedtext', tinymce.plugins.InsertPreformatText);
})();