function Disable(idTenant, idService) {
   
    if (idService === 1) {
        NewAlert('You cannot disable tenant with basic service.');
    }
    else {
        $.ajax({
            type: "POST",
            url: "Admin/Disable",
            data: { idTenant: idTenant },
            dataType: "text",
            success: function (r) {
                NewAlert('Tenant has been disabled till next payment.');
            }
        });
    }
}

function ViewChange(idTenant, idService) {
    if (idService === 3) {
        NewAlert("You cannot change to better service from service Business.");
    }
    else {
        $("#change-service").empty();
        if (idService == 1)
            $("#change-service").append('<option value="2">Pro</option>');
        $("#change-service").append('<option value="3">Business</option>');
        $("#change-id-tenant").val(idTenant);
        $("#dialog-change").css("display", "flex");
    }
}

function ViewAdd(idTenant, idService) {
    if (idService === 1) {
        NewAlert('You cannot add days to tenant with basic service.');
    }
    else {
        $("#add-id-tenant").val(idTenant);
        $("#dialog-add").css("display", "flex");
    }
}

function NewAlert(text) {
    $("#alert-text").text(text);
    $("#dialog-alert").css("display", "flex");

}


function CloseChange() {
    $("#dialog-change").css("display", "none");
}

function CloseAdd() {
    $("#dialog-add").css("display", "none");

}

function CloseDialogAlert() {
    $("#dialog-alert").css("display", "none");

}