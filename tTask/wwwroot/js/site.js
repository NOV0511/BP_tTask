// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

var commentLoadingCompleted = true;
var assignedToLoadingCompleted = true;

Date.prototype.addHours = function (h) {
    this.setTime(this.getTime() + (h * 60 * 60 * 1000));
    return this;
}

$(function() {
    $('[data-toggle="popover"]').popover({
        trigger: 'click'
    });
});

//$(".tasks-sublist-item").click(function () {
$(".task-title").click(function () {
    if (commentLoadingCompleted && assignedToLoadingCompleted) {
        commentLoadingCompleted = false;
        assignedToLoadingCompleted = false;


        $(".tasks-sidebar").addClass("show");
        //$(".tasks-sidebar").css("display", "block");
        //if ($("#people-sidebar").length) {
        //    $("#people-sidebar").css("display", "none");
        //} else {
        //    $("#tasks-main").addClass("task-detail-active");
        //}

        $(".task-title").each(function () {
            $(this).removeClass("selected-task");
        });
        $(this).addClass("selected-task");

        $.ajax({
            type: "GET",
            url: "Tasks/GetTask",
            data: { id: this.dataset.id },
            dataType: "text",
            success: function (r) {
                var task = JSON.parse(r);
                if (task.completed === null) {
                    $("#task-state").text("To be done...");
                }
                else {
                    let date = task.completed.substring(0, 10);
                    let time = task.completed.substring(11, 16);
                    $("#task-state").text("Completed " + date + " " + time);
                }

                let date = task.created.substring(0, 10);
                let time = task.created.substring(11, 16);
                $("#task-created").text(date + " " + time);

                $("#task-name p").text(task.name);
                $("#task-description p").text(task.description);

                $.ajax({
                    type: "GET",
                    url: "Tasks/GetLoggedUserId",
                    dataType: "text",
                    success: function (r) {
                        var userId = JSON.parse(r);
                        $("#task-detail-btns").empty();
                        if (userId == task.idUser) {
                            
                            $("<button/>", {
                                html: 'Modify',
                                class: 'btn btn-outline-primary',
                                click: function () {
                                    $("#update-task-name").val(task.name);
                                    $("#update-task-description").val(task.description);
                                    
                                    $("#update-task-deadline").val(task.deadline.replace('T', ' '));
                                    $("#update-task-idTask").val(task.idTask);

                                    var priority = $("#update-task-priority");
                                    if (priority.length === 0) {
                                        $("#update-task-choose-priority").val(task.priority);
                                    }

                                    $.ajax({
                                        type: "GET",
                                        url: "Tasks/GetProjectUsers",
                                        data: { idProject: task.idProject },
                                        dataType: "text",
                                        success: function (r) {
                                            var users = JSON.parse(r);

                                            $("#update-task-assign").empty();

                                            for (let i in users) {
                                                $("#update-task-assign").append('<option value="' + users[i].id + '">' + users[i].firstName + ' ' + users[i].surname + ' - ' + users[i].email + '</option>');
                                            }
                                            document.getElementById('update-task-assign').size = document.getElementById('update-task-assign').length < 3 ? document.getElementById('update-task-assign').length : 3;


                                            var values = []

                                            for (var u in task.userTask) {
                                                values.push(task.userTask[u].idUser);
                                            }
                                            $("#update-task-assign").val(values);


                                            $("body").addClass("dialog-open");
                                            window.scrollTo(0, 0);

                                            $("#dialog-update-task").css("display", "flex");

                                        }
                                    });


                                }
                            }).appendTo("#task-detail-btns");

                            $("<button/>", {
                                html: 'Delete',
                                class: 'btn btn-outline-secondary ml-2',
                                click: function () {
                                    NewDialogDeleteTask(task.idTask);
                                }
                            }).appendTo("#task-detail-btns");
                        }
                    }
                });

                $("#task-priority").text(task.priority);

                date = task.deadline.substring(0, 10);
                time = task.deadline.substring(11, 16);
                $("#task-deadline").text(date + " " + time);

                $.ajax({
                    type: "GET",
                    url: "Tasks/GetUser",
                    data: { id: task.idUser },
                    dataType: "text",
                    success: function (r) {
                        var user = JSON.parse(r);
                        $("#task-appliciant-name").text(user.firstName + " " + user.surname);

                        if (user.photopath == null) {
                            $("#task-appliciant-img").html('<img src="/img/empty_profile.png" />');
                        } else {
                            $("#task-appliciant-img").html('<img src="' + user.photopath + '" />');
                        }
                    }
                });

                $("#task-assigned-to").empty();
                for (var u in task.userTask) {
                    if (u == task.userTask.length - 1)
                        GetAssignedTo(task.userTask[u], true);
                    else
                        GetAssignedTo(task.userTask[u], false);
                }
                if (task.userTask.length == 0) assignedToLoadingCompleted = true;

                $.ajax({
                    type: "GET",
                    url: "Tasks/GetComments",
                    data: { id: task.idTask },
                    dataType: "text",
                    success: async function (r) {
                        var comments = JSON.parse(r);
                        $("#task-comment-counter").text("(" + comments.length + ")");

                        $("#task-comments").empty();

                        for (var c in comments) {
                            if (c == comments.length-1)
                                await GetComments(comments[c], true);
                            else
                                await GetComments(comments[c], false);
                        }
                        if (comments.length == 0) commentLoadingCompleted = true;

                    }
                });

            }
        });
    }
});

function GetComments(comment, last) {
    return $.ajax({
        type: "GET",
        url: "Tasks/GetUser",
        data: { id: comment.idUser },
        dataType: "text",
        success: function (r) {
            var user = JSON.parse(r);
            var divComment = '<div class="task-comment-item flex">'

            if (user.photopath == null) {
                divComment += '<div><img src="/img/empty_profile.png" /></div>';
            } else {
                divComment += '<div><img src="' + user.photopath + '" /></div>';
            }
            let date = comment.created.substring(0, 10);
            let time = comment.created.substring(11, 16);
            divComment += `<div class="task-comment-textpart">
                                                <div class="flex task-comment-properties"> 
                                                    <div class="task-comment-name">` + user.firstName + " " + user.surname + `</div>
                                                    <div class="task-comment-date">` + date + " " + time + `</div>`;
            $.ajax({
                type: "GET",
                url: "Tasks/GetLoggedUserId",
                dataType: "text",
                success: function (r) {
                    var userId = JSON.parse(r);
                    if (userId == comment.idUser) {
                        divComment += '<div class="task-comment-delete"><i class="far fa-trash-alt" onclick="DeleteComment(' + comment.idTask + ',' + comment.idComment + ')"></i></div>'
                    } 

                    divComment += `</div>
                        <div class="task-comment-text">` + comment.text + `</div> 
                    </div>`;
                    divComment += '</div>';
                    $("#task-comments").append(divComment);

                    if (last) commentLoadingCompleted = true;
                }
            });
        }
    });
}

function DeleteComment(idTask, idComment) {
    $.ajax({
        type: "POST",
        url: "Tasks/DeleteComment",
        data: { idComment: idComment },
        dataType: "text",
        success: function (r) {
            $('.task-title[data-id="' + idTask + '"]')[0].click();
        }
    });
}

function GetAssignedTo(userTask, last) {
    $.ajax({
        type: "GET",
        url: "Tasks/GetUser",
        data: { id: userTask.idUser },
        dataType: "text",
        success: function (r) {
            var user = JSON.parse(r);
            var div = document.createElement('div');

            if (user.photopath == null) {
                div.innerHTML = '<img src="/img/empty_profile.png" />';
            } else {
                div.innerHTML = '<img src="' + user.photopath + '" />';
            }


            div.innerHTML += " " + user.firstName + " " + user.surname;

            if (userTask.completed != null) {
                div.innerHTML += '<span class="task-user-completed"><i class="far fa-check-circle"></i></span>';
                $("#task-assigned-to").append(div);
                if (last) assignedToLoadingCompleted = true;
            } else {
                $.ajax({
                    type: "GET",
                    url: "Tasks/GetLoggedUserId",
                    dataType: "text",
                    success: function (r) {
                        var userId = JSON.parse(r);
                        if (userId == userTask.idUser) {
                            div.innerHTML += '<span id="task-mark-completed" onclick="completeTask();" title="Mark as completed!"><i class="fas fa-flag-checkered"></i></span>';
                        }

                        $("#task-assigned-to").append(div);
                        if (last) assignedToLoadingCompleted = true;
                    }
                });
            }
        }
    });
}



