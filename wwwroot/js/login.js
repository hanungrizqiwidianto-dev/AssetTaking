var e = Swal.mixin({ buttonsStyling: !1, customClass: { confirmButton: "btn btn-alt-success m-5", cancelButton: "btn btn-alt-danger m-5", input: "form-control" } });

function PostLogin() {
    var obj = {
        Username: $("#login-username").val(),
        Password: $("#login-password").val(),
        Jobsite: $("#jobSite").val()
    };

    $.ajax({
        url: "/api/Login/Get_Login", // API lokal
        data: JSON.stringify(obj),
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        beforeSend: function () {
            $("#overlay").show();
        },
        success: function (data) {
            console.log(data);
            if (data.remarks == true) {
                MakeSession(obj.Username, obj.Jobsite);
            } else {
                Swal.fire({
                    title: "Error!",
                    text: "Username or Password incorrect.",
                    icon: 'error',
                });
                $("#overlay").hide();
            }
        },
        error: function (xhr) {
            Swal.fire({
                title: "Error!",
                text: 'Message : ' + xhr.responseText,
                icon: 'error',
            });
        }
    });
}

function MakeSession(nrp, site) {
    var obj = {
        NRP: nrp,
        Jobsite: site
    };

    $.ajax({
        type: "POST",
        url: "/Login/MakeSession", // Controller Core MVC
        dataType: "json",
        data: JSON.stringify(obj),
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            debugger
            if (data.remarks == true) {
                window.location.href = "/Home/Index";
            } else {
                Swal.fire({
                    title: "Error!",
                    text: data.message,
                    icon: 'error',
                });
                $("#overlay").hide();
            }
        },
        error: function (xhr) {
            alert(xhr.responseText);
        }
    });
}

function getRole() {
    $.ajax({
        url: "/api/Master/GetRole", // API lokal
        type: "GET",
        cache: false,
        success: function (result) {
            console.log("GetRole API Response:", result); // Debug log
            $('#jobSite').empty();
            let text = '<option></option>';
            
            // Check if result has Data property and it's an array
            if (result && result.data && Array.isArray(result.data)) {
                $.each(result.data, function (key, val) {
                    console.log("Role item:", val); // Debug log
                    text += '<option value="' + val.roleName + '">' + val.roleName + '</option>';
                });
            } else if (result && Array.isArray(result)) {
                // If result is directly an array
                $.each(result, function (key, val) {
                    console.log("Role item (direct array):", val); // Debug log
                    text += '<option value="' + val.roleName + '">' + val.roleName + '</option>';
                });
            } else {
                console.error("Invalid data structure:", result);
                console.log("Type of result:", typeof result);
                console.log("Result keys:", Object.keys(result || {}));
                text += '<option>No roles found</option>';
            }
            
            $("#jobSite").html(text);
            
            // Refresh select2 if it's initialized
            if ($("#jobSite").hasClass("select2-hidden-accessible")) {
                $("#jobSite").trigger('change');
            }
            
            console.log("Final HTML:", text); // Debug log
        },
        error: function (xhr, status, error) {
            console.error("GetRole API Error:", xhr.responseText);
            $('#jobSite').empty();
            $("#jobSite").html('<option>Error loading roles</option>');
        }
    });
}

$(document).ready(function () {
    console.log("Login.js document ready");
    
    // Initialize select2 first
    setTimeout(function() {
        console.log("Initializing select2 and calling getRole");
        $('#jobSite').select2({
            placeholder: "Select a role",
            allowClear: true
        });
        
        // Call getRole after a small delay to ensure DOM is ready
        getRole();
    }, 500);
});
