/*
Imagination by TEMPLATED
templated.co @templatedco
Released for free under the Creative Commons Attribution 3.0 license (templated.co/license)
*/

(function ($) {
	$.fn.setSkelJsPath = function (cssPath) {
		if ($('#skel-panels-pageWrapper').length < 1) {
			skel.init({
				prefix: cssPath + 'style',
				resetCSS: true,
				boxModel: 'border',
				grid: {
					gutters: 50
				},
				breakpoints: {
					'mobile': {
						range: '-480',
						lockViewport: true,
						containers: 'fluid',
						grid: {
							collapse: true,
							gutters: 10
						}
					},
					'desktop': {
						range: '481-',
						containers: 1200
					},
					'1000px': {
						range: '481-1200',
						containers: 960
					}
				}
			}, {
				panels: {
					panels: {
						navPanel: {
							breakpoints: 'mobile',
							position: 'left',
							style: 'reveal',
							size: '80%',
							html: '<div data-action="navList" data-args="nav"></div>'
						}
					},
					overlays: {
						titleBar: {
							breakpoints: 'mobile',
							position: 'top-left',
							height: 44,
							width: '100%',
							html: '<span class="toggle" data-action="togglePanel" data-args="navPanel"></span>' +
 '<span class="title" data-action="copyHTML" data-args="logo"></span>'
						}
					}
				}
			});
		}
	}
})(jQuery);

(function ($) {
	var skelCssPath = '';

	$.fn.loadSkel = function () {
		try {
			$(this).setSkelJsPath(skelCssPath);
		} catch (err) { }
	}

	$.fn.detectTemplatePath = function () {
		var scriptEls = document.getElementsByTagName('script');
		var thisScriptEl = scriptEls[scriptEls.length - 1];
		var scriptPath = thisScriptEl.src;

		var scriptFolder = scriptPath.substr(0, scriptPath.lastIndexOf('/'));
		var rootFolder = scriptFolder.substr(0, scriptFolder.lastIndexOf('/') + 1);

		if (skelCssPath.length < 1) {
			skelCssPath = rootFolder + 'css/'
		}

		//alert(skelCssPath);
	}
})(jQuery);

$(document).detectTemplatePath();
$(document).loadSkel();

$(document).ready(function () {
	//$(document).loadSkel();

	setTimeout("$(document).loadSkel();", 100);
});

setTimeout("$(document).loadSkel();", 1500);