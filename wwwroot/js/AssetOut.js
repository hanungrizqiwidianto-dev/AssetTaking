$(document).ready(function() {
    console.log("AssetOut.js loaded");

    // Image preview functionality for new upload
    $('#fotoFile').on('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            // Validate file type
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
            if (!allowedTypes.includes(file.type)) {
                Swal.fire({
                    title: 'Format File Tidak Valid!',
                    text: 'Silakan pilih file gambar dengan format JPG, JPEG, PNG, atau GIF.',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
                $(this).val('');
                $('#imagePreview').hide();
                return;
            }

            // Validate file size (5MB)
            if (file.size > 5 * 1024 * 1024) {
                Swal.fire({
                    title: 'Ukuran File Terlalu Besar!',
                    text: 'Ukuran file maksimal 5MB.',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
                $(this).val('');
                $('#imagePreview').hide();
                return;
            }

            // Show preview
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#previewImg').attr('src', e.target.result);
                $('#imagePreview').show();
            };
            reader.readAsDataURL(file);
        } else {
            $('#imagePreview').hide();
        }
    });

    // Initialize Select2 with AJAX search
    initializeAssetSelect();

    // Handle asset selection change
    $('#assetSelect').on('select2:select', function() {
        const selectedData = $(this).select2('data')[0];
        if (selectedData && selectedData.id) {
            loadAssetDetailFromSelect2(selectedData);
            enableForm();
        } else {
            clearForm();
            disableForm();
        }
    });

    // Handle asset selection clear
    $('#assetSelect').on('select2:clear', function() {
        clearForm();
        disableForm();
    });

    // Handle form reset
    $('#assetOutForm').on('reset', function() {
        setTimeout(function() {
            clearForm();
            disableForm();
            $('#assetSelect').val(null).trigger('change');
        }, 10);
    });

    // Handle form submission
    $('#assetOutForm').on('submit', function(e) {
        e.preventDefault();
        
        const assetInId = $('#assetSelect').val();
        const qty = parseInt($('#qty').val());
        const maxQty = parseInt($('#qty').attr('max'));
        
        if (!assetInId) {
            Swal.fire({
                title: 'Error!',
                text: 'Pilih asset terlebih dahulu',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        if (qty > maxQty) {
            Swal.fire({
                title: 'Error!',
                text: `Qty tidak boleh melebihi stok tersedia (${maxQty})`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Create FormData for file upload
        const formData = new FormData();
        formData.append('AssetInId', parseInt(assetInId));
        formData.append('Qty', qty);
        
        // Add file if selected
        const fileInput = $('#fotoFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('fotoFile', fileInput.files[0]);
        }

        console.log("Submitting asset out with form data");

        // Disable submit button during submission
        $('#submitBtn').prop('disabled', true).text('Processing...');

        $.ajax({
            url: '/api/AssetOut/Create',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                console.log("Asset out response:", response);
                if (response.remarks || response.Remarks) {
                    Swal.fire({
                        title: 'Berhasil!',
                        text: response.message || response.Message,
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(function() {
                        $('#assetOutForm')[0].reset();
                        clearForm();
                        disableForm();
                        $('#assetSelect').val(null).trigger('change');
                        // Refresh notification after successful asset out
                        if (typeof refreshNotifications === 'function') {
                            refreshNotifications();
                        }
                    });
                } else {
                    Swal.fire({
                        title: 'Error!',
                        text: response.message || response.Message,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error("Asset out error:", xhr.responseText);
                let errorMessage = 'Terjadi kesalahan: ' + error;
                
                try {
                    const errorResponse = JSON.parse(xhr.responseText);
                    errorMessage = errorResponse.Message || errorResponse.message || errorMessage;
                } catch (e) {
                    // Keep the default error message
                }

                Swal.fire({
                    title: 'Error!',
                    text: errorMessage,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            },
            complete: function() {
                // Re-enable submit button
                $('#submitBtn').prop('disabled', false).html('<i class="fa fa-save"></i> Submit Asset Out');
            }
        });
    });

    function initializeAssetSelect() {
        $('#assetSelect').select2({
            theme: 'bootstrap-5',
            placeholder: 'Ketik untuk mencari asset...',
            allowClear: true,
            minimumInputLength: 0,
            ajax: {
                url: '/api/AssetOut/SearchAssets',
                dataType: 'json',
                delay: 300,
                data: function (params) {
                    return {
                        term: params.term || '',
                        page: params.page || 1,
                        pageSize: 20
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    
                    return {
                        results: data.results,
                        pagination: {
                            more: data.pagination.more
                        }
                    };
                },
                cache: true
            },
            templateResult: function(asset) {
                if (asset.loading) {
                    return asset.text;
                }
                
                if (!asset.id) {
                    return asset.text;
                }

                var $container = $(
                    "<div class='select2-result-asset clearfix'>" +
                        "<div class='select2-result-asset__meta'>" +
                            "<div class='select2-result-asset__title'></div>" +
                            "<div class='select2-result-asset__description'></div>" +
                        "</div>" +
                    "</div>"
                );

                $container.find('.select2-result-asset__title').text(asset.nomorAsset + ' - ' + asset.namaBarang);
                $container.find('.select2-result-asset__description').text('Kode: ' + (asset.kodeBarang || '-') + ' | Kategori: ' + (asset.kategoriBarang || '-') + ' | Qty: ' + (asset.qty || 0));

                return $container;
            },
            templateSelection: function(asset) {
                if (!asset.id) {
                    return asset.text;
                }
                return asset.nomorAsset + ' - ' + asset.namaBarang + ' (Qty: ' + (asset.qty || 0) + ')';
            },
            escapeMarkup: function(markup) {
                return markup;
            }
        });
    }

    function loadAssetDetailFromSelect2(selectedData) {
        if (selectedData && selectedData.id) {
            $('#nomorAsset').val(selectedData.nomorAsset || '');
            $('#namaBarang').val(selectedData.namaBarang || '');
            $('#kodeBarang').val(selectedData.kodeBarang || '');
            $('#kategoriBarang').val(selectedData.kategoriBarang || '');
            $('#foto').val(selectedData.foto || '');
            $('#stockInfo').text(selectedData.qty || 0);
            $('#qty').attr('max', selectedData.qty || 0);
            
            // Show current photo if exists
            if (selectedData.foto && selectedData.foto !== '') {
                $('#currentImg').attr('src', selectedData.foto);
                $('#currentImagePreview').show();
            } else {
                $('#currentImagePreview').hide();
            }
        }
    }

    function loadAssetDetail(assetId) {
        $.ajax({
            url: `/api/AssetOut/GetAssetInDetail/${assetId}`,
            type: 'GET',
            success: function(response) {
                console.log("Asset detail:", response);
                if (response.remarks || response.Remarks) {
                    const asset = response.data;
                    $('#nomorAsset').val(asset.nomorAsset || '');
                    $('#namaBarang').val(asset.namaBarang || '');
                    $('#kodeBarang').val(asset.kodeBarang || '');
                    $('#kategoriBarang').val(asset.kategoriBarang || '');
                    $('#foto').val(asset.foto || '');
                    $('#stockInfo').text(asset.qty || 0);
                    $('#qty').attr('max', asset.qty || 0);
                    
                    // Show current photo if exists
                    if (asset.foto && asset.foto !== '') {
                        $('#currentImg').attr('src', asset.foto);
                        $('#currentImagePreview').show();
                    } else {
                        $('#currentImagePreview').hide();
                    }
                } else {
                    Swal.fire({
                        title: 'Error!',
                        text: response.message || response.Message,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error("Error loading asset detail:", error);
                Swal.fire({
                    title: 'Error!',
                    text: 'Gagal memuat detail asset',
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    }

    function enableForm() {
        $('#qty').prop('disabled', false);
        $('#nomorAsset').prop('disabled', false);
        $('#namaBarang').prop('disabled', false);
        $('#kodeBarang').prop('disabled', false);
        $('#kategoriBarang').prop('disabled', false);
        $('#foto').prop('disabled', false);
        $('#submitBtn').prop('disabled', false);
    }

    function disableForm() {
        $('#qty').prop('disabled', true);
        $('#nomorAsset').prop('disabled', true);
        $('#namaBarang').prop('disabled', true);
        $('#kodeBarang').prop('disabled', true);
        $('#kategoriBarang').prop('disabled', true);
        $('#foto').prop('disabled', true);
        $('#submitBtn').prop('disabled', true);
    }

    function clearForm() {
        $('#nomorAsset').val('');
        $('#namaBarang').val('');
        $('#kodeBarang').val('');
        $('#kategoriBarang').val('');
        $('#foto').val('');
        $('#qty').val('').removeAttr('max');
        $('#stockInfo').text('-');
        $('#fotoFile').val('');
        $('#imagePreview').hide();
        $('#currentImagePreview').hide();
    }

    // Initialize form state
    disableForm();
});
