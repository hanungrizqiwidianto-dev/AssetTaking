$(document).ready(function () {
    $("#btnSubmit").click(function () {
        var data = {
            AppName: $("#AppName").val(),
            Division: $("#Division").val(),
            Description: $("#Description").val(),
            CreatedBy: $("#hd_nrp").val()
        };

        $.ajax({
            url: "/api/Register/Create", // API lokal
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(data),
            success: function (res) {
                $("#message").html(
                    '<div class="alert alert-success">App registered successfully!</div>'
                );
                $("#registerForm")[0].reset();
            },
            error: function (err) {
                $("#message").html(
                    '<div class="alert alert-danger">Error: ' + err.responseText + '</div>'
                );
            }
        });
    });
});
