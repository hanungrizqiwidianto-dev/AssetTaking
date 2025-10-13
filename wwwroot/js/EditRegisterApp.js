$(document).ready(function () {
    var id = $("#Id").val();

    // Load existing data
    $.get("/api/Register/GetById/" + id, function (data) { // API lokal
        $("#AppName").val(data.AppName);
        $("#Division").val(data.Division);
        $("#Description").val(data.Description);
    });

    // Update data
    $("#btnUpdate").click(function () {
        var data = {
            Id: $("#Id").val(),
            AppName: $("#AppName").val(),
            Division: $("#Division").val(),
            Description: $("#Description").val(),
            UpdatedBy: $("#hd_nrp").val()
        };

        $.ajax({
            url: "/api/Register/Update", // API lokal
            type: "PUT",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (res) {
                $("#message").html('<div class="alert alert-success">Update success!</div>');
            },
            error: function (err) {
                $("#message").html('<div class="alert alert-danger">Error: ' + err.responseText + '</div>');
            }
        });
    });
});
