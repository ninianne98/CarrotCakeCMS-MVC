function checkGridBoxes(gridID) {
	$('#' + gridID + ' input[type=checkbox]').each(function () {
		$(this).prop('checked', true);
	});
}

function uncheckGridBoxes(gridID) {
	$('#' + gridID + ' input[type=checkbox]').each(function () {
		$(this).prop('checked', false);
	});
}

$(document).ready(function () {
	// make the header fixed on scroll
	$('.datatable').addClass('table');
});