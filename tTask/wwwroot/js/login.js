$("#sign-in").click(function () {
    $("#sign-in").addClass("active");
    $("#sign-up").removeClass("active");
    $("#sign-in-form").addClass("selected");
    $("#sign-up-form").removeClass("selected");
});

$("#sign-up").click(function () {
    $("#sign-up").addClass("active");
    $("#sign-in").removeClass("active");
    $("#sign-up-form").addClass("selected");
    $("#sign-in-form").removeClass("selected");
});

