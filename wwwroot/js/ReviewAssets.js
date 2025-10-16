$(document).ready(function () {
    var table = $("#tbl_assets").DataTable({
        ajax: {
            url: "/api/Review/GetAssets",
            dataSrc: ""
        },
        "columnDefs": [
            { "className": "dt-center", "targets": [0, 1, 3, 4] },
            { "className": "dt-nowrap", "targets": '_all' }
        ],
        scrollX: true,
        columns: [
            { data: "kodeBarang" },
            { data: "nomorAsset" },
            { data: "namaBarang" },
            { data: "kategoriBarang" },
            { data: "qty" },
            {
                data: "createdAt",
                render: function (data) {
                    return data ? moment(data).format("DD/MM/YYYY HH:mm") : "-";
                }
            },
            {
                data: null,
                orderable: false,
                render: function (data, type, row) {
                    return `<div class="btn-group" role="group">
                                <button type="button" class="btn btn-sm btn-info btn-detail" 
                                   data-kode="${row.kodeBarang}" 
                                   data-nomor="${row.nomorAsset}"
                                   title="View Details">
                                   <i class="fa fa-eye"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-success btn-generate-qr me-1" 
                                   data-kode="${row.kodeBarang}" 
                                   data-nomor="${row.nomorAsset}"
                                   data-nama="${row.namaBarang}"
                                   data-kategori="${row.kategoriBarang}"
                                   data-qty="${row.qty}"
                                   title="Generate QR Code">
                                   <i class="fa fa-qrcode"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-danger btn-delete-all" 
                                   data-kode="${row.kodeBarang}" 
                                   data-nomor="${row.nomorAsset}"
                                   data-nama="${row.namaBarang}"
                                   title="Delete All Transactions">
                                   <i class="fa fa-trash"></i>
                                </button>
                            </div>`;
                }
            }
        ]
    });

    // Handle detail button click
    $('#tbl_assets tbody').on('click', '.btn-detail', function () {
        var kodeBarang = $(this).data('kode');
        var nomorAsset = $(this).data('nomor');
        
        // Clear previous data
        $("#assetDetailsContainer").empty();
        
        // Show loading
        $("#assetDetailsContainer").html('<div class="text-center py-4"><i class="fa fa-spinner fa-spin fa-2x"></i><br><small>Loading asset details...</small></div>');
        
        // Show modal
        $('#assetDetailModal').modal('show');
        
        // Fetch asset details
        $.ajax({
            url: `/api/Review/GetAssetDetails?kodeBarang=${encodeURIComponent(kodeBarang)}&nomorAsset=${encodeURIComponent(nomorAsset)}`,
            type: 'GET',
            success: function (data) {
                $("#assetDetailsContainer").empty();
                
                if (data && data.length > 0) {
                    var html = '<div class="row">';
                    
                    data.forEach(function (asset, index) {
                        var statusBadge = '';
                        if (asset.status === 1) {
                            statusBadge = '<span class="badge bg-success">Asset In</span>';
                        } else if (asset.status === 2) {
                            statusBadge = '<span class="badge bg-warning">Asset Out</span>';
                        } else {
                            statusBadge = '<span class="badge bg-secondary">Unknown</span>';
                        }
                        
                        var fotoHtml = asset.foto ? 
                            `<img src="${asset.foto}" alt="Asset Photo" class="img-fluid rounded" style="max-height: 150px;">` : 
                            '<div class="bg-light rounded d-flex align-items-center justify-content-center" style="height: 150px;"><i class="fa fa-image fa-2x text-muted"></i></div>';
                            
                        html += `
                            <div class="col-md-6 mb-4">
                                <div class="block block-rounded h-100">
                                    <div class="block-header block-header-default">
                                        <h4 class="block-title">Asset #${asset.id}</h4>
                                        <div class="block-options">
                                            ${statusBadge}
                                        </div>
                                    </div>
                                    <div class="block-content">
                                        <div class="row">
                                            <div class="col-4">
                                                ${fotoHtml}
                                            </div>
                                            <div class="col-8">
                                                <table class="table table-borderless table-sm">
                                                    <tr>
                                                        <td><strong>Nama Barang:</strong></td>
                                                        <td>${asset.namaBarang || '-'}</td>
                                                    </tr>
                                                    <tr>
                                                        <td><strong>Kode Barang:</strong></td>
                                                        <td><span class="badge bg-primary">${asset.kodeBarang || '-'}</span></td>
                                                    </tr>
                                                    <tr>
                                                        <td><strong>Nomor Asset:</strong></td>
                                                        <td><span class="badge bg-info">${asset.nomorAsset || '-'}</span></td>
                                                    </tr>
                                                    <tr>
                                                        <td><strong>Kategori:</strong></td>
                                                        <td>${asset.kategoriBarang || '-'}</td>
                                                    </tr>
                                                    <tr>
                                                        <td><strong>Quantity:</strong></td>
                                                        <td><span class="badge bg-dark">${asset.qty || 0}</span></td>
                                                    </tr>
                                                    <tr>
                                                        <td><strong>Tanggal Masuk:</strong></td>
                                                        <td>${asset.tanggalMasuk ? moment(asset.tanggalMasuk).format("DD/MM/YYYY") : '-'}</td>
                                                    </tr>
                                                </table>
                                            </div>
                                        </div>
                                        <hr>
                                        <div class="row">
                                            <div class="col-6">
                                                <small class="text-muted">
                                                    <strong>Created:</strong><br>
                                                    ${asset.createdAt ? moment(asset.createdAt).format("DD/MM/YYYY HH:mm") : '-'}<br>
                                                    by: ${asset.createdBy || '-'}
                                                </small>
                                            </div>
                                            <div class="col-6">
                                                <small class="text-muted">
                                                    <strong>Modified:</strong><br>
                                                    ${asset.modifiedAt ? moment(asset.modifiedAt).format("DD/MM/YYYY HH:mm") : '-'}<br>
                                                    by: ${asset.modifiedBy || '-'}
                                                </small>
                                            </div>
                                        </div>
                                        <div class="text-end mt-3">
                                            <button type="button" class="btn btn-sm btn-warning btn-edit-asset" 
                                                data-id="${asset.id}"
                                                data-nama="${asset.namaBarang}"
                                                data-nomor="${asset.nomorAsset}" 
                                                data-kode="${asset.kodeBarang}"
                                                data-kategori="${asset.kategoriBarang}"
                                                data-qty="${asset.qty}"
                                                data-foto="${asset.foto || ''}"
                                                title="Edit Asset">
                                                <i class="fa fa-edit"></i> Edit
                                            </button>
                                            <button type="button" class="btn btn-sm btn-success btn-generate-qr-detail ms-2" 
                                                data-nama="${asset.namaBarang}"
                                                data-nomor="${asset.nomorAsset}"
                                                data-kode="${asset.kodeBarang}"
                                                data-kategori="${asset.kategoriBarang}"
                                                data-qty="${asset.qty}"
                                                title="Generate QR Code">
                                                <i class="fa fa-qrcode"></i> Generate QR
                                            </button>
                                            <button type="button" class="btn btn-sm btn-danger btn-delete-asset ms-2" 
                                                data-id="${asset.id}"
                                                data-nama="${asset.namaBarang}"
                                                title="Delete Asset">
                                                <i class="fa fa-trash"></i> Delete
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        `;
                    });
                    
                    html += '</div>';
                    $("#assetDetailsContainer").html(html);
                } else {
                    $("#assetDetailsContainer").html('<div class="text-center py-5"><i class="fa fa-info-circle fa-2x text-muted"></i><br><h5 class="mt-2">No asset details found</h5></div>');
                }
            },
            error: function (xhr, status, error) {
                console.error('Error fetching asset details:', error);
                $("#assetDetailsContainer").html('<div class="text-center py-5 text-danger"><i class="fa fa-exclamation-triangle fa-2x"></i><br><h5 class="mt-2">Error loading asset details</h5><p>Please try again later.</p></div>');
            }
        });
    });

    // Handle edit button click
    $(document).on('click', '.btn-edit-asset', function() {
        const assetId = $(this).data('id');
        const assetData = {
            id: assetId,
            namaBarang: $(this).data('nama'),
            nomorAsset: $(this).data('nomor'),
            kodeBarang: $(this).data('kode'),
            kategoriBarang: $(this).data('kategori'),
            qty: $(this).data('qty'),
            foto: $(this).data('foto')
        };
        
        showEditAssetModal(assetData);
    });

    // Handle delete button click
    $(document).on('click', '.btn-delete-asset', function() {
        const assetId = $(this).data('id');
        const assetNama = $(this).data('nama');
        
        deleteAsset(assetId, assetNama);
    });

    // Handle delete all button click (delete semua transaksi berdasarkan kode barang dan nomor asset)
    $(document).on('click', '.btn-delete-all', function() {
        const kodeBarang = $(this).data('kode');
        const nomorAsset = $(this).data('nomor');
        const namaBarang = $(this).data('nama');
        
        deleteAllAssetTransactions(kodeBarang, nomorAsset, namaBarang);
    });

    // Handle generate QR button click (from main table)
    $(document).on('click', '.btn-generate-qr', function() {
        const namaBarang = $(this).data('nama');
        const nomorAsset = $(this).data('nomor');
        const kodeBarang = $(this).data('kode');
        const kategoriBarang = $(this).data('kategori');
        const qty = $(this).data('qty');
        
        generateQRFromAsset(namaBarang, nomorAsset, kodeBarang, kategoriBarang, qty);
    });

    // Handle generate QR button click (from modal detail)
    $(document).on('click', '.btn-generate-qr-detail', function() {
        const namaBarang = $(this).data('nama');
        const nomorAsset = $(this).data('nomor');
        const kodeBarang = $(this).data('kode');
        const kategoriBarang = $(this).data('kategori');
        const qty = $(this).data('qty');
        
        generateQRFromAsset(namaBarang, nomorAsset, kodeBarang, kategoriBarang, qty);
    });

    // Show edit asset modal
    function showEditAssetModal(assetData) {
        Swal.fire({
            title: 'Edit Asset Quantity',
            html: `
                <form id="editAssetForm">
                    <div class="row">
                        <div class="col-12 mb-3">
                            <label class="form-label text-start w-100">Nama Barang</label>
                            <input type="text" class="form-control" value="${assetData.namaBarang}" disabled readonly>
                            <small class="text-muted">Field ini tidak dapat diedit</small>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-6 mb-3">
                            <label class="form-label text-start w-100">Nomor Asset</label>
                            <input type="text" class="form-control" value="${assetData.nomorAsset}" disabled readonly>
                            <small class="text-muted">Field ini tidak dapat diedit</small>
                        </div>
                        <div class="col-6 mb-3">
                            <label class="form-label text-start w-100">Kode Barang</label>
                            <input type="text" class="form-control" value="${assetData.kodeBarang}" disabled readonly>
                            <small class="text-muted">Field ini tidak dapat diedit</small>
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-6 mb-3">
                            <label class="form-label text-start w-100">Kategori</label>
                            <select class="form-control" disabled readonly>
                                <option value="${assetData.kategoriBarang}" selected>
                                    ${assetData.kategoriBarang === 'SPAREPART' ? 'Spare Part' : 
                                      assetData.kategoriBarang === 'RnD' ? 'RnD Assets' : assetData.kategoriBarang}
                                </option>
                            </select>
                            <small class="text-muted">Field ini tidak dapat diedit</small>
                        </div>
                        <div class="col-6 mb-3">
                            <label class="form-label text-start w-100">Quantity <span class="text-primary">*</span></label>
                            <input type="number" class="form-control" id="edit_qty" value="${assetData.qty}" min="1" required>
                            <small class="text-primary">Hanya field ini yang dapat diedit</small>
                        </div>
                    </div>
                    <div class="alert alert-info mt-3">
                        <i class="fa fa-info-circle me-2"></i>
                        <strong>Catatan:</strong> Untuk keamanan data, hanya quantity yang dapat diedit pada halaman review.
                    </div>
                </form>
            `,
            showCancelButton: true,
            confirmButtonText: 'Update Quantity',
            cancelButtonText: 'Batal',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            width: '600px',
            preConfirm: () => {
                const qty = parseInt($('#edit_qty').val());

                // Validate quantity
                if (!qty || qty < 1) {
                    Swal.showValidationMessage('Quantity harus diisi dengan nilai minimal 1');
                    return false;
                }

                return {
                    qty: qty
                };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                updateAsset(assetData.id, result.value);
            }
        });
    }

    // Update asset function
    async function updateAsset(assetId, formData) {
        try {
            Swal.fire({
                title: 'Updating...',
                text: 'Sedang mengupdate quantity asset',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            const response = await fetch(`/api/Review/UpdateAsset/${assetId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                Swal.fire({
                    title: 'Berhasil!',
                    text: 'Quantity asset berhasil diupdate',
                    icon: 'success',
                    confirmButtonText: 'OK'
                }).then(() => {
                    // Refresh table
                    table.ajax.reload();
                    // Close detail modal if open
                    $('#assetDetailModal').modal('hide');
                });
            } else {
                Swal.fire({
                    title: 'Gagal!',
                    text: result.message,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        } catch (error) {
            console.error('Error updating asset:', error);
            Swal.fire({
                title: 'Error!',
                text: 'Terjadi kesalahan saat mengupdate quantity asset',
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    }

    // Delete asset function
    function deleteAsset(assetId, assetNama) {
        Swal.fire({
            title: 'Konfirmasi Hapus Transaksi',
            html: `
                <div class="text-start">
                    <p><strong>Asset:</strong> ${assetNama}</p>
                    <div class="alert alert-warning mt-3">
                        <i class="fa fa-exclamation-triangle me-2"></i>
                        <strong>Perhatian!</strong><br>
                        Menghapus transaksi ini akan:<br>
                        • Menghapus record di TblTAssets<br>
                        • Mengurangi quantity di table transaksi terkait (Asset In/Out)<br>
                        • Menyesuaikan balance asset secara otomatis
                    </div>
                    <p class="text-muted small">Operasi ini tidak dapat dibatalkan.</p>
                </div>
            `,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Ya, Hapus Transaksi!',
            cancelButtonText: 'Batal',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            width: '500px'
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    Swal.fire({
                        title: 'Processing...',
                        text: 'Sedang menghapus transaksi dan menyesuaikan quantity',
                        allowOutsideClick: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    const response = await fetch(`/api/Review/DeleteAsset/${assetId}`, {
                        method: 'DELETE'
                    });

                    const result = await response.json();

                    if (result.success) {
                        Swal.fire({
                            title: 'Berhasil!',
                            text: result.message,
                            icon: 'success',
                            confirmButtonText: 'OK'
                        }).then(() => {
                            // Refresh table
                            table.ajax.reload();
                            // Close detail modal if open
                            $('#assetDetailModal').modal('hide');
                        });
                    } else {
                        Swal.fire({
                            title: 'Gagal!',
                            text: result.message,
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                } catch (error) {
                    console.error('Error deleting asset:', error);
                    Swal.fire({
                        title: 'Error!',
                        text: 'Terjadi kesalahan saat menghapus transaksi',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    }

    // Delete all asset transactions function
    function deleteAllAssetTransactions(kodeBarang, nomorAsset, namaBarang) {
        Swal.fire({
            title: 'Konfirmasi Hapus Semua Transaksi',
            html: `
                <div class="text-start">
                    <p><strong>Asset:</strong> ${namaBarang}</p>
                    <p><strong>Kode Barang:</strong> ${kodeBarang}</p>
                    <p><strong>Nomor Asset:</strong> ${nomorAsset}</p>
                    <div class="alert alert-danger mt-3">
                        <i class="fa fa-exclamation-triangle me-2"></i>
                        <strong>PERINGATAN!</strong><br>
                        Operasi ini akan menghapus:<br>
                        • <strong>SEMUA</strong> transaksi di TblTAssets<br>
                        • <strong>SEMUA</strong> record di TblMAssetIn<br>
                        • <strong>SEMUA</strong> record di TblMAssetOut<br>
                        untuk asset ini
                    </div>
                    <p class="text-danger small"><strong>Operasi ini tidak dapat dibatalkan dan akan menghapus seluruh riwayat asset!</strong></p>
                </div>
            `,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Ya, Hapus Semua!',
            cancelButtonText: 'Batal',
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            width: '600px'
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    Swal.fire({
                        title: 'Processing...',
                        text: 'Sedang menghapus semua transaksi asset',
                        allowOutsideClick: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    const response = await fetch(`/api/Review/DeleteAllAssetTransactions?kodeBarang=${encodeURIComponent(kodeBarang)}&nomorAsset=${encodeURIComponent(nomorAsset)}`, {
                        method: 'DELETE'
                    });

                    const result = await response.json();

                    if (result.success) {
                        Swal.fire({
                            title: 'Berhasil!',
                            text: result.message,
                            icon: 'success',
                            confirmButtonText: 'OK'
                        }).then(() => {
                            // Refresh table
                            table.ajax.reload();
                        });
                    } else {
                        Swal.fire({
                            title: 'Gagal!',
                            text: result.message,
                            icon: 'error',
                            confirmButtonText: 'OK'
                        });
                    }
                } catch (error) {
                    console.error('Error deleting all asset transactions:', error);
                    Swal.fire({
                        title: 'Error!',
                        text: 'Terjadi kesalahan saat menghapus semua transaksi',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    }

    // Function untuk generate QR dari asset data
    function generateQRFromAsset(namaBarang, nomorAsset, kodeBarang, kategoriBarang, qty) {
        // Encode parameters untuk URL
        const params = new URLSearchParams({
            nama: namaBarang || '',
            nomor: nomorAsset || '',
            kode: kodeBarang || '',
            kategori: kategoriBarang || '',
            qty: qty || '1',
            fromReview: 'true',  // Flag untuk menandakan generate dari review
            fromSerial: 'false'  // Flag untuk menandakan bukan dari serial specific
        });
        
        // Redirect ke halaman Generate QR dengan parameter
        window.location.href = `/Asset/GenerateQR?${params.toString()}`;
    }
});