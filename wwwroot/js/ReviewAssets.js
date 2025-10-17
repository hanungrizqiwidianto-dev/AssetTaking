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
                                <button type="button" class="btn btn-sm btn-warning btn-edit" 
                                   data-id="${row.id}" 
                                   data-kode="${row.kodeBarang}" 
                                   data-nomor="${row.nomorAsset}"
                                   data-nama="${row.namaBarang}"
                                   data-kategori="${row.kategoriBarang}"
                                   data-qty="${row.qty}"
                                   data-state="${row.state || ''}"
                                   data-district="${row.district || ''}"
                                   title="Edit Asset">
                                   <i class="fa fa-edit"></i>
                                </button>
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
                                   data-state="${row.state || ''}"
                                   data-district="${row.district || ''}"
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

    // Handle detail button click - redirect to detail page
    $('#tbl_assets tbody').on('click', '.btn-detail', function () {
        var kodeBarang = $(this).data('kode');
        var nomorAsset = $(this).data('nomor');
        
        // Redirect to detail page
        window.location.href = `/Review/Detail?kode=${encodeURIComponent(kodeBarang)}&nomor=${encodeURIComponent(nomorAsset)}`;
    });

    // Handle edit button click
    $(document).on('click', '.btn-edit', function() {
        const assetData = {
            id: $(this).data('id'),
            namaBarang: $(this).data('nama'),
            nomorAsset: $(this).data('nomor'),
            kodeBarang: $(this).data('kode'),
            kategoriBarang: $(this).data('kategori'),
            qty: $(this).data('qty'),
            state: $(this).data('state'),
            district: $(this).data('district')
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
        const state = $(this).data('state');
        const district = $(this).data('district');
        
        generateQRFromAsset(namaBarang, nomorAsset, kodeBarang, kategoriBarang, qty, state, district);
    });

    // Handle generate QR button click (from modal detail)
    $(document).on('click', '.btn-generate-qr-detail', function() {
        const namaBarang = $(this).data('nama');
        const nomorAsset = $(this).data('nomor');
        const kodeBarang = $(this).data('kode');
        const kategoriBarang = $(this).data('kategori');
        const qty = $(this).data('qty');
        const state = $(this).data('state');
        const district = $(this).data('district');
        
        generateQRFromAsset(namaBarang, nomorAsset, kodeBarang, kategoriBarang, qty, state, district);
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
                        Operasi ini akan menghapus semua data asset terkait.<br>
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
    function generateQRFromAsset(namaBarang, nomorAsset, kodeBarang, kategoriBarang, qty, state, district) {
        // Encode parameters untuk URL
        const params = new URLSearchParams({
            nama: namaBarang || '',
            nomor: nomorAsset || '',
            kode: kodeBarang || '',
            kategori: kategoriBarang || '',
            qty: qty || '1',
            state: state || '',
            district: district || '',
            fromReview: 'true',  // Flag untuk menandakan generate dari review
            fromSerial: 'false'  // Flag untuk menandakan bukan dari serial specific
        });
        
        // Redirect ke halaman Generate QR dengan parameter
        window.location.href = `/Asset/GenerateQR?${params.toString()}`;
    }
});