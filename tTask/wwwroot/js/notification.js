function init() {
    $("#notification-counter").css("display", "none");
    $.ajax({
        type: "GET",
        url: "Notification/UserUseNotifications",
        dataType: "text",
        success: function (r) {
            var useNotification = JSON.parse(r);
            if (useNotification) {

                $.ajax({
                    type: "GET",
                    url: "Notification/GetCountUnread",
                    dataType: "text",
                    success: function (r) {
                        var count = JSON.parse(r);
                        /*if (count != 0)*/ {
                            $("#notification-counter").text(count > 99 ? 99 : count);
                            $("#sidenav-notifications").css("display", "block");
                            $("#notification-counter").css("display", "inline-block");
                        }
                    }
                });
            }

            else {
                $.ajax({
                    type: "GET",
                    url: "Notification/GetNOProjectRequests",
                    dataType: "text",
                    success: function (r) {
                        var count = JSON.parse(r);
                        if (count != 0) {
                            $("#notification-counter").text(count > 99 ? 99 : count);
                            $("#sidenav-notifications").css("display", "block");
                            $("#notification-counter").css("display", "inline-block");
                        }
                    }
                });
            }
        }
    });
}

window.onload = init;




var notificationsActive = false;

/*
function ShowNotifications() {
    if (notificationsActive) {
        $("#notification-tab").css("display", "none");
        notificationsActive = false;

    } else {
        $.ajax({
            type: "GET",
            url: "Notification/GetUnreadNotifications",
            dataType: "text",
            success: function (r) {
                var notifications = JSON.parse(r);


                $("#notification-list").empty();
                for (var i in notifications) {
                    var item = '<div class="notification-item" onclick="ReadNotification(' + notifications[i].idNotification + ')">' +
                                    '<div class="notification-text"> ' + notifications[i].text + '</div>' +
                                    '<div class="notification-date"> ' + notifications[i].created.substring(0, 10) + ' ' + notifications[i].created.substring(11, 16) + '</div>' +
                                '</div>';
                    $("#notification-list").append(item);
                }


                $("#notification-tab").css("display", "block");
            }
        });

        notificationsActive = true;
    }
}*/

function ReadNotification(idNotification) {
    $.ajax({
        type: "POST",
        url: "Notification/ReadNotification",
        data: { idNotification: idNotification },
        dataType: "text",
        success: function (r) {
            location.href = location.href;
            //notificationsActive = false;
            //ShowNotifications();
            init();
        }
    });
}

function ReadAllNotifications() {
    $.ajax({
        type: "POST",
        url: "Notification/ReadAllNotifications",
        dataType: "text",
        success: function (r) {
            location.href = location.href;
        }
    });
}

function ShowRead() {
    $("#notifications-read").css("display", "block");
    $("#notifications-unread").css("display", "none");
    $("#not-btn-read").addClass("notifications-btn-active");
    $("#not-btn-unread").removeClass("notifications-btn-active");
}

function ShowUnread() {
    $("#notifications-read").css("display", "none");
    $("#notifications-unread").css("display", "block");
    $("#not-btn-read").removeClass("notifications-btn-active");
    $("#not-btn-unread").addClass("notifications-btn-active");
}