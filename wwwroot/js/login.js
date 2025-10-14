var e = Swal.mixin({ buttonsStyling: !1, customClass: { confirmButton: "btn btn-alt-success m-5", cancelButton: "btn btn-alt-danger m-5", input: "form-control" } });

function PostLogin() {
    // Validation
    const username = $("#login-username").val().trim();
    const password = $("#login-password").val().trim();
    const jobsite = $("#jobSite").val();
    
    if (!username) {
        Swal.fire({
            title: "Validation Error!",
            text: "Please enter your username.",
            icon: 'warning',
        });
        $("#login-username").focus();
        return;
    }
    
    if (!password) {
        Swal.fire({
            title: "Validation Error!",
            text: "Please enter your password.",
            icon: 'warning',
        });
        $("#login-password").focus();
        return;
    }
    
    if (!jobsite) {
        Swal.fire({
            title: "Validation Error!",
            text: "Please select a role.",
            icon: 'warning',
        });
        return;
    }

    var obj = {
        Username: username,
        Password: password,
        Jobsite: jobsite
    };

    $.ajax({
        url: "/api/Login/Get_Login", // API lokal
        data: JSON.stringify(obj),
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        beforeSend: function () {
            // Show loading state
            $("#overlay").show();
            $("#loginBtn").prop('disabled', true);
            $("#loginBtnText").text('Signing In...');
            $("#loginSpinner").show();
        },
        success: function (data) {
            console.log(data);
            if (data.remarks == true) {
                MakeSession(obj.Username, obj.Jobsite);
            } else {
                Swal.fire({
                    title: "Login Failed!",
                    text: "Username or Password incorrect.",
                    icon: 'error',
                    confirmButtonColor: '#667eea'
                });
                resetLoginButton();
            }
        },
        error: function (xhr) {
            Swal.fire({
                title: "Connection Error!",
                text: 'Unable to connect to server. Please try again later.',
                icon: 'error',
                confirmButtonColor: '#667eea'
            });
            resetLoginButton();
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
                    title: "Session Error!",
                    text: data.message || "Failed to create session. Please try again.",
                    icon: 'error',
                    confirmButtonColor: '#667eea'
                });
                resetLoginButton();
            }
        },
        error: function (xhr) {
            Swal.fire({
                title: "Server Error!",
                text: 'Unable to process request. Please contact administrator.',
                icon: 'error',
                confirmButtonColor: '#667eea'
            });
            resetLoginButton();
        }
    });
}

function resetLoginButton() {
    $("#overlay").hide();
    $("#loginBtn").prop('disabled', false);
    $("#loginBtnText").text('Sign In');
    $("#loginSpinner").hide();
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
            allowClear: true,
            theme: 'bootstrap-5'
        });
        
        // Call getRole after a small delay to ensure DOM is ready
        getRole();
    }, 500);
    
    // Handle Enter key press for login
    $('#loginForm input').on('keypress', function(e) {
        if (e.which === 13) { // Enter key
            e.preventDefault();
            PostLogin();
        }
    });
    
    // Handle form submission
    $('#loginForm').on('submit', function(e) {
        e.preventDefault();
        PostLogin();
    });
    
    // Focus on username field on page load
    setTimeout(function() {
        $('#login-username').focus();
    }, 1000);
});
