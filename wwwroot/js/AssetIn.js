$(document).ready(function() {
    console.log("AssetIn.js loaded");

    // Image preview functionality
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

    // Validasi duplikasi untuk manual input
    async function validateDuplicate(nomorAsset, kodeBarang) {
        try {
            const response = await fetch('/api/AssetIn/CheckDuplicate', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    NomorAsset: nomorAsset,
                    KodeBarang: kodeBarang
                })
            });

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error validating duplicate:', error);
            return { isDuplicate: false, message: 'Error validasi' };
        }
    }

    // Validasi real-time untuk nomor asset
    $('#nomorAsset').on('blur', async function() {
        const nomorAsset = $(this).val().trim();
        if (nomorAsset) {
            const validation = await validateDuplicate(nomorAsset, '');
            if (validation.isDuplicate) {
                Swal.fire({
                    title: 'Peringatan!',
                    text: `${validation.duplicateType} "${nomorAsset}" sudah ada di database`,
                    icon: 'warning',
                    confirmButtonText: 'OK'
                });
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        }
    });

    // Validasi real-time untuk kode barang
    $('#kodeBarang').on('blur', async function() {
        const kodeBarang = $(this).val().trim();
        if (kodeBarang) {
            const validation = await validateDuplicate('', kodeBarang);
            if (validation.isDuplicate) {
                Swal.fire({
                    title: 'Peringatan!',
                    text: `${validation.duplicateType} "${kodeBarang}" sudah ada di database`,
                    icon: 'warning',
                    confirmButtonText: 'OK'
                });
                $(this).addClass('is-invalid');
            } else {
                $(this).removeClass('is-invalid');
            }
        }
    });

    // Handle form submission
    $('#assetInForm').on('submit', async function(e) {
        e.preventDefault();
        
        const nomorAsset = $('#nomorAsset').val().trim();
        const kodeBarang = $('#kodeBarang').val().trim();
        
        // Validasi duplikasi sebelum submit
        const validation = await validateDuplicate(nomorAsset, kodeBarang);
        if (validation.isDuplicate) {
            Swal.fire({
                title: 'Data Sudah Ada!',
                text: `${validation.duplicateType} sudah terdaftar di database. Silakan gunakan nomor/kode yang berbeda.`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }
        
        // Create FormData for file upload
        const formData = new FormData();
        formData.append('NamaBarang', $('#namaBarang').val());
        formData.append('NomorAsset', nomorAsset);
        formData.append('KodeBarang', kodeBarang);
        formData.append('KategoriBarang', $('#kategoriBarang').val());
        formData.append('Qty', parseInt($('#qty').val()));
        
        // Add file if selected
        const fileInput = $('#fotoFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('fotoFile', fileInput.files[0]);
        }

        console.log("Submitting asset in with form data");

        $.ajax({
            url: '/api/AssetIn/Create',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                console.log("Asset in response:", response);
                if (response.remarks) {
                    Swal.fire({
                        title: 'Berhasil!',
                        text: response.message,
                        icon: 'success',
                        confirmButtonText: 'OK'
                    }).then(function() {
                        $('#assetInForm')[0].reset();
                        // Hapus class invalid setelah reset
                        $('#nomorAsset, #kodeBarang').removeClass('is-invalid');
                        // Refresh notification after successful asset in
                        if (typeof refreshNotifications === 'function') {
                            refreshNotifications();
                        }
                    });
                } else {
                    Swal.fire({
                        title: 'Error!',
                        text: response.message,
                        icon: 'error',
                        confirmButtonText: 'OK'
                    });
                }
            },
            error: function(xhr, status, error) {
                console.error("Asset in error:", xhr.responseText);
                Swal.fire({
                    title: 'Error!',
                    text: 'Terjadi kesalahan: ' + error,
                    icon: 'error',
                    confirmButtonText: 'OK'
                });
            }
        });
    });

    // QR/Barcode Scanner Functionality
    let html5QrcodeScanner = null;
    let isScanning = false;

    // Image preview functionality for scan form
    $('#scan_fotoFile').on('change', function(e) {
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
                $('#scan_imagePreview').hide();
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
                $('#scan_imagePreview').hide();
                return;
            }

            // Show preview
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#scan_previewImg').attr('src', e.target.result);
                $('#scan_imagePreview').show();
            };
            reader.readAsDataURL(file);
        } else {
            $('#scan_imagePreview').hide();
        }
    });

    // Initialize scanner controls
    initializeScannerControls();

    function initializeScannerControls() {
        const startBtn = $('#startScanBtn');
        const stopBtn = $('#stopScanBtn');
        const resetBtn = $('#resetScanBtn');
        const scannedForm = $('#scannedAssetForm');
        const waitingScan = $('#waiting-scan');
        const scannerStatus = $('#scanner-status');

        // Start scanning
        startBtn.on('click', function() {
            startScanning();
        });

        // Stop scanning
        stopBtn.on('click', function() {
            stopScanning();
        });

        // Reset scanner form
        resetBtn.on('click', function() {
            resetScannerForm();
        });

        // Handle scanned form submission
        scannedForm.on('submit', function(e) {
            e.preventDefault();
            submitScannedAsset();
        });

        // Tab change handler
        $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function(e) {
            if (e.target.id === 'scan-tab' && isScanning) {
                // Stop scanning when switching away from scan tab
                stopScanning();
            }
        });
    }

    function startScanning() {
        const qrReaderElement = $('#qr-reader');
        const startBtn = $('#startScanBtn');
        const stopBtn = $('#stopScanBtn');
        const scannerStatus = $('#scanner-status');

        // Check if Html5Qrcode is available
        if (typeof Html5Qrcode === 'undefined') {
            Swal.fire({
                title: 'Error!',
                text: 'QR Scanner library tidak tersedia. Silakan refresh halaman.',
                icon: 'error'
            });
            return;
        }

        html5QrcodeScanner = new Html5Qrcode("qr-reader");
        
        Html5Qrcode.getCameras().then(devices => {
            if (devices && devices.length) {
                const cameraId = devices[0].id;
                
                // Show scanner element
                qrReaderElement.show();
                
                html5QrcodeScanner.start(
                    cameraId,
                    {
                        fps: 10,
                        qrbox: { width: 250, height: 250 },
                        aspectRatio: 1.0
                    },
                    onScanSuccess,
                    onScanFailure
                ).then(() => {
                    isScanning = true;
                    startBtn.hide();
                    stopBtn.show();
                    scannerStatus.html(`
                        <div class="scanning">
                            <i class="fa fa-camera fa-2x text-success mb-2 d-block"></i>
                            <h6 class="text-success">Scanner Aktif</h6>
                            <small class="text-muted">Arahkan ke QR Code atau Barcode</small>
                        </div>
                    `);
                }).catch(err => {
                    console.error("Error starting scanner:", err);
                    Swal.fire({
                        title: 'Error!',
                        text: 'Gagal memulai scanner. Pastikan camera dapat diakses.',
                        icon: 'error'
                    });
                    qrReaderElement.hide();
                });
            } else {
                Swal.fire({
                    title: 'Error!',
                    text: 'Tidak ada camera yang terdeteksi di device ini.',
                    icon: 'error'
                });
            }
        }).catch(err => {
            console.error("Error getting cameras:", err);
            Swal.fire({
                title: 'Error!',
                text: 'Gagal mengakses camera. Pastikan browser memiliki permission.',
                icon: 'error'
            });
        });
    }

    function stopScanning() {
        if (html5QrcodeScanner && isScanning) {
            html5QrcodeScanner.stop().then(() => {
                isScanning = false;
                $('#startScanBtn').show();
                $('#stopScanBtn').hide();
                $('#qr-reader').hide();
                $('#scanner-status').html(`
                    <i class="fa fa-qrcode fa-3x text-muted mb-2 d-block"></i>
                    <h6 class="text-muted">Scanner berhenti</h6>
                    <small class="text-muted">Klik "Mulai Scan" untuk melanjutkan</small>
                `);
                html5QrcodeScanner = null;
            }).catch(err => {
                console.error("Error stopping scanner:", err);
            });
        }
    }

    function onScanSuccess(decodedText, decodedResult) {
        console.log("QR/Barcode scanned:", decodedText);
        
        // Stop scanning immediately
        stopScanning();
        
        // Show scan success message
        $('#scan-result').show();
        setTimeout(() => {
            $('#scan-result').hide();
        }, 3000);

        try {
            // Try to parse as JSON first (for structured QR codes)
            const assetData = JSON.parse(decodedText);
            
            if (assetData.nomorAsset || assetData.namaBarang) {
                fillScannedForm({
                    nomorAsset: assetData.nomorAsset || '',
                    namaBarang: assetData.namaBarang || '',
                    kodeBarang: assetData.kodeBarang || '',
                    kategoriBarang: assetData.kategoriBarang || '',
                    foto: assetData.foto || ''
                });
            } else {
                throw new Error("Invalid QR format");
            }
        } catch (e) {
            // If not JSON, treat as simple asset number or name
            if (decodedText.length > 20) {
                // Likely a description, use as nama barang
                fillScannedForm({
                    nomorAsset: '',
                    namaBarang: decodedText,
                    kodeBarang: '',
                    kategoriBarang: '',
                    foto: ''
                });
            } else {
                // Likely an asset number
                fillScannedForm({
                    nomorAsset: decodedText,
                    namaBarang: `Item-${decodedText}`,
                    kodeBarang: decodedText,
                    kategoriBarang: '',
                    foto: ''
                });
            }
        }
        
        // Show form and enable submit button
        showScannedForm();
        
        // Validasi duplikasi untuk data yang di-scan
        validateScannedData();
    }

    function onScanFailure(error) {
        // Handle scan failure silently (don't spam console)
        // console.log("Scan failed:", error);
    }

    function fillScannedForm(data) {
        $('#scan_nomorAsset').val(data.nomorAsset);
        $('#scan_namaBarang').val(data.namaBarang);
        $('#scan_kodeBarang').val(data.kodeBarang);
        $('#scan_kategoriBarang').val(data.kategoriBarang);
        $('#scan_foto').val(data.foto);
        $('#scan_qty').val(1); // Default quantity
    }

    function showScannedForm() {
        $('#waiting-scan').hide();
        $('#scannedAssetForm').show();
        // Jangan langsung enable submit button, biarkan validateScannedData() yang mengatur
        
        // Focus on quantity field for quick input
        $('#scan_qty').focus().select();
    }

    // Validasi duplikasi untuk data yang di-scan
    async function validateScannedData() {
        const nomorAsset = $('#scan_nomorAsset').val().trim();
        const kodeBarang = $('#scan_kodeBarang').val().trim();
        
        // Show loading state
        $('#validationStatus').show().removeClass('alert-danger alert-success').addClass('alert-warning');
        $('#validationMessage').text('Memvalidasi data...');
        $('#submitScannedBtn').prop('disabled', true);
        
        if (nomorAsset || kodeBarang) {
            const validation = await validateDuplicate(nomorAsset, kodeBarang);
            if (validation.isDuplicate) {
                // Show error state
                $('#validationStatus').removeClass('alert-warning alert-success').addClass('alert-danger');
                $('#validationMessage').text(`${validation.duplicateType} sudah terdaftar di database!`);
                $('#submitScannedBtn').prop('disabled', true);
                
                Swal.fire({
                    title: 'Data Sudah Ada!',
                    text: `${validation.duplicateType} "${nomorAsset || kodeBarang}" sudah terdaftar di database. Silakan scan QR/Barcode yang berbeda.`,
                    icon: 'error',
                    confirmButtonText: 'Scan Ulang'
                }).then(() => {
                    // Reset dan scan ulang
                    resetScannerForm();
                    startScanning();
                });
                return true; // Return true jika ada duplikasi
            } else {
                // Show success state
                $('#validationStatus').removeClass('alert-warning alert-danger').addClass('alert-success');
                $('#validationMessage').text('Data valid, siap untuk disimpan!');
                $('#submitScannedBtn').prop('disabled', false);
                return false; // Return false jika tidak ada duplikasi
            }
        } else {
            // Hide validation status if no data to validate
            $('#validationStatus').hide();
            $('#submitScannedBtn').prop('disabled', false);
        }
        return false;
    }

    function resetScannerForm() {
        $('#scannedAssetForm')[0].reset();
        $('#scannedAssetForm').hide();
        $('#waiting-scan').show();
        $('#submitScannedBtn').prop('disabled', true);
        $('#scan-result').hide();
        $('#validationStatus').hide(); // Hide validation status
        $('#scan_imagePreview').hide(); // Hide image preview
        
        // Stop scanning if active
        if (isScanning) {
            stopScanning();
        }
    }

    async function submitScannedAsset() {
        const nomorAsset = $('#scan_nomorAsset').val().trim();
        const kodeBarang = $('#scan_kodeBarang').val().trim();
        
        // Validasi duplikasi final sebelum submit - BLOKIR SUBMIT JIKA ADA DUPLIKASI
        const validation = await validateDuplicate(nomorAsset, kodeBarang);
        if (validation.isDuplicate) {
            Swal.fire({
                title: 'Tidak Dapat Submit!',
                text: `${validation.duplicateType} sudah ada di database. Data tidak dapat disimpan.`,
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return; // Stop submit process
        }
        
        // Create FormData for file upload
        const formData = new FormData();
        formData.append('NamaBarang', $('#scan_namaBarang').val());
        formData.append('NomorAsset', nomorAsset);
        formData.append('KodeBarang', kodeBarang);
        formData.append('KategoriBarang', $('#scan_kategoriBarang').val());
        formData.append('Qty', parseInt($('#scan_qty').val()));
        
        // Add file if selected
        const fileInput = $('#scan_fotoFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('fotoFile', fileInput.files[0]);
        }

        // Validate required fields
        const namaBarang = $('#scan_namaBarang').val();
        const kategoriBarang = $('#scan_kategoriBarang').val();
        const qty = $('#scan_qty').val();
        
        if (!namaBarang || !kategoriBarang || !qty) {
            Swal.fire({
                title: 'Validasi Error!',
                text: 'Nama Barang, Kategori, dan Quantity harus diisi.',
                icon: 'warning'
            });
            return;
        }

        try {
            const response = await fetch('/api/AssetIn/CreateFromScan', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            if (result.success) {
                Swal.fire({
                    title: 'Berhasil!',
                    text: result.message,
                    icon: 'success',
                    confirmButtonText: 'OK'
                }).then(() => {
                    // Refresh notifications
                    if (typeof refreshNotifications === 'function') {
                        refreshNotifications();
                    }
                    
                    // Reset form
                    resetScannerForm();
                    
                    // Ask user what to do next
                    Swal.fire({
                        title: 'Scan Lagi?',
                        text: 'Apakah Anda ingin scan QR/Barcode lagi?',
                        icon: 'question',
                        showCancelButton: true,
                        confirmButtonText: 'Ya, Scan Lagi',
                        cancelButtonText: 'Selesai'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            startScanning();
                        }
                    });
                });
            } else {
                Swal.fire({
                    title: 'Gagal!',
                    text: result.message,
                    icon: 'error'
                });
            }
        } catch (error) {
            console.error('Error submitting scanned asset:', error);
            Swal.fire({
                title: 'Error!',
                text: 'Terjadi kesalahan saat menyimpan data',
                icon: 'error'
            });
        }
    }

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
            title: 'Konfirmasi Upload',
            html: `
                <div class="text-left">
                    <p><strong>File:</strong> ${file.name}</p>
                    <p><strong>Ukuran:</strong> ${(file.size / 1024 / 1024).toFixed(2)} MB</p>
                    <p><strong>Format:</strong> ${fileExtension.toUpperCase()}</p>
                </div>
                <div class="alert alert-info mt-3">
                    <i class="fa fa-info-circle"></i> 
                    Proses import akan memvalidasi semua data dan menampilkan hasil secara detail.
                </div>
            `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: '<i class="fa fa-upload"></i> Ya, Upload!',
            cancelButtonText: '<i class="fa fa-times"></i> Batal',
            confirmButtonColor: '#007bff',
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
                                <strong>Berhasil mengimpor ${response.successCount} data asset</strong>
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
                                            <h4 class="mt-3">Semua data berhasil diimpor!</h4>
                                            <p>Data asset telah tersimpan dalam database dan siap digunakan.</p>
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
});