$("#submit-new-comment").click(function () {
    var task = $(".selected-task")[0];
    $.ajax({
        type: "GET",
        url: "Tasks/CanComment",
        data: { idTask: task.dataset.id },
        dataType: "text",
        success: function (r) {
            var canComment = JSON.parse(r);
            if (canComment) {
                var text = $("#comment-text").val();
                if (text.length > 0) {
                    $.ajax({
                        type: "POST",
                        url: "Tasks/NewComment",
                        data: { text: text, idTask: task.dataset.id },
                        dataType: "text",
                        success: function (r) {
                            $("#comment-text").val("");
                            task.click();
                        }
                    });
                }
            }
            else {
                NewAlert('Task has got maximum comments possible. \n If you want to add more comments, please subscribe to higher service.');
                $("#comment-text").val("");
            }
        }
    });
   
});

$("#add-new-project-btn").click(function () {
    $.ajax({
        type: "GET",
        url: "Project/CanAdd",
        dataType: "text",
        success: function (r) {
            var canAdd = JSON.parse(r);
            if (canAdd) {
                var name = $("#add-new-project-name").val();
                var desc = $("#add-new-project-description").val();
                if (name.length > 0) {
                    $.ajax({
                        type: "POST",
                        url: "Project/AddNewProject",
                        data: { projectName: name, projectDescription: desc },
                        dataType: "text",
                        success: function (r) {
                            location.href = location.href;
                        }
                    });
                }
            }
            else {
                NewAlert('You cannot add more projects. \n If you want to add more projects, please subscribe to higher service.');
                $("#dialog-new-project").css("display", "none");
            }
        }
    });

});

$("#update-project-btn").click(function () {
    var id = $("#add-new-project-id").val();
    var name = $("#add-new-project-name").val();
    var desc = $("#add-new-project-description").val();
    if (name.length > 0) {
        $.ajax({
            type: "POST",
            url: "Project/UpdateProject",
            data: { idProject: id, projectName: name, projectDescription: desc },
            dataType: "text",
            success: function (r) {
                location.href = location.href;
            }
        });
    }
});

$("#modify-project-btn").click(function () {
    var id = $("#modify-project-id").val();
    var name = $("#modify-project-name").val();
    var desc = $("#modify-project-description").val();
    if (name.length > 0) {
        $.ajax({
            type: "POST",
            url: "Project/UpdateProject",
            data: { idProject: id, projectName: name, projectDescription: desc },
            dataType: "text",
            success: function (r) {
                location.href = location.href;
            }
        });
    }
});

$("#task-mark-completed").click(function () {
    var task = $(".selected-task")[0];
    $.ajax({
        type: "POST",
        url: "Tasks/CompleteTask",
        data: { idTask: task.dataset.id },
        dataType: "text",
        success: function (r) {
            task.click();
        }
    });
});

function completeTask() {
    var task = $(".selected-task")[0];
    $.ajax({
        type: "POST",
        url: "Tasks/CompleteTask",
        data: { idTask: task.dataset.id },
        dataType: "text",
        success: function (r) {
            task.click();
        }
    });
}


function ChangeProjectActiveState(element, idUser, idProject, idRole, state) {
    var checkedValue = state == '1' ? true : false;
    $.ajax({
        type: "POST",
        url: "HomePage/ChangeProjectState",
        data: { idProject: idProject, idUser: idUser, idRole: idRole, checkedValue: state },
        dataType: "text",
        success: function (r) {
            if (checkedValue) {
                $(element).removeClass("project-choose-no");
                $(element).addClass("project-choose-active");
            }
            else {
                $(element).addClass("project-choose-no");
                $(element).removeClass("project-choose-active");
            }
            location.href = location.href;
        }
    });
}

function ProjectRequest(idProject, action) {
    $.ajax({
        type: "POST",
        url: "HomePage/ProjectRequest",
        data: { idProject: idProject, action: action },
        dataType: "text",
        success: function (r) {
            location.href = location.href;
        }
    });
}


function AddNewProject() {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#dialog-new-project").css("display", "flex");
}

function CloseDialogNewProject() {
    $("body").removeClass("dialog-open");
    $("#dialog-new-project").css("display", "none");
}

function ModifyProject(id, name, desc) {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#dialog-modify-project").css("display", "flex");
    $("#modify-project-id").val(id);
    $("#modify-project-name").val(name);
    $("#modify-project-description").val(desc);
}

function CloseDialogModifyProject() {
    $("body").removeClass("dialog-open");
    $("#dialog-modify-project").css("display", "none");
}

function CloseTaskDetail() {
    $(".tasks-sidebar").removeClass("show");

        $(".tasks-sublist-item").each(function () {
        $(this).removeClass("selected-task");
    });
}

//function CloseTaskDetail() {
//    $(".tasks-sidebar").css("display", "none");
//    if ($("#people-sidebar").length) {
//        $("#people-sidebar").css("display", "block");
//    }
//    else {
//        $("#tasks-main").removeClass("task-detail-active");
//    }
//    $(".tasks-sublist-item").each(function () {
//        $(this).removeClass("selected-task");
//    });
//}

function ChangeUserProjectRole(idUser, idProject) {
    $.ajax({
        type: "POST",
        url: "Project/ChangeRole",
        data: { idUser: idUser, idProject: idProject },
        dataType: "text",
        success: function (r) {
            location.href = location.href;
        }
    });
}

function AddNewTask() {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);

    var today = new Date().addHours(1)
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();
    var hh = today.getHours();
    var MM = today.getMinutes();
    if (dd < 10) {
        dd = '0' + dd
    }
    if (mm < 10) {
        mm = '0' + mm
    }
    if (hh < 10) {
        hh = '0' + hh
    }
    if (MM < 10) {
        MM = '0' + MM
    }

    today = yyyy + '-' + mm + '-' + dd + ' ' + hh + ':' + MM;
    $("#dialog-task-deadline").val(today);
    document.getElementById('dialog-assign').size = document.getElementById('dialog-assign').length < 3 ? document.getElementById('dialog-assign').length : 3;
    
    $("#dialog-new-task").css("display", "flex");
}
function AddUserToProject() {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#dialog-add-user").css("display", "flex");
}

function CloseDialogNewTask() {
    $("body").removeClass("dialog-open");
    $("#dialog-new-task").css("display", "none");
}
function CloseDialogAddUser() {
    $("body").removeClass("dialog-open");
    $("#dialog-add-user").css("display", "none");
}

$("#choose-project").on('change', function () {
    $("#dialog-id-project").val(this.value);
    $.ajax({
        type: "GET",
        url: "Tasks/GetProjectUsers",
        data: { idProject: this.value },
        dataType: "text",
        success: function (r) {
            var users = JSON.parse(r);
           
            $("#dialog-assign").empty();

            for (let i in users) {
                $("#dialog-assign").append('<option value="' + users[i].id + '">' + users[i].firstName + ' ' + users[i].surname + ' - ' + users[i].email + '</option>');
            }
            document.getElementById('dialog-assign').size = document.getElementById('dialog-assign').length < 3 ? document.getElementById('dialog-assign').length : 3;

        }
    });
})


$("#img").on('change', function (e) {
    var labelVal = $("#upload-img-label").val;
    var fileName = e.target.value.split('\\').pop();
    if (fileName.length > 13)
        fileName = fileName.substring(0, 14) + "...";

    if (fileName)
        $("#upload-img-label").text(fileName);
    else
        $("#upload-img-label").text(labelVal);
});


function DemoteUser(idUser) {
    $.ajax({
        type: "POST",
        url: "Tenant/DemoteUser",
        data: { idUser: idUser },
        dataType: "text",
        success: function (r) {
            location.href = location.href;
        }
    });
}


function ViewChangeService() {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#dialog-change-service").css("display", "flex");
}

function CloseDialogChangeService() {
    $("body").removeClass("dialog-open");
    $("#dialog-change-service").css("display", "none");
}

function CloseDialogAlert() {
    $("body").removeClass("dialog-open");
    $("#dialog-alert").css("display", "none");
}

function NewDialogDeleteProject() {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#dialog-delete-project").css("display", "flex");
}

function NewDialogDeleteProject(idProject) {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#dialog-delete-project").css("display", "flex");
    $("#delete-project-id").val(idProject);
}

