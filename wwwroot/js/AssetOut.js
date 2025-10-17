$(document).ready(function() {
    console.log("AssetOut.js loaded");

    // Load states and districts
    loadStates();
    loadDistricts();

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
            loadSerialNumbers(selectedData.id);
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
        $('#serialNumberSelect').val(null).trigger('change');
    });

    // Handle quantity input change
    $('#qty').on('input change', function() {
        updateFormValidation();
    });

    // Handle form reset
    $('#assetOutForm').on('reset', function() {
        setTimeout(function() {
            clearForm();
            disableForm();
            $('#assetSelect').val(null).trigger('change');
            $('#serialNumberSelect').val(null).trigger('change');
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

        // Validate serial numbers selection
        const selectedSerialsValidation = $('#serialNumberSelect').val();
        if (!selectedSerialsValidation || selectedSerialsValidation.length !== qty) {
            Swal.fire({
                title: 'Error!',
                text: `Pilih ${qty} serial numbers sesuai dengan quantity yang diinginkan`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Validate state selection
        const state = $('#state').val();
        if (!state) {
            Swal.fire({
                title: 'Error!',
                text: 'State/Kondisi harus diisi',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Validate district out selection
        const dstrctOut = $('#dstrctOut').val();
        if (!dstrctOut) {
            Swal.fire({
                title: 'Error!',
                text: 'District Out harus diisi',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Create FormData for file upload
        const formData = new FormData();
        formData.append('AssetInId', parseInt(assetInId));
        formData.append('Qty', qty);
        
        // Add state and district
        const stateText = $('#state option:selected').text();
        
        formData.append('State', stateText); // Send state text, not ID
        formData.append('DstrctOut', dstrctOut);
        
        // Add selected serial numbers
        const selectedSerials = $('#serialNumberSelect').val();
        selectedSerials.forEach((serialId, index) => {
            formData.append(`SelectedSerials[${index}]`, serialId);
        });
        
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
        $('#serialNumberSelect').prop('disabled', false);
        // Don't enable submit button here - let updateFormValidation handle it
        updateFormValidation();
    }

    function disableForm() {
        $('#qty').prop('disabled', true);
        $('#nomorAsset').prop('disabled', true);
        $('#namaBarang').prop('disabled', true);
        $('#kodeBarang').prop('disabled', true);
        $('#kategoriBarang').prop('disabled', true);
        $('#foto').prop('disabled', true);
        $('#serialNumberSelect').prop('disabled', true);
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
        $('#serialNumberSelect').val(null).trigger('change');
    }

    // Initialize form state
    disableForm();

    // Excel Upload Functionality
    $('#excelUploadForm').on('submit', function(e) {
        e.preventDefault();
        
        const fileInput = $('#excelFile')[0];
        if (!fileInput.files.length) {
            Swal.fire({
                title: 'Peringatan!',
                text: 'Silakan pilih file Excel terlebih dahulu',
                icon: 'warning'
            });
            return;
        }

        const file = fileInput.files[0];
        const maxSize = 10 * 1024 * 1024; // 10MB
        
        if (file.size > maxSize) {
            Swal.fire({
                title: 'File Terlalu Besar!',
                text: 'Ukuran file maksimal 10MB',
                icon: 'error'
            });
            return;
        }

        // Validate file extension
        const allowedExtensions = ['.xlsx', '.xls'];
        const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
        
        if (!allowedExtensions.includes(fileExtension)) {
            Swal.fire({
                title: 'Format File Tidak Valid!',
                text: 'Hanya file Excel (.xlsx, .xls) yang diperbolehkan',
                icon: 'error'
            });
            return;
        }

        // Show confirmation
        Swal.fire({
            title: 'Konfirmasi Upload Asset Out',
            html: `
                <div class="text-left">
                    <p><strong>File:</strong> ${file.name}</p>
                    <p><strong>Ukuran:</strong> ${(file.size / 1024 / 1024).toFixed(2)} MB</p>
                    <p><strong>Format:</strong> ${fileExtension.toUpperCase()}</p>
                </div>
                <div class="alert alert-warning mt-3">
                    <i class="fa fa-exclamation-triangle"></i> 
                    <strong>Perhatian:</strong> Proses ini akan mengurangi stok asset sesuai dengan data di Excel.
                </div>
            `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="fa fa-upload"></i> Ya, Upload!',
            cancelButtonText: '<i class="fa fa-times"></i> Batal',
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d'
        }).then((result) => {
            if (result.isConfirmed) {
                uploadExcelFile();
            }
        });
    });

    function uploadExcelFile() {
        const formData = new FormData($('#excelUploadForm')[0]);
        
        // Show progress
        $('#uploadExcelBtn').prop('disabled', true);
        $('#uploadProgress').show();

        $.ajax({
            url: $('#excelUploadForm').attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function(response) {
                // Hide progress
                $('#uploadProgress').hide();
                $('#uploadExcelBtn').prop('disabled', false);
                
                if (response.success) {
                    // Show success or warning SweetAlert with statistics
                    let icon = response.status; // 'success' or 'warning'
                    let confirmButtonText = 'OK';
                    let htmlContent = '';
                    
                    // Create detailed message with statistics
                    if (response.successCount > 0) {
                        htmlContent += `
                            <div class="alert alert-success">
                                <i class="fa fa-check-circle"></i> 
                                <strong>Berhasil memproses ${response.successCount} data asset out</strong>
                            </div>
                        `;
                    }
                    
                    if (response.errorCount > 0) {
                        htmlContent += `
                            <div class="alert alert-warning">
                                <i class="fa fa-exclamation-triangle"></i> 
                                <strong>Ditemukan ${response.errorCount} error</strong>
                            </div>
                        `;
                        confirmButtonText = 'Lihat Detail';
                    }
                    
                    // Add detailed message
                    htmlContent += `<div class="text-left mt-2">${response.message.replace(/\n/g, '<br>')}</div>`;
                    
                    Swal.fire({
                        title: response.title,
                        html: htmlContent,
                        icon: icon,
                        confirmButtonText: confirmButtonText,
                        confirmButtonColor: icon === 'success' ? '#28a745' : '#ffc107',
                        width: '600px'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            // Reset form after successful upload
                            $('#excelUploadForm')[0].reset();
                            $('#excelFile').removeClass('is-invalid');
                            $('#uploadResults').hide();
                            
                            // Show additional success message if all data imported successfully
                            if (response.successCount > 0 && response.errorCount === 0) {
                                Swal.fire({
                                    title: 'Import Selesai!',
                                    html: `
                                        <div class="text-center">
                                            <i class="fa fa-check-circle text-success" style="font-size: 48px;"></i>
                                            <h4 class="mt-3">Semua data berhasil diproses!</h4>
                                            <p>Asset out telah tersimpan dalam database dan stok telah dikurangi sesuai dengan data yang diimport.</p>
                                        </div>
                                    `,
                                    icon: 'success',
                                    timer: 3000,
                                    showConfirmButton: false
                                });
                            }
                        }
                    });
                } else {
                    // Show error SweetAlert
                    Swal.fire({
                        title: response.title || 'Error!',
                        html: response.message.replace(/\n/g, '<br>'),
                        icon: 'error',
                        confirmButtonText: 'OK',
                        confirmButtonColor: '#dc3545'
                    });
                }
            },
            error: function(xhr, status, error) {
                // Hide progress
                $('#uploadProgress').hide();
                $('#uploadExcelBtn').prop('disabled', false);
                
                let errorMessage = 'Terjadi kesalahan saat mengupload file';
                let title = 'Error!';
                
                if (xhr.responseText) {
                    try {
                        const errorResponse = JSON.parse(xhr.responseText);
                        title = errorResponse.title || title;
                        errorMessage = errorResponse.message || errorMessage;
                    } catch (e) {
                        // Use default error message if JSON parsing fails
                        if (xhr.status === 0) {
                            errorMessage = 'Tidak dapat terhubung ke server. Periksa koneksi internet Anda.';
                        } else if (xhr.status === 413) {
                            errorMessage = 'File terlalu besar untuk diupload.';
                        } else if (xhr.status >= 500) {
                            errorMessage = 'Terjadi kesalahan pada server.';
                        }
                    }
                }
                
                Swal.fire({
                    title: title,
                    html: errorMessage.replace(/\n/g, '<br>'),
                    icon: 'error',
                    confirmButtonText: 'OK',
                    confirmButtonColor: '#dc3545'
                });
            }
        });
    }

    // File input change event for validation
    $('#excelFile').on('change', function() {
        const file = this.files[0];
        if (file) {
            const maxSize = 10 * 1024 * 1024; // 10MB
            const allowedExtensions = ['.xlsx', '.xls'];
            const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
            
            let isValid = true;
            let errorMessage = '';
            
            if (file.size > maxSize) {
                isValid = false;
                errorMessage = 'Ukuran file maksimal 10MB';
            } else if (!allowedExtensions.includes(fileExtension)) {
                isValid = false;
                errorMessage = 'Format file harus .xlsx atau .xls';
            }
            
            if (!isValid) {
                $(this).addClass('is-invalid');
                $(this).next('.form-text').text(errorMessage).addClass('text-danger');
                $('#uploadExcelBtn').prop('disabled', true);
            } else {
                $(this).removeClass('is-invalid');
                $(this).next('.form-text').text('Format file yang didukung: .xlsx, .xls (Maksimal ukuran file: 10MB)').removeClass('text-danger');
                $('#uploadExcelBtn').prop('disabled', false);
            }
        }
    });

    // Tab switch event to reset Excel form
    $('#excel-tab').on('shown.bs.tab', function () {
        // Reset form when switching to Excel tab
        $('#excelUploadForm')[0].reset();
        $('#uploadResults').hide();
        $('#uploadProgress').hide();
        $('#uploadExcelBtn').prop('disabled', false);
        $('#excelFile').removeClass('is-invalid');
    });

    // Load states from API
    async function loadStates() {
        try {
            const response = await fetch('/api/AssetIn/GetStates');
            const result = await response.json();
            
            if (result.success) {
                const stateSelect = $('#state');
                // Keep the first option (placeholder)
                const placeholder = stateSelect.find('option:first');
                stateSelect.empty().append(placeholder);
                
                // Add states from database
                result.data.forEach(state => {
                    stateSelect.append(`<option value="${state.value}">${state.text}</option>`);
                });
            }
        } catch (error) {
            console.error('Error loading states:', error);
        }
    }

    // Load districts from API
    async function loadDistricts() {
        try {
            const response = await fetch('/api/AssetIn/GetDistricts');
            const result = await response.json();
            
            if (result.success) {
                const districtSelect = $('#dstrctOut');
                // Keep the first option (placeholder)
                const placeholder = districtSelect.find('option:first');
                districtSelect.empty().append(placeholder);
                
                // Add districts from database
                result.data.forEach(district => {
                    districtSelect.append(`<option value="${district.value}">${district.text}</option>`);
                });
            }
        } catch (error) {
            console.error('Error loading districts:', error);
        }
    }

    // Load serial numbers for selected asset
    async function loadSerialNumbers(assetId) {
        try {
            const response = await fetch(`/api/AssetOut/GetSerialNumbers?assetId=${assetId}`);
            const result = await response.json();
            
            // Clear and reset the dropdown
            $('#serialNumberSelect').empty().prop('disabled', false);
            $('#scan_serialNumberSelect').empty().prop('disabled', false);
            
            if (result.success && result.data.length > 0) {
                // Add options to the dropdown
                result.data.forEach((serial) => {
                    const option = new Option(serial.serialNumber, serial.serialId, false, false);
                    $('#serialNumberSelect').append(option);
                    $('#scan_serialNumberSelect').append(option.cloneNode(true));
                });
                
                // Initialize Select2 on the dropdowns
                initializeSerialNumberSelect();
            } else {
                // Add empty option when no data
                $('#serialNumberSelect').append('<option value="">Tidak ada serial numbers tersedia</option>');
                $('#scan_serialNumberSelect').append('<option value="">Tidak ada serial numbers tersedia</option>');
            }
        } catch (error) {
            console.error('Error loading serial numbers:', error);
            $('#serialNumberSelect').append('<option value="">Error loading serial numbers</option>');
            $('#scan_serialNumberSelect').append('<option value="">Error loading serial numbers</option>');
        }
    }

    // Initialize Select2 for serial number dropdowns
    function initializeSerialNumberSelect() {
        $('#serialNumberSelect').select2({
            theme: 'bootstrap-5',
            placeholder: 'Pilih serial numbers...',
            allowClear: true,
            width: '100%',
            closeOnSelect: false
        });

        $('#scan_serialNumberSelect').select2({
            theme: 'bootstrap-5',
            placeholder: 'Pilih serial numbers...',
            allowClear: true,
            width: '100%',
            closeOnSelect: false
        });

        // Handle selection changes
        $('#serialNumberSelect').on('change', function() {
            updateFormValidation();
        });

        $('#scan_serialNumberSelect').on('change', function() {
            updateScanFormValidation();
        });
    }



    // Update form validation based on serial numbers selection
    function updateFormValidation() {
        const selectedSerials = $('#serialNumberSelect').val() || [];
        const currentQty = parseInt($('#qty').val()) || 0;
        const assetSelected = $('#assetSelect').val();
        
        // Enable submit button if:
        // 1. Asset is selected
        // 2. Quantity is greater than 0
        // 3. Either no serial numbers required OR serial numbers count matches quantity
        const submitBtn = $('#assetOutForm button[type="submit"]');
        
        if (assetSelected && currentQty > 0) {
            // If serial numbers are available but none selected, still allow submission
            // The backend can handle asset out without specific serial numbers
            submitBtn.prop('disabled', false);
        } else {
            submitBtn.prop('disabled', true);
        }
    }

    // Update scan form validation based on serial numbers selection
    function updateScanFormValidation() {
        const selectedSerials = $('#scan_serialNumberSelect').val() || [];
        const currentQty = parseInt($('#scan_out_qty').val()) || 0;
        
        // Enable/disable submit button
        const submitBtn = $('#scan_submitBtn');
        if (selectedSerials.length > 0 && selectedSerials.length === currentQty) {
            submitBtn.prop('disabled', false);
        } else {
            submitBtn.prop('disabled', true);
        }
    }
});
