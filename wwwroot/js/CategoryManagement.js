$(document).ready(function () {
    // Initialize DataTable
    var table = $('#categoryTable').DataTable({
        processing: true,
        serverSide: false,
        ajax: {
            url: '/api/Category/GetCategories',
            type: 'GET',
            dataSrc: 'data',
            error: function(xhr, error, code) {
                console.error('Error loading categories:', error);
                Swal.fire({
                    title: 'Error!',
                    text: 'Gagal memuat data kategori',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        },
        columns: [
            { 
                data: null,
                className: 'text-center',
                render: function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            { 
                data: 'kategoriBarang',
                defaultContent: '-',
                render: function(data, type, row) {
                    return data || '-';
                }
            },
            { 
                data: 'createdAt', 
                className: 'text-center',
                defaultContent: '-',
                render: function(data, type, row) {
                    return data || '-';
                }
            },
            { 
                data: 'createdBy', 
                className: 'text-center',
                defaultContent: '-',
                render: function(data, type, row) {
                    return data || '-';
                }
            },
            { 
                data: 'modifiedAt', 
                className: 'text-center',
                defaultContent: '-',
                render: function(data, type, row) {
                    return data || '-';
                }
            },
            { 
                data: 'modifiedBy', 
                className: 'text-center',
                defaultContent: '-',
                render: function(data, type, row) {
                    return data || '-';
                }
            },
            {
                data: null,
                className: 'text-center',
                render: function (data, type, row) {
                    return `
                        <div class="btn-group" role="group">
                            <button type="button" class="btn btn-sm btn-warning btn-edit" 
                                    data-id="${row.id}" data-kategori="${row.kategoriBarang}"
                                    title="Edit Kategori">
                                <i class="fa fa-edit"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-danger btn-delete" 
                                    data-id="${row.id}" data-kategori="${row.kategoriBarang}"
                                    title="Hapus Kategori">
                                <i class="fa fa-trash"></i>
                            </button>
                        </div>
                    `;
                }
            }
        ],
        order: [[1, 'asc']],
        pageLength: 25,
        responsive: true,
        language: {
            processing: "Memuat data...",
            search: "Search:",
            lengthMenu: "Tampilkan _MENU_ data per halaman",
            info: "Menampilkan _START_ sampai _END_ dari _TOTAL_ data",
            infoEmpty: "Menampilkan 0 sampai 0 dari 0 data",
            infoFiltered: "(difilter dari _MAX_ total data)",
            paginate: {
                first: "Pertama",
                last: "Terakhir",
                next: "Selanjutnya",
                previous: "Sebelumnya"
            },
            emptyTable: "Tidak ada data kategori"
        }
    });

    // Add Category Button
    $('#btnAddCategory').on('click', function () {
        showCategoryModal();
    });

    // Edit Category Button
    $(document).on('click', '.btn-edit', function () {
        const categoryId = $(this).data('id');
        const kategoriBarang = $(this).data('kategori');
        showCategoryModal(categoryId, kategoriBarang);
    });

    // Delete Category Button
    $(document).on('click', '.btn-delete', function () {
        const categoryId = $(this).data('id');
        const kategoriBarang = $(this).data('kategori');
        deleteCategory(categoryId, kategoriBarang);
    });

    // Show Category Modal (Add/Edit)
    function showCategoryModal(categoryId = null, kategoriBarang = '') {
        const isEdit = categoryId !== null;
        const modalTitle = isEdit ? 'Edit Kategori Asset' : 'Tambah Kategori Asset';
        const buttonText = isEdit ? 'Update Kategori' : 'Simpan Kategori';

        Swal.fire({
            title: modalTitle,
            html: `
                <form id="categoryForm">
                    <div class="mb-3">
                        <label for="kategoriBarang" class="form-label text-start d-block">
                            <strong>Kategori Barang</strong> <span class="text-danger">*</span>
                        </label>
                        <input type="text" class="form-control" id="kategoriBarang" 
                               placeholder="Masukkan nama kategori barang" value="${kategoriBarang}" required>
                        <div class="form-text text-muted text-start">
                            <i class="fa fa-info-circle me-1"></i>
                            Contoh: RnD, SPAREPART, OFFICE EQUIPMENT, dll
                        </div>
                    </div>
                </form>
            `,
            showCancelButton: true,
            confirmButtonText: buttonText,
            cancelButtonText: 'Batal',
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            width: '500px',
            preConfirm: () => {
                const kategori = $('#kategoriBarang').val().trim();

                if (!kategori) {
                    Swal.showValidationMessage('Kategori barang harus diisi!');
                    return false;
                }

                if (kategori.length > 50) {
                    Swal.showValidationMessage('Kategori barang maksimal 50 karakter!');
                    return false;
                }

                return {
                    kategoriBarang: kategori
                };
            }
        }).then((result) => {
            if (result.isConfirmed) {
                if (isEdit) {
                    updateCategory(categoryId, result.value);
                } else {
                    createCategory(result.value);
                }
            }
        });
    }

    // Create Category
    async function createCategory(formData) {
        try {
            Swal.fire({
                title: 'Menyimpan...',
                text: 'Sedang menyimpan kategori baru',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            const response = await fetch('/api/Category/CreateCategory', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();

            if (result.success) {
                Swal.fire({
                    title: 'Berhasil!',
                    text: result.message,
                    icon: 'success',
                    confirmButtonText: 'OK'
                }).then(() => {
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
            console.error('Error creating category:', error);
            Swal.fire({
                title: 'Error!',
                text: 'Terjadi kesalahan saat menyimpan kategori',
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    }

    // Update Category
    async function updateCategory(categoryId, formData) {
        try {
            Swal.fire({
                title: 'Mengupdate...',
                text: 'Sedang mengupdate kategori',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            const response = await fetch(`/api/Category/UpdateCategory/${categoryId}`, {
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
                    text: result.message,
                    icon: 'success',
                    confirmButtonText: 'OK'
                }).then(() => {
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
            console.error('Error updating category:', error);
            Swal.fire({
                title: 'Error!',
                text: 'Terjadi kesalahan saat mengupdate kategori',
                icon: 'error',
                confirmButtonText: 'OK'
            });
        }
    }

    // Delete Category
    function deleteCategory(categoryId, kategoriBarang) {
        Swal.fire({
            title: 'Konfirmasi Hapus',
            html: `
                <div class="text-start">
                    <p><strong>Kategori:</strong> ${kategoriBarang}</p>
                    <div class="alert alert-warning mt-3">
                        <i class="fa fa-exclamation-triangle me-2"></i>
                        <strong>Perhatian!</strong><br>
                        Kategori yang sedang digunakan pada asset tidak dapat dihapus.
                    </div>
                    <p class="text-muted small">Operasi ini tidak dapat dibatalkan.</p>
                </div>
            `,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonText: 'Ya, Hapus!',
            cancelButtonText: 'Batal',
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            width: '500px'
        }).then(async (result) => {
            if (result.isConfirmed) {
                try {
                    Swal.fire({
                        title: 'Menghapus...',
                        text: 'Sedang menghapus kategori',
                        allowOutsideClick: false,
                        didOpen: () => {
                            Swal.showLoading();
                        }
                    });

                    const response = await fetch(`/api/Category/DeleteCategory/${categoryId}`, {
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
                    console.error('Error deleting category:', error);
                    Swal.fire({
                        title: 'Error!',
                        text: 'Terjadi kesalahan saat menghapus kategori',
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            }
        });
    }
});