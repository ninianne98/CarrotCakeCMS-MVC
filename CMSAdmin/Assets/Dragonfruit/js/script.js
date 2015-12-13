/*

Dragonfruit Template
http://www.templatemo.com/free-website-templates/411-dragonfruit
http://www.templatemo.com/about
http://creativecommons.org/licenses/by/4.0/

*/

$(document).ready(function () {
	$(window).load(function () {
		$('.external-link').unbind('click');
	});
	//main menu
	$("#navigate_links").singlePageNav();

	//banner slide
	$('.banner').unslider({ fluid: true, speed: 600, delay: 2000 });
	//Babber slider on scroll
	$(window).on("load scroll resize", function () {
		banner_height = ($(document).width() / 1920) * 760;
		$('.banner').height(banner_height);
		$('.banner ul li').height(banner_height);
		if (banner_height > 250) {
			caption_margin_top = (banner_height - 100) / 2;
			$('.banner .slide_caption:hidden').show();
			$('.banner .slide_caption').css({ "margin-top": caption_margin_top });
		} else {
			$('.banner .slide_caption').hide();
		}
	});

	//mobile menu and desktop menu
	$("#dragonfruit_mobile_menu").css({ "right": -1500 });
	$("#mobile_menu").click(function () {
		$("#dragonfruit_mobile_menu").show();
		$("#dragonfruit_mobile_menu").animate({ "right": 0 });
		return false;
	});
	$(window).on("load resize", function () {
		if ($(window).width() > 768) {
			$("#dragonfruit_mobile_menu").css({ "right": -1500 });
		}
	});

	$.fn.anchorAnimate = function (settings) {
		settings = jQuery.extend({
			speed: 1100
		}, settings);
		return this.each(function () {
			var caller = this
			$(caller).click(function (event) {
				event.preventDefault();
				var locationHref = window.location.href;
				var elementClick = $(caller).attr("href");
				var destination = $(elementClick).offset().top - $('#dragonfruit_banner_menu').outerHeight();
				$("#dragonfruit_mobile_menu").animate({ "right": -1500 });
				$("#dragonfruit_mobile_menu").fadeOut();
				$("html,body").css({ "overflow": "auto" });
				$("html,body").stop().animate({ scrollTop: destination }, settings.speed, function () {
					// Detect if pushState is available
					if (history.pushState) {
						history.pushState(null, null, elementClick);
					}
				});
				return false;
			});
		});
	}
	//animate scroll function calll
	$("#dragonfruit_mobile_menu a").anchorAnimate();

	//about icon
	$(window).on("load scroll resize", function () {
		var about_wap_width = $(".about_icon").width();
		var about_icon_padding_left = (about_wap_width / 100) * 30;
		var about_icon_width = (about_wap_width / 100) * 40;
		var about_icon_size = (about_icon_width / 100) * 50;
		var about_icon_padding_top = (about_icon_width / 100) * 25;
		$(".about_icon .imgwap").css({
			'margin-left': about_icon_padding_left,
			'width': about_icon_width,
			'height': about_icon_width,
		});
		$(".about_icon .imgwap i").css({
			"font-size": about_icon_size,
			"padding-top": about_icon_padding_top,
		});
		$(".about_icon p").css({
			'padding-left': "10%",
			'padding-right': "10%",
		});
	});

	//about testimonial
	$.current_testimonial = $("div.testimonial_text").first();
	$("div.testimonial_text").hide();
	$.current_testimonial.show();
	$(window).on("load scroll resize", function () {
		testimonial_child_height = $(".testimonial_text").height();
		$("#testimonial_text_wap").height(testimonial_child_height);
		$(".pre_next_wap").height(testimonial_child_height);
	});
	$("#prev_testimonial").click(function () {
		$.current_testimonial.effect("fade", {}, 200, function () {
			$.current_testimonial_prev = ($.current_testimonial.index() == 0) ? $(".testimonial_text").last() : $.current_testimonial.prev();
			$.current_testimonial_prev.fadeIn();
			$.current_testimonial = $.current_testimonial_prev;
		});
		return false;
	});
	$("#next_testimonial").click(function () {
		$.current_testimonial.effect("fade", {}, 200, function () {
			$.current_testimonial_next = ($.current_testimonial.index() == $(".testimonial_text").last().index()) ? $(".testimonial_text").first() : $.current_testimonial.next();
			$.current_testimonial_next.fadeIn();
			$.current_testimonial = $.current_testimonial_next;
		});
		return false;
	});

	//event
	$(".event_box_img").load(function () {
		img_height = $(this).height();
		$(this).parent(".event_box_wap").height(img_height);
	});
	$(window).on("load resize", function () {
		img_height = $(".event_box_img").height();
		if ($(window).width() > 550) {
			$(".event_box_wap").height(img_height);
			$(".event_box_wap").each(function () {
				total_height = $(this).children(".event_box_caption").outerHeight();
				header_height = $(this).children(".event_box_caption").children("h1").outerHeight();
				admin_height = $(this).children(".event_box_caption").children("p").eq(0).outerHeight();
				paragraph_height = $(this).children(".event_box_caption").children("p").eq(1).outerHeight();
				hide_paragraph_height = header_height + admin_height + 10;
				$(this).children(".event_box_caption").css({ "top": "-" + hide_paragraph_height + "px" });
			});
		} else {
			$(".event_box_wap").height(img_height);
			$(".event_box_wap").each(function () {
				total_height = $(this).children(".event_box_caption").outerHeight();
				header_height = $(this).children(".event_box_caption").children("h1").outerHeight();
				admin_height = $(this).children(".event_box_caption").children("p").eq(0).outerHeight();
				paragraph_height = $(this).children(".event_box_caption").children("p").eq(1).outerHeight();
				hide_paragraph_height = header_height + admin_height + 10;
				$(this).height(total_height + img_height);
				$(this).children(".event_box_caption").css({ "top": "0px" });
			});
		}
	});
	$(".event_box_wap").hover(function () {
		if ($(window).width() > 550) {
			total_height = $(this).children(".event_box_caption").outerHeight();
			header_height = $(this).children(".event_box_caption").children("h1").outerHeight();
			admin_height = $(this).children(".event_box_caption").children("p").eq(0).outerHeight();
			paragraph_height = $(this).children(".event_box_caption").children("p").eq(1).outerHeight();
			hide_paragraph_height = header_height + admin_height + paragraph_height + 10;
			$(this).children(".event_box_caption").stop().animate({ "top": "-" + hide_paragraph_height + "px" });
		}
	}, function () {
		if ($(window).width() > 550) {
			total_height = $(this).children(".event_box_caption").outerHeight();
			header_height = $(this).children(".event_box_caption").children("h1").outerHeight();
			admin_height = $(this).children(".event_box_caption").children("p").eq(0).outerHeight();
			paragraph_height = $(this).children(".event_box_caption").children("p").eq(1).outerHeight();
			hide_paragraph_height = header_height + admin_height + 10;
			$(this).children(".event_box_caption").stop().animate({ "top": "-" + hide_paragraph_height + "px" });
		}
	});

	if ($("#dragonfruit_timeline").length > 0) {
		var initHeightL = 10; //$('#dragonfruit_banner_menu').outerHeight() + 25;
		var initHeightR = initHeightL + 45;

		//timeline
		$(window).on("load resize", function () {
			$.timeline_right_position_top = initHeightR;
			$.timeline_old_right_position_top = 0;
			$.timeline_left_position_top = initHeightL;
			$.timeline_old_left_position_top = 0;
			var w_width = ($(window).width() > 1600) ? 1600 : $(window).width();
			$.timeline_item_width = (w_width - 50) / 2;
			$(".time_line_wap").each(function () {
				//if class name already exit remove
				$(this).children("span.left_timer").remove();
				$(this).children("span.right_timer").remove();
				$(this).removeClass("left_timeline");
				$(this).removeClass("right_timeline");
				if ($(window).width() < 970) {
					$("#dragonfruit_timeline .inner-container").css({
						"position": "absolute"
					});
					positon_left = $("#dragonfruit_timeline .inner-container").position().left + 100;
					//put on right
					$(this).css({
						'left': 70,
						'top': $.timeline_right_position_top,
						'width': $(window).width() - positon_left
					});
					$(this).addClass("right_timeline");
					$.timeline_old_right_position_top = $.timeline_right_position_top;
					$.timeline_right_position_top = $.timeline_right_position_top + $(this).outerHeight() + 40;
					$(this).prepend("<span class=\"right_timer\"><span class=\"glyphicon glyphicon-time\"></span></span>");
					$(this).children("span.right_timer").css({
						left: -86, width: 60,
					});
				} else if ($.timeline_left_position_top == 0) {
					$("#dragonfruit_timeline .inner-container").css({
						"position": "relative"
					});
					//put on left
					$(this).css({
						'left': 0,
						'top': 0,
						'width': $.timeline_item_width - 50
					});
					$(this).addClass("left_timeline");
					$.timeline_old_left_position_top = $.timeline_left_position_top;
					$.timeline_left_position_top = $.timeline_left_position_top + $(this).outerHeight() + 40;
					$(this).prepend("<span class=\"left_timer\"><span class=\"glyphicon glyphicon-time\"></span></span>");
					$(this).children("span.left_timer").css({
						left: $.timeline_item_width - 50,
					});
				} else if ($.timeline_right_position_top < $.timeline_left_position_top) {
					$("#dragonfruit_timeline .inner-container").css({
						"position": "relative"
					});
					$.timeline_right_position_top = ($.timeline_old_left_position_top + 40) < $.timeline_right_position_top ? $.timeline_right_position_top : $.timeline_right_position_top + 40;
					//put on right
					$(this).css({
						'left': $.timeline_item_width + 79,
						'top': $.timeline_right_position_top,
						'width': $.timeline_item_width - 50
					});
					$(this).addClass("right_timeline");
					$.timeline_old_right_position_top = $.timeline_right_position_top;
					$.timeline_right_position_top = $.timeline_right_position_top + $(this).outerHeight() + 40;
					$(this).prepend("<span class=\"right_timer\"><span class=\"glyphicon glyphicon-time\"></span></span>");
					$(this).children("span.right_timer").css({
						left: -99,
					});
				} else {
					$("#dragonfruit_timeline .inner-container").css({
						"position": "relative"
					});
					$.timeline_left_position_top = ($.timeline_old_right_position_top + 40) < $.timeline_left_position_top ? $.timeline_left_position_top : $.timeline_left_position_top + 40;
					//put on left
					$(this).css({
						'left': 0,
						'top': $.timeline_left_position_top,
						'width': $.timeline_item_width - 50
					});
					$(this).addClass("left_timeline");
					$.timeline_old_left_position_top = $.timeline_left_position_top;
					$.timeline_left_position_top = $.timeline_left_position_top + $(this).outerHeight() + 40;
					$(this).prepend("<span class=\"left_timer\"><span class=\"glyphicon glyphicon-time\"></span></span>");
					$(this).children("span.left_timer").css({
						left: $.timeline_item_width - 50,
					});
				}
				//calculate and define container height
				if ($.timeline_left_position_top > $.timeline_right_position_top) {
					$("#dragonfruit_timeline .inner-container").height($.timeline_left_position_top - 40);
					$("#dragonfruit_timeline").height($.timeline_left_position_top + 200);
				} else {
					$("#dragonfruit_timeline .inner-container").height($.timeline_right_position_top - 40);
					$("#dragonfruit_timeline").height($.timeline_right_position_top + 200);
				}
				$(this).fadeIn();
			});
		});
	}

	if ($("#dragonfruit_about").length > 0) {
		//about
		$(document).scroll(function () {
			document_top = $(document).scrollTop();
			event_wapper_top = $("#dragonfruit_about").position().top - $('#dragonfruit_banner_menu').outerHeight();
			if (document_top < event_wapper_top) {
				degree = (360 / event_wapper_top) * (document_top);
				event_animate_num = event_wapper_top - document_top;
				event_animate_alpha = (1 / document_top) * (event_wapper_top);
				$("#dragonfruit_about .imgwap").css({
					'-webkit-transform': 'rotate(' + degree + 'deg)',
					'-moz-transform': 'rotate(' + degree + 'deg)',
					'-ms-transform': 'rotate(' + degree + 'deg)',
					'-o-transform': 'rotate(' + degree + 'deg)',
					'transform': 'rotate(' + degree + 'deg)',
				});
				$("#dragonfruit_about .about_icon").css({
					'opacity': event_animate_alpha
				});
			} else {
				$("#dragonfruit_about .imgwap").css({
					'-webkit-transform': 'rotate(' + 360 + 'deg)',
					'-moz-transform': 'rotate(' + 360 + 'deg)',
					'-ms-transform': 'rotate(' + 360 + 'deg)',
					'-o-transform': 'rotate(' + 360 + 'deg)',
					'transform': 'rotate(' + 360 + 'deg)',
				});
				$("#dragonfruit_about .about_icon").css({
					'opacity': 1
				});
			}
		});
	}

	if ($("#dragonfruit_events").length > 0) {
		//event
		$(document).scroll(function () {
			document_top = $(document).scrollTop();
			event_wapper_top = $("#dragonfruit_events").position().top - $('#dragonfruit_banner_menu').outerHeight();
			if (document_top < event_wapper_top) {
				event_animate_num = event_wapper_top - document_top;
				event_animate_alpha = (1 / event_wapper_top) * (document_top);
				$("#dragonfruit_events .event_animate_left").css({
					'left': -event_animate_num, 'opacity': event_animate_alpha
				});
				$("#dragonfruit_events .event_animate_right").css({
					'left': event_animate_num, 'opacity': event_animate_alpha
				});
			} else {
				$("#dragonfruit_events .event_animate_left").css({
					'left': 0, 'opacity': 1
				});
				$("#dragonfruit_events .event_animate_right").css({
					'left': 0, 'opacity': 1
				});
			}
		});
	}
});

