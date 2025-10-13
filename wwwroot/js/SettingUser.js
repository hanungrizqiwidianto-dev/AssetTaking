Codebase.helpersOnLoad(['jq-select2']);

$("document").ready(function () {
    $('.select2-modal').select2({
        dropdownParent: $('.modal')
    });
})

$("#txt_nrp").on("change", function () {
    $.ajax({
        url: "/api/Master/Get_Employee/" + this.value, // API lokal
        type: "GET",
        cache: false,
        success: function (result) {
            console.log("Employee response:", result); // Debug log
            // Check different possible response structures
            if (result && result.data && result.data.length > 0) {
                let employee = result.data[0];
                $("#txt_district").val(employee.deptCode || employee.DeptCode || '');
                $("#txt_email").val(employee.email || employee.Email || '');
            } else if (result && result.Data && result.Data.length > 0) {
                let employee = result.Data[0];
                $("#txt_district").val(employee.DSTRCT_CODE || employee.DeptCode || '');
                $("#txt_email").val(employee.EMAIL || employee.Email || '');
            } else {
                console.log("No employee data found");
                $("#txt_district").val('');
                $("#txt_email").val('');
            }
        },
        error: function(xhr, status, error) {
            console.error("Employee API Error:", xhr.responseText);
        }
    });
})

var table = $("#tbl_user").DataTable({
    ajax: {
        url: "/api/Setting/Get_UserSetting", // API lokal
        dataSrc: function(json) {
            console.log("DataTable response:", json); // Debug log
            return json.Data || json.data || json;
        },
    },
    "columnDefs": [
        { "className": "dt-center", "targets": [0] }
    ],
    scrollX: true,
    columns: [
        { data: 'username' },
        { data: 'name' }, // Changed from 'NAME' to 'Name'
        { data: 'roleName' },
        {
            data: 'idRole', // Changed from 'ID.Role' to 'IdRole'
            targets: 'no-sort', orderable: false,
            render: function (data, type, row) {
                action = `<div class="btn-group">`
                action += `<button type="button" value="${row.username}" onclick="deleteUser(${row.idRole}, this.value)" class="btn btn-sm btn-danger" title="Delete">Delete
                                </button>` // Changed from ID_Role to IdRole
                action += `</div>`
                return action;
            }
        }
    ],

});


function insertUser() {
    let obj = new Object();
    obj.IdRole = $('#txt_group').val(); // Changed from ID_Role to IdRole
    obj.Username = $('#txt_nrp').val();

    $.ajax({
        url: "/api/Setting/Create_User", // API lokal
        data: JSON.stringify(obj),
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            if (data.remarks == true) {
                Swal.fire(
                    'Saved!',
                    'Data has been Saved.',
                    'success'
                );
                $('#modal-insert').modal('hide');
                $('.select2-modal').val('').trigger('change');
                $('.form-control').val('');

                table.ajax.reload();
            } if (data.remarks == false) {
                Swal.fire(
                    'Error!',
                    'Message : ' + data.message,
                    'error'
                );
            }

        },
        error: function (xhr) {
            alert(xhr.responseText);
        }
    })
}

function deleteUser(role, nrp) {
    Swal.fire({
        title: "Are you sure?",
        text: "You will not be able to recover this data!",
        icon: "warning",
        showCancelButton: !0,
        customClass: { confirmButton: "btn btn-alt-danger m-1", cancelButton: "btn btn-alt-secondary m-1" },
        confirmButtonText: "Yes, delete it!",
        html: !1,
        preConfirm: function (e) {
            return new Promise(function (e) {
                setTimeout(function () {
                    e();
                }, 50);
            });
        },
    }).then(function (n) {
        if (n.value == true) {
            $.ajax({
                url: "/api/Setting/Delete_User?role=" + role + "&nrp=" + nrp, // API lokal
                type: "DELETE", // Changed from POST to DELETE to match controller method
                success: function (data) {
                    if (data.remarks == true) {
                        Swal.fire("Deleted!", "Your Data has been deleted.", "success");
                        table.ajax.reload();
                    } if (data.remarks == false) {
                        Swal.fire("Cancelled", "Message : " + data.message, "error");
                    }

                },
                error: function (xhr) {
                    alert(xhr.responseText);
                }
            })
        } else {
            Swal.fire("Cancelled", "Your Data is safe", "error");
        }
    });
}
