Codebase.helpersOnLoad(['jq-select2']);

var table = $("#tbl_menu").DataTable({
    ajax: {
        url: "/api/Setting/Get_Menu/" + $("#txt_group").val(),
        dataSrc: function(json) {
            console.log("Menu DataTable response:", json);
            return json.Data || json.data || json;
        },
    },
    "columnDefs": [
        { "className": "dt-center", "targets": [0] }
    ],
    scrollX: true,
    columns: [
        {
            data: 'isAllow',
            targets: 'no-sort', orderable: false,
            render: function (data, type, row) {
                let idMenu = "";
                if (row.idMenu == 1) {
                    idMenu = "disabled"
                }
                let action = "";
                if (data == 1 || data == true) {
                    action = `<input type="checkbox" class="form-control-sm" name="txt_allow" data-menu="${row.idMenu}" onclick="updateMenu(this)" checked ${idMenu}>`;
                } else {
                    action = `<input type="checkbox" class="form-control-sm" name="txt_allow" data-menu="${row.idMenu}" onclick="updateMenu(this)">`;
                }
                return action;
            }
        },
        { data: 'nameMenu' }
    ],
});

$("#txt_group").on("change", function () {
    console.log("Role changed to:", this.value);
    table.ajax.url("/api/Setting/Get_Menu/" + this.value).load();
})

function updateMenu(cek) {
    let obj = new Object();
    obj.IdMenu = parseInt(cek.getAttribute('data-menu'));
    obj.IdRole = parseInt($('#txt_group').val());
    if ($(cek).prop("checked") == true) {
        obj.IsAllow = true;
    }
    else if ($(cek).prop("checked") == false) {
        obj.IsAllow = false;
    }

    console.log("Update menu request:", obj);

    $.ajax({
        url: "/api/Setting/Update_Menu",
        data: JSON.stringify(obj),
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            console.log("Update menu response:", data);
            if (data.remarks == true) {
                Swal.fire('Saved!', 'Data has been Saved.', 'success');
                table.ajax.reload();
            } if (data.remarks == false) {
                Swal.fire('Error!', 'Message : ' + data.message, 'error');
            }
        },
        error: function (xhr) {
            console.error("Update menu error:", xhr.responseText);
            alert(xhr.responseText);
        }
    })
}