function CloseDialogDeleteProject() {
    $("body").removeClass("dialog-open");
    $("#dialog-delete-project").css("display", "none");
}

function NewDialogDeleteTask(idTask) {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#delete-task-id").val(idTask);
    $("#dialog-delete-task").css("display", "flex");
}

function CloseDialogDeleteTask() {
    $("body").removeClass("dialog-open");
    $("#dialog-delete-task").css("display", "none");
}

function CloseDialogUpdateTask() {
    $("body").removeClass("dialog-open");
    $("#dialog-update-task").css("display", "none");
}

function CloseDialogPayment() {
    $("body").removeClass("dialog-open");
    $("#dialog-payment").css("display", "none");
}

function CheckServiceChange(idNewService, idCurrentService) {
    $("#dialog-change-service").css("display", "none");
    $.ajax({
        type: "GET",
        url: "Service/GetTenantNumbers",
        dataType: "text",
        success: function (r) {
            var obj = JSON.parse(r);
            console.log(obj);
            var alertMsg = '';
            var fail = false;

            if (idNewService === idCurrentService) {
                alertMsg = 'You are already using this service!';
                fail = true;
            }
            else if (idNewService === 1) {
                if (obj.noUsers > 5) {
                    alertMsg = 'You cannot get lower service, because your application has ' + obj.noUsers + ' users! \n Maximum for basic service is 5 users.';
                    fail = true;
                }
                if (obj.noProjects > 1) {
                    alertMsg = '\nYou cannot get lower service, because your application has ' + obj.noProjects + ' projects! \n Maximum for basic service is 1 project.';
                    fail = true;
                }
            }
            else if (idNewService === 2 && idCurrentService !== 1) {
                if (obj.noUsers > 10) {
                    alertMsg = 'You cannot get lower service, because your application has ' + obj.noUsers + ' users! \n Maximum for pro service is 10 users.';
                    fail = true;
                }
                if (obj.noProjects > 5) {
                    alertMsg = '\nYou cannot get lower service, because your application has ' + obj.noProjects + ' projects! \n Maximum for pro service is 5 projects.';
                    fail = true;
                }
            }

            if (fail) {
                $("#alert-text").text(alertMsg);
                $("#dialog-alert").css("display", "flex");
            }
            else {
                $.ajax({
                    type: "GET",
                    url: "Service/GetService",
                    data: { idService: idNewService },
                    dataType: "text",
                    success: function (r) {
                        var service = JSON.parse(r);
                        $("#payment-name").text(service.name);
                        $("#payment-price").text(service.price);
                        $("#payment-id-service").val(service.idService);

                        $("#dialog-payment").css("display", "flex");
                    }
                });
            }
        }
    });
}

function ViewPayment(name, price, idService) {
    console.log(name);
    $("#payment-name").text(name.toString());
    $("#payment-price").text(price);
    $("#payment-id-service").val(idService);

    $("#dialog-payment").css("display", "flex");

}

function ChangeSettings(checkbox, name) {
    $.ajax({
        type: "POST",
        url: "Profile/ChangeSettings",
        data: { name: name, checkValue: checkbox.checked },
        dataType: "text",
        success: function (r) {
            if (name == "notifications") {
                if (checkbox.checked) {
                    $("#sidenav-notifications").css("display", "block");

                } else {
                    $.ajax({
                        type: "GET",
                        url: "Notification/GetNOProjectRequests",
                        dataType: "text",
                        success: function (r) {
                            var count = JSON.parse(r);
                            if (count != 0) {
                                $("#notification-counter").text(count > 99 ? 99 : count);
                                $("#notification-counter").css("display", "inline-block");
                            }
                            else {
                                $("#sidenav-notifications").css("display", "none");
                            }
                        }
                    });
                }
            }
        }
    });
}

function NewAlert(msg) {
    $("body").addClass("dialog-open");
    window.scrollTo(0, 0);
    $("#alert-text").text(msg);
    $("#dialog-alert").css("display", "flex");
}


$('#dialog-task-deadline').datetimepicker({
    minDate: new Date(),
    startDate: new Date()
});

$('#update-task-deadline').datetimepicker({
    minDate: new Date(),
    startDate: new Date()
});