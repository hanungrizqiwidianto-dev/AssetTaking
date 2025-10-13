$(document).ready(function () {
    var table = $("#tbl_register").DataTable({
        ajax: {
            url: "/api/Register/GetAll", // API lokal
            dataSrc: ""
        },
        "columnDefs": [
            { "className": "dt-center", "targets": [0, 1, 2, 4, 5, 6] },
            { "className": "dt-nowrap", "targets": '_all' }
        ],
        scrollX: true,
        columns: [
            { data: "Id" },
            { data: "AppName" },
            { data: "Division" },
            { data: "Description" },
            {
                data: "Status",
                render: function (data) {
                    let text = "";
                    if (data === 1) {
                        text = `<span class="badge bg-warning">Pending</span>`;
                    } else if (data === 2) {
                        text = `<span class="badge bg-primary">Approved Div Head</span>`;
                    } else if (data === 3) {
                        text = `<span class="badge bg-danger">Rejected Div Head</span>`;
                    } else if (data === 4) {
                        text = `<span class="badge bg-success">Approved IT Head</span>`;
                    } else if (data === 5) {
                        text = `<span class="badge bg-danger">Rejected IT Head</span>`;
                    } else {
                        text = `<span class="badge bg-secondary">Unknown</span>`;
                    }
                    return text;
                }
            },
            { data: "CreatedBy" },
            {
                data: "CreatedAt",
                render: function (data) {
                    return moment(data).format("DD/MM/YYYY HH:mm");
                }
            },
            {
                data: "Id",
                orderable: false,
                render: function (data, type, row) {
                    let roleId = $("#hd_role").val(); // ambil role dari hidden input
                    let disabled = "";

                    // tombol aktif hanya jika status pending dan role 1 atau 4
                    if (row.Status !== 1 || (roleId != "1" && roleId != "4")) {
                        disabled = "disabled";
                    }

                    return `<div class="btn-group">
                        <a href="/Register/Edit?id=${data}" 
                           class="btn btn-sm btn-warning ${disabled}" 
                           ${disabled}>
                            <i class="fa fa-edit"></i>
                        </a>
                    </div>`;
                }
            }

        ]
    });

    // Filter dropdown berdasarkan status
    table.on('init', function () {
        table
            .columns(4) // kolom status
            .every(function () {
                var column = this;
                var select = $('<select class="form-control form-control-sm" style="width:200px; margin-left:10px;"><option value="">-- STATUS --</option></select>')
                    .appendTo($("#tbl_register_filter.dataTables_filter"))
                    .on('change', function () {
                        var val = $.fn.dataTable.util.escapeRegex($(this).val());
                        column.search(val ? '^' + val + '$' : '', true, false).draw();
                    });

                // isi opsi filter sesuai status
                select.append('<option value="Pending">Pending</option>');
                select.append('<option value="Approved Div Head">Approved Div Head</option>');
                select.append('<option value="Rejected Div Head">Rejected Div Head</option>');
                select.append('<option value="Approved IT Head">Approved IT Head</option>');
                select.append('<option value="Rejected IT Head">Rejected IT Head</option>');
            });
    });
});