//// Google Maps

function initialize() {
	if ($("#dragonfruit_contact_map").length > 0) {
		//define map
		var map;
		//lat lng
		myLatlng = new google.maps.LatLng(36.214415, -86.6149);
		//define style
		var styles = [
			{
				//set all color
				featureType: "all",
				stylers: [{ hue: "#35a9d8" }]
			},
			{
				//hide business
				featureType: "poi.business",
				elementType: "labels",
				stylers: [{ visibility: "off" }]
			}
		];

		//map options
		var mapOptions = {
			zoom: 16,
			center: myLatlng,
			mapTypeControlOptions: { mapTypeIds: [google.maps.MapTypeId.ROADMAP, 'map_style'] },
			panControl: false, //hide panControl
			zoomControl: true, //hide zoomControl
			mapTypeControl: false, //hide mapTypeControl
			scaleControl: false, //hide scaleControl
			streetViewControl: false, //hide streetViewControl
			overviewMapControl: false, //hide overviewMapControl
		}
		//adding attribute value
		map = new google.maps.Map(document.getElementById('dragonfruit_contact_map'), mapOptions);
		var styledMap = new google.maps.StyledMapType(styles, { name: "Styled Map" });
		map.mapTypes.set('map_style', styledMap);
		map.setMapTypeId('map_style');
		//add marker
		var marker = new google.maps.Marker({
			position: myLatlng,
			map: map,
			title: 'Welcome to Hermitage'
		});
	}
}

google.maps.event.addDomListener(window, 'load', initialize);
google.maps.event.addDomListener(window, 'resize', initialize);