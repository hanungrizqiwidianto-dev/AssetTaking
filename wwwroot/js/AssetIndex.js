$(document).ready(function () {
    var table = $("#tbl_apps").DataTable({
        processing: true,
        serverSide: false,
        ajax: {
            url: "/Task/GetApps",   // HARUS absolute
            type: "GET",
            dataSrc: function (json) {
                console.log("Response dari server:", json);
                return json;
            }
        },
        columns: [
            { data: "appName" },
            { data: "division" },
            { data: "description" },
            {
                data: "status",
                render: function (d) {
                    if (d === 4) return `<span class="badge bg-success">Approved IT Head</span>`;
                    return `<span class="badge bg-secondary">Unknown</span>`;
                }
            },
            {
                data: "id",
                render: function (id) {
                    return `<a href="/Task/Detail?appId=${id}" class="btn btn-sm btn-primary"><i class="fa fa-tasks"></i></a>`;
                }
            }
        ],
        error: function (xhr, error, thrown) {
            console.error("DataTable AJAX error:", xhr.responseText);
        }
    });

});
