$(document).ready(function () {
    var appId = $("#appId").val();

    // Load employees untuk AssignedTo dropdown
    $.get("/Task/GetEmployees", function (emps) {
        $("#assignedTo").empty();
        $("#assignedTo").append(`<option value="">-- Select Employee --</option>`);
        $.each(emps, function (i, e) {
            $("#assignedTo").append(`<option value="${e.employeeId}">${e.employeeId} - ${e.name}</option>`);
        });
    });

    // Init DataTable
    var table = $("#tbl_task").DataTable({
        ajax: {
            url: "/Task/GetTasks?appId=" + appId,
            dataSrc: ""
        },
        columns: [
            { data: "taskName" },
            { data: "role" },
            { data: "assignedTo" },
            {
                data: "status",
                render: function (d) {
                    if (d === 1) return `<span class="badge bg-warning">Pending</span>`;
                    if (d === 2) return `<span class="badge bg-info">In Progress</span>`;
                    if (d === 3) return `<span class="badge bg-success">Done</span>`;
                    return `<span class="badge bg-secondary">Unknown</span>`;
                }
            },
            {
                data: "progress",
                render: function (d) {
                    let label = "";
                    switch (d) {
                        case 0: label = "Development"; break;
                        case 20: label = "Testing on Development"; break;
                        case 40: label = "Up to Staging"; break;
                        case 60: label = "Testing on Staging"; break;
                        case 80: label = "Prepared to Production"; break;
                        case 100: label = "Production Done"; break;
                        default: label = d + "%";
                    }
                    return `<span class="badge bg-primary">${label}</span>`;
                }
            },
            {
                data: "id",
                render: function (id) {
                    return `
                        <button class="btn btn-sm btn-warning btn-edit" data-id="${id}"><i class="fa fa-edit"></i></button>
                        <button class="btn btn-sm btn-danger btn-delete" data-id="${id}"><i class="fa fa-trash"></i></button>
                    `;
                }
            }
        ]
    });

    // Save
    $("#btnSaveTask").click(function () {
        var obj = {
            Id: $("#taskId").val() || 0,
            AppId: appId,
            TaskName: $("#taskName").val(),
            Role: $("#taskRole").val(),
            AssignedTo: $("#assignedTo").val(),
            Status: parseInt($("#taskStatus").val()),
            Progress: parseInt($("#taskProgress").val())
        };

        var url = obj.Id == 0 ? "/Task/Create" : "/Task/Update";
        var method = obj.Id == 0 ? "POST" : "PUT";

        $.ajax({
            url: url,
            type: method,
            data: JSON.stringify(obj),
            contentType: "application/json",
            success: function (res) {
                if (res.remarks) {
                    $("#modal-task").modal("hide");
                    table.ajax.reload();
                } else {
                    alert(res.message || "Error saving task");
                }
            }
        });
    });

    // Edit
    $("#tbl_task").on("click", ".btn-edit", function () {
        var id = $(this).data("id");
        $.get("/Task/GetTasks?appId=" + appId, function (tasks) {
            var task = tasks.find(t => t.id == id);
            if (task) {
                $("#taskId").val(task.id);
                $("#taskName").val(task.taskName);
                $("#taskRole").val(task.role);
                $("#assignedTo").val(task.assignedTo);
                $("#taskStatus").val(task.status);
                $("#taskProgress").val(task.progress);
                $("#modal-task").modal("show");
            }
        });
    });

    // Delete
    $("#tbl_task").on("click", ".btn-delete", function () {
        var id = $(this).data("id");
        if (confirm("Delete this task?")) {
            $.ajax({
                url: "/Task/Delete?id=" + id,
                type: "DELETE",
                success: function (res) {
                    if (res.remarks) table.ajax.reload();
                    else alert(res.message || "Delete failed");
                }
            });
        }
    });
});
