/*

jQuery Ajax Tooltip

http://www.ajaxprojects.com/ajax/tutorialdetails.php?itemid=718
http://rndnext.blogspot.com/2009/02/jquery-ajax-tooltip.html

*/

function cmsSetHTMLMessage(d) {
	$('#dataPopupContent').html(d);
}
function cmsSetTextMessage(d) {
	$('#dataPopupContent').text(d);
}

$(function () {
	var hideDelay = 2000;
	var hideTimer = null;

	// One instance that's reused to show info for the current data
	var container = $('<div id="dataPopupContainer">'
                    + '<table width="" border="0" cellspacing="0" cellpadding="0" align="center" class="dataPopupPopup">'
                    + '<tr>'
                    + '   <td class="corner topLeft"></td>'
                    + '   <td class="top"> </td>'
                    + '   <td class="corner topRight"></td>'
                    + '</tr>'
                    + '<tr>'
                    + '   <td class="left">&nbsp;</td>'
                    + '   <td class="center"><div id="dataPopupContent"></div></td>'
                    + '   <td class="right">&nbsp;</td>'
                    + '</tr>'
                    + '<tr>'
                    + '   <td class="corner bottomLeft">&nbsp;</td>'
                    + '   <td class="bottom">&nbsp;</td>'
                    + '   <td class="corner bottomRight"></td>'
                    + '</tr>'
                    + '</table>'
                    + '</div>');

	$('body').append(container);

	$(document).on('mouseover', '.dataPopupTrigger', function () {
		var dataId = $(this).attr('rel');

		if (hideTimer)
			clearTimeout(hideTimer);

		var pos = $(this).offset();
		var width = $(this).width();
		container.css({
			left: (pos.left + width) + 'px',
			top: pos.top + 20 + 'px'
		});

		container.css('display', 'block');

		cmsDoToolTipDataRequest(dataId);
	});

	$(document).on('mouseout', '.dataPopupTrigger', function () {
		if (hideTimer)
			clearTimeout(hideTimer);
		hideTimer = setTimeout(function () {
			container.css('display', 'none');
		}, hideDelay);
	});

	// Allow mouse over of details without hiding details
	$('#dataPopupContainer').mouseover(function () {
		if (hideTimer)
			clearTimeout(hideTimer);
	});

	// Hide after mouseout
	$('#dataPopupContainer').mouseout(function () {
		if (hideTimer)
			clearTimeout(hideTimer);
		hideTimer = setTimeout(function () {
			container.css('display', 'none');
		}, hideDelay);
	});
});