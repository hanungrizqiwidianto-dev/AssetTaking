$(document).ready(function() {
    console.log("AssetIn.js loaded");
    
    // Step navigation state
    let currentStep = 1;
    let assetData = {};
    let generatedSerials = [];
    
    // Load categories from database
    loadCategories();
    
    // Load states from database
    loadStates();
    
    // Load districts from database  
    loadDistricts();

    // Step Navigation Functions
    function showStep(stepNumber) {
        // Hide all steps
        $('.step-content').removeClass('active').hide();
        $('.step-indicator').removeClass('active completed');
        
        // Show current step
        $(`#step${stepNumber}-content`).addClass('active').show();
        $(`#step${stepNumber}-indicator`).addClass('active');
        
        // Mark previous steps as completed
        for (let i = 1; i < stepNumber; i++) {
            $(`#step${i}-indicator`).addClass('completed');
        }
        
        currentStep = stepNumber;
    }

    function validateStep1() {
        const namaBarang = $('#namaBarang').val()?.trim();
        const nomorAsset = $('#nomorAsset').val()?.trim();
        const kategoriBarang = $('#kategoriBarang').val();
        const kodeBarang = $('#kodeBarang').val()?.trim();
        const qty = parseInt($('#qty').val());

        console.log('Validating step 1:', { namaBarang, nomorAsset, kategoriBarang, kodeBarang, qty });

        if (!namaBarang || !nomorAsset || !kategoriBarang || !kodeBarang || !qty || qty <= 0) {
            Swal.fire({
                title: 'Data Tidak Lengkap!',
                text: 'Mohon lengkapi semua field yang wajib diisi.',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return false;
        }

        return true;
    }

    function populateAssetSummary() {
        $('#summary-namaBarang').text($('#namaBarang').val());
        $('#summary-nomorAsset').text($('#nomorAsset').val());
        $('#summary-kategoriBarang').text($('#kategoriBarang').val());
        $('#summary-qty').text($('#qty').val());
    }

    async function generateSerialNumbers() {
        const kategoriBarang = $('#kategoriBarang').val();
        const qty = parseInt($('#qty').val());

        console.log('Generating serial numbers:', { kategoriBarang, qty });

        try {
            const response = await fetch('/api/AssetIn/GenerateSerialNumbers', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    KategoriBarang: kategoriBarang,
                    Quantity: qty
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const result = await response.json();
            console.log('GenerateSerialNumbers response:', result);
            
            if (result.success) {
                // Handle different response formats
                const serials = result.data || result.serialNumbers || [];
                console.log('Generated serials:', serials);
                
                if (!Array.isArray(serials) || serials.length === 0) {
                    throw new Error('No serial numbers generated');
                }
                
                return serials;
            } else {
                throw new Error(result.message || 'Failed to generate serial numbers');
            }
        } catch (error) {
            console.error('Error in generateSerialNumbers:', error);
            throw error;
        }
    }

    function createSerialPoMapping(serials) {
        if (!serials || !Array.isArray(serials) || serials.length === 0) {
            $('#serialPoMapping').html('<div class="alert alert-warning"><i class="fa fa-exclamation-triangle me-1"></i>Tidak ada serial numbers yang dapat ditampilkan.</div>');
            return;
        }
        
        const poNumber = $('#poNumber').val() || '';
        let html = '<div class="row">';
        
        serials.forEach((serial, index) => {
            html += `
                <div class="col-md-6 mb-3">
                    <div class="serial-po-item">
                        <div class="row">
                            <div class="col-12 mb-2">
                                <label class="form-label fw-bold">Serial Number ${index + 1}</label>
                                <div class="serial-number-display">${serial}</div>
                                <input type="hidden" name="serials[${index}]" value="${serial}">
                            </div>
                            <div class="col-12">
                                <label class="form-label" for="poItem_${index}">PO Item</label>
                                <input type="text" class="form-control" id="poItem_${index}" 
                                       name="poItems[${index}]" placeholder="Contoh: 001, 002">
                                <small class="form-text text-muted">
                                    PO Item untuk serial ${serial}
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });
        
        html += '</div>';
        
        if (poNumber) {
            html = `<div class="alert alert-info mb-3">
                        <i class="fa fa-info-circle me-1"></i>
                        <strong>PO Number:</strong> ${poNumber}
                    </div>` + html;
        }
        
        $('#serialPoMapping').html(html);
    }

    // Event Handlers for Step Navigation
    $('#nextToStep2').on('click', async function() {
        if (!validateStep1()) {
            return;
        }

        // Store step 1 data
        assetData = {
            namaBarang: $('#namaBarang').val(),
            nomorAsset: $('#nomorAsset').val(),
            kategoriBarang: $('#kategoriBarang').val(),
            kodeBarang: $('#kodeBarang').val(),
            qty: parseInt($('#qty').val()),
            poNumber: $('#poNumber').val(),
            state: $('#state').val(),
            dstrctIn: $('#dstrctIn').val()
        };

        try {
            // Show loading
            $(this).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Generating Serial Numbers...');
            
            // Generate serial numbers
            generatedSerials = await generateSerialNumbers();
            
            // Validate generated serials
            if (!generatedSerials || !Array.isArray(generatedSerials) || generatedSerials.length === 0) {
                throw new Error('Serial numbers tidak dapat di-generate. Silakan coba lagi.');
            }
            
            // Populate summary and create mapping
            populateAssetSummary();
            createSerialPoMapping(generatedSerials);
            
            // Move to step 2
            showStep(2);
            
        } catch (error) {
            Swal.fire({
                title: 'Error!',
                text: 'Gagal generate serial numbers: ' + error.message,
                icon: 'error',
                confirmButtonText: 'OK'
            });
        } finally {
            $(this).prop('disabled', false).html('<i class="fa fa-arrow-right"></i> Lanjut ke Serial Numbers & PO Items');
        }
    });

    $('#backToStep1').on('click', function() {
        showStep(1);
    });

    // Form submission for step 2
    $('#serialPoForm').on('submit', async function(e) {
        e.preventDefault();
        
        // Collect PO items data
        const poItems = [];
        $('input[name^="poItems"]').each(function() {
            poItems.push($(this).val() || '');
        });

        // Prepare form data
        const formData = new FormData();
        
        // Add asset data from step 1
        Object.keys(assetData).forEach(key => {
            if (assetData[key] !== null && assetData[key] !== undefined) {
                formData.append(key, assetData[key]);
            }
        });

        // Add serial numbers and PO items
        generatedSerials.forEach((serial, index) => {
            formData.append(`serialNumbers[${index}]`, serial);
            formData.append(`poItems[${index}]`, poItems[index] || '');
        });

        // Add photo if selected
        const fileInput = $('#fotoFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('fotoFile', fileInput.files[0]);
        }

        // Add additional flags
        formData.append('manualSerial', 'false');
        formData.append('useStepProcess', 'true');

        try {
            $('#submitAssetIn').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Menyimpan...');
            
            const response = await fetch('/api/AssetIn/CreateWithSteps', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success || result.remarks) {
                Swal.fire({
                    title: 'Berhasil!',
                    text: result.message || 'Asset In berhasil disimpan',
                    icon: 'success',
                    confirmButtonText: 'OK'
                }).then(() => {
                    // Reset form and go back to step 1
                    resetAllForms();
                    showStep(1);
                });
            } else {
                throw new Error(result.message || 'Terjadi kesalahan saat menyimpan data');
            }
            
        } catch (error) {
            Swal.fire({
                title: 'Error!',
                text: error.message,
                icon: 'error',
                confirmButtonText: 'OK'
            });
        } finally {
            $('#submitAssetIn').prop('disabled', false).html('<i class="fa fa-save"></i> Submit Asset In');
        }
    });

    function resetAllForms() {
        $('#assetDataForm')[0].reset();
        $('#serialPoForm')[0].reset();
        $('#imagePreview').hide();
        assetData = {};
        generatedSerials = [];
        $('#serialPoMapping').empty();
    }

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

    // Check if asset exists for quantity increment information
    async function checkAssetExists(nomorAsset, kodeBarang) {
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
            console.error('Error checking asset:', error);
            return { isDuplicate: false, message: 'Error validasi' };
        }
    }

    // Generate item code based on category
    async function generateItemCode(kategoriBarang) {
        try {
            const response = await fetch('/api/AssetIn/GenerateItemCode', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    KategoriBarang: kategoriBarang
                })
            });

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error generating item code:', error);
            return { success: false, message: 'Error generate kode barang' };
        }
    }

    // Reset manual form function
    function resetManualForm() {
        $('#assetInForm')[0].reset();
        // Reset checkbox to unchecked (automatic mode)
        $('#manualKodeBarang').prop('checked', false);
        $('#manualSerial').prop('checked', false);
        $('#scan_manualSerial').prop('checked', false);
        
        // Reset disabled fields properly
        $('#kodeBarang').prop('disabled', true);
        $('#kodeBarang').prop('readonly', true);
        $('#kodeBarang').val('');
        
        // Reset serial number display for manual form
        $('#autoSerialInfo').show();
        $('#manualSerialInput').hide();
        $('#serialNumbers').val('');
        
        // Reset serial number display for scan form
        $('#scan_autoSerialInfo').show();
        $('#scan_manualSerialInput').hide();
        $('#scan_serialNumbers').val('');
        
        // Reset info text
        $('#kodeBarangInfo').html('<i class="fa fa-info-circle me-1"></i> Kode barang akan otomatis diisi berdasarkan kategori yang dipilih');
        $('#serialHelpText').text('Serial numbers akan dibuat otomatis (contoh: RND12391) berdasarkan kategori yang dipilih');
        $('#scan_serialHelpText').text('Serial numbers akan dibuat otomatis (contoh: RND12391) berdasarkan kategori yang dipilih');
        
        // Clear image preview
        $('#imagePreview').hide();
        $('#scan_imagePreview').hide();
        
        // Remove validation classes
        $('#nomorAsset, #kodeBarang').removeClass('is-invalid is-info');
    }

    // Auto-generate item code when category changes
    $('#kategoriBarang').on('change', async function() {
        const kategoriBarang = $(this).val().trim();
        const isManualInput = $('#manualKodeBarang').is(':checked');
        const isManualSerial = $('#manualSerial').is(':checked');
        
        if (kategoriBarang && !isManualInput) {
            // Temporarily enable the field to set value
            $('#kodeBarang').prop('disabled', false);
            
            const result = await generateItemCode(kategoriBarang);
            if (result.success) {
                $('#kodeBarang').val(result.itemCode);
                
                // Show info message
                Swal.fire({
                    title: 'Kode Barang Otomatis',
                    text: `Kode barang "${result.itemCode}" telah digenerate berdasarkan kategori "${kategoriBarang}"`,
                    icon: 'info',
                    timer: 3000,
                    showConfirmButton: false
                });
            } else {
                $('#kodeBarang').val('');
            }
            
            // Disable the field again after setting value
            $('#kodeBarang').prop('disabled', true);
        } else if (!kategoriBarang) {
            // Clear kode barang if no category selected
            if (!isManualInput) {
                $('#kodeBarang').prop('disabled', false);
                $('#kodeBarang').val('');
                $('#kodeBarang').prop('disabled', true);
            }
        }
        
        // Auto-generate serial numbers when category changes (if not manual mode)
        if (kategoriBarang && !isManualSerial) {
            const qty = parseInt($('#qty').val()) || 1;
            
            // Generate actual serial numbers for manual input tab
            if ($('#manualInputTab').hasClass('active')) {
                generateAndDisplaySerialNumbers(kategoriBarang, qty);
            } else {
                // For other tabs, just show preview
                const serialPreview = generateSerialPreview(kategoriBarang, qty);
                $('#serialHelpText').html(
                    '<i class="fa fa-info-circle me-1"></i>' +
                    `Serial numbers akan dibuat otomatis berdasarkan kategori "${kategoriBarang}": ` +
                    `<strong>${serialPreview}</strong>`
                );
            }
        } else if (!kategoriBarang && !isManualSerial) {
            // Reset to default help text
            $('#serialHelpText').text('Serial numbers akan dibuat otomatis (contoh: RND12391) berdasarkan kategori yang dipilih');
            $('#serialNumbers').val('');
        }
    });
    
    // Auto-update serial numbers when quantity changes
    $('#qty').on('input change', function() {
        const kategoriBarang = $('#kategoriBarang').val();
        const isManualSerial = $('#manualSerial').is(':checked');
        
        if (kategoriBarang && !isManualSerial) {
            const qty = parseInt($(this).val()) || 1;
            
            // Generate actual serial numbers for manual input tab
            if ($('#manualInputTab').hasClass('active')) {
                generateAndDisplaySerialNumbers(kategoriBarang, qty);
            } else {
                // For other tabs, just show preview
                const serialPreview = generateSerialPreview(kategoriBarang, qty);
                $('#serialHelpText').html(
                    '<i class="fa fa-info-circle me-1"></i>' +
                    `Serial numbers akan dibuat otomatis berdasarkan kategori "${kategoriBarang}": ` +
                    `<strong>${serialPreview}</strong>`
                );
            }
        }
    });
    
    // Generate and display serial numbers
    async function generateAndDisplaySerialNumbers(kategoriBarang, qty) {
        try {
            const result = await generateSerialNumbersForCategory(kategoriBarang, qty);
            
            if (result.success) {
                // Fill the serial numbers textarea
                $('#serialNumbers').val(result.serialNumbersText);
                
                // Update help text
                $('#serialHelpText').html(
                    '<i class="fa fa-check-circle me-1 text-success"></i>' +
                    `Serial numbers telah digenerate otomatis berdasarkan kategori "${kategoriBarang}"`
                );
            } else {
                // Show preview if generation fails
                const serialPreview = generateSerialPreview(kategoriBarang, qty);
                $('#serialHelpText').html(
                    '<i class="fa fa-info-circle me-1"></i>' +
                    `Serial numbers akan dibuat otomatis berdasarkan kategori "${kategoriBarang}": ` +
                    `<strong>${serialPreview}</strong>`
                );
            }
        } catch (error) {
            console.error('Error generating serial numbers:', error);
            const serialPreview = generateSerialPreview(kategoriBarang, qty);
            $('#serialHelpText').html(
                '<i class="fa fa-info-circle me-1"></i>' +
                `Serial numbers akan dibuat otomatis berdasarkan kategori "${kategoriBarang}": ` +
                `<strong>${serialPreview}</strong>`
            );
        }
    }

    // Auto-generate serial numbers when category and quantity are set (for manual input)
    async function generateSerialNumbersForCategory(kategoriBarang, qty) {
        try {
            const response = await fetch('/api/AssetIn/GenerateSerialNumbers', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    KategoriBarang: kategoriBarang,
                    Quantity: qty
                })
            });

            const result = await response.json();
            return result;
        } catch (error) {
            console.error('Error generating serial numbers:', error);
            return { success: false, message: 'Error generate serial numbers' };
        }
    }

    // Function to generate serial number preview (for display only)
    function generateSerialPreview(kategoriBarang, qty) {
        // Get category prefix
        let prefix = '';
        switch (kategoriBarang.toLowerCase()) {
            case 'rnd': 
            case 'research & development':
                prefix = 'RND';
                break;
            case 'sparepart':
            case 'spare part':
                prefix = 'SPR';
                break;
            case 'tools':
                prefix = 'TLS';
                break;
            case 'equipment':
                prefix = 'EQP';
                break;
            case 'furniture':
                prefix = 'FUR';
                break;
            case 'electronic':
                prefix = 'ELC';
                break;
            case 'automotive':
                prefix = 'AUT';
                break;
            case 'software':
                prefix = 'SFT';
                break;
            case 'consumable':
                prefix = 'CON';
                break;
            default:
                prefix = 'GEN';
                break;
        }
        
        if (qty <= 3) {
            // Show all serial numbers for small quantities
            let previews = [];
            for (let i = 0; i < qty; i++) {
                previews.push(`${prefix}${String(i + 1).padStart(5, '0')}`);
            }
            return previews.join(', ');
        } else {
            // Show first few and count for larger quantities
            return `${prefix}00001, ${prefix}00002, ... (${qty} total)`;
        }
    }

    // Handle manual input checkbox toggle
    $('#manualKodeBarang').on('change', function() {
        const isManualInput = $(this).is(':checked');
        const kodeBarangField = $('#kodeBarang');
        const kodeBarangInfo = $('#kodeBarangInfo');
        
        if (isManualInput) {
            // Enable manual input
            kodeBarangField.prop('disabled', false);
            kodeBarangField.prop('readonly', false);
            kodeBarangField.focus();
            
            // Update info text
            kodeBarangInfo.html('<i class="fa fa-edit me-1"></i> Anda dapat menginput kode barang secara manual');
        } else {
            // Disable manual input and switch back to automatic
            kodeBarangField.prop('disabled', true);
            kodeBarangField.prop('readonly', true);
            
            // Update info text
            kodeBarangInfo.html('<i class="fa fa-info-circle me-1"></i> Kode barang akan otomatis diisi berdasarkan kategori yang dipilih');
            
            // Auto-generate code if category is selected
            const kategoriBarang = $('#kategoriBarang').val();
            if (kategoriBarang) {
                $('#kategoriBarang').trigger('change');
            } else {
                kodeBarangField.val('');
            }
        }
    });

    // Handle manual serial number checkbox toggle
    $('#manualSerial').on('change', function() {
        const isManualInput = $(this).is(':checked');
        const autoSerialInfo = $('#autoSerialInfo');
        const manualSerialInput = $('#manualSerialInput');
        const serialHelpText = $('#serialHelpText');
        
        if (isManualInput) {
            // Show manual input
            autoSerialInfo.hide();
            manualSerialInput.show();
            $('#serialNumbers').focus();
            
            // Update help text
            serialHelpText.text('Masukkan serial number secara manual. Pisahkan dengan koma jika quantity lebih dari 1 (contoh: RND12391, RND12392)');
        } else {
            // Show automatic info
            manualSerialInput.hide();
            autoSerialInfo.show();
            $('#serialNumbers').val('');
            
            // Update help text and regenerate preview if category is selected
            const kategoriBarang = $('#kategoriBarang').val();
            if (kategoriBarang) {
                const qty = parseInt($('#qty').val()) || 1;
                const serialPreview = generateSerialPreview(kategoriBarang, qty);
                serialHelpText.html(
                    '<i class="fa fa-info-circle me-1"></i>' +
                    `Serial numbers akan dibuat otomatis berdasarkan kategori "${kategoriBarang}": ` +
                    `<strong>${serialPreview}</strong>`
                );
            } else {
                serialHelpText.text('Serial numbers akan dibuat otomatis (contoh: RND12391) berdasarkan kategori yang dipilih');
            }
        }
    });

    // Handle manual serial number checkbox toggle for scan form
    $('#scan_manualSerial').on('change', function() {
        const isManualInput = $(this).is(':checked');
        const autoSerialInfo = $('#scan_autoSerialInfo');
        const manualSerialInput = $('#scan_manualSerialInput');
        const serialHelpText = $('#scan_serialHelpText');
        
        if (isManualInput) {
            // Show manual input
            autoSerialInfo.hide();
            manualSerialInput.show();
            $('#scan_serialNumbers').focus();
            
            // Update help text
            serialHelpText.text('Masukkan serial number secara manual. Pisahkan dengan koma jika quantity lebih dari 1 (contoh: RND12391, RND12392)');
        } else {
            // Show automatic info
            manualSerialInput.hide();
            autoSerialInfo.show();
            $('#scan_serialNumbers').val('');
            
            // Update help text
            serialHelpText.text('Serial numbers akan dibuat otomatis (contoh: RND12391) berdasarkan kategori yang dipilih');
        }
    });

    // Handle reset button click
    $('#assetInForm button[type="reset"]').on('click', function(e) {
        e.preventDefault();
        resetManualForm();
    });

    // Show information when asset exists (instead of blocking)
    $('#nomorAsset, #kodeBarang').on('blur', async function() {
        const nomorAsset = $('#nomorAsset').val().trim();
        const kodeBarang = $('#kodeBarang').val().trim();
        
        if (nomorAsset && kodeBarang) {
            const validation = await checkAssetExists(nomorAsset, kodeBarang);
            if (validation.isDuplicate) {
                // Show info instead of warning
                Swal.fire({
                    title: 'Informasi Asset',
                    text: validation.message,
                    icon: 'info',
                    timer: 4000,
                    showConfirmButton: false
                });
                $(this).removeClass('is-invalid').addClass('is-info');
            } else {
                $(this).removeClass('is-invalid is-info');
            }
        }
    });

    // Handle form submission
    $('#assetInForm').on('submit', async function(e) {
        e.preventDefault();
        
        const nomorAsset = $('#nomorAsset').val().trim();
        const kodeBarang = $('#kodeBarang').val().trim();
        
        // Check if asset exists for information (no longer blocking)
        const validation = await checkAssetExists(nomorAsset, kodeBarang);
        if (validation.isDuplicate) {
            // Show confirmation dialog for existing asset
            const result = await Swal.fire({
                title: 'Asset Sudah Ada',
                text: validation.message,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Ya, Tambahkan Qty',
                cancelButtonText: 'Batal',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d'
            });
            
            if (!result.isConfirmed) {
                return;
            }
        }
        
        // Create FormData for file upload
        const formData = new FormData();
        formData.append('NamaBarang', $('#namaBarang').val());
        formData.append('NomorAsset', nomorAsset);
        formData.append('KodeBarang', kodeBarang);
        formData.append('KategoriBarang', $('#kategoriBarang').val());
        formData.append('Qty', parseInt($('#qty').val()));
        
        // Add manual serial data
        const isManualSerial = $('#manualSerial').is(':checked');
        formData.append('ManualSerial', isManualSerial);
        if (isManualSerial) {
            formData.append('SerialNumbers', $('#serialNumbers').val().trim());
        }
        
        // Add PO data
        formData.append('PoNumber', $('#poNumber').val() || '');
        formData.append('PoItem', $('#poItem').val() || '');
        formData.append('StateId', $('#state').val() || '');
        formData.append('DstrctIn', $('#dstrctIn').val() || '');
        
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
                        resetManualForm();
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
                    foto: assetData.foto || '',
                    qty: assetData.qty || 1,
                    state: assetData.state || '',
                    district: assetData.district || '',
                    serialNumber: assetData.serialNumber || ''
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
        
        // Use quantity from QR data, default to 1 if not specified
        var qtyFromQR = data.qty || 1;
        $('#scan_qty').val(qtyFromQR);
        
        // Handle state and district from QR data
        if (data.state) {
            $('#scan_state').val(data.state);
        }
        if (data.district) {
            $('#scan_dstrctIn').val(data.district);
        }
        
        // Handle serial number from QR data
        if (data.serialNumber) {
            $('#scan_manualSerial').prop('checked', true);
            $('#scan_manualSerial').trigger('change'); // Trigger the change event to show manual input
            
            // If qty > 1, check if we have multiple serial numbers
            if (qtyFromQR > 1) {
                // For multiple qty, we expect comma-separated serial numbers
                $('#scan_serialNumbers').val(data.serialNumber);
            } else {
                $('#scan_serialNumbers').val(data.serialNumber);
            }
        } else {
            $('#scan_manualSerial').prop('checked', false);
            $('#scan_manualSerial').trigger('change'); // Trigger the change event to hide manual input
            $('#scan_serialNumbers').val('');
        }
    }

    function showScannedForm() {
        $('#waiting-scan').hide();
        $('#scannedAssetForm').show();
        // Jangan langsung enable submit button, biarkan validateScannedData() yang mengatur
        
        // Focus on quantity field for quick input
        $('#scan_qty').focus().select();
    }

    // Check scanned data for information (no longer blocking)
    async function validateScannedData() {
        const nomorAsset = $('#scan_nomorAsset').val().trim();
        const kodeBarang = $('#scan_kodeBarang').val().trim();
        
        // Show loading state
        $('#validationStatus').show().removeClass('alert-danger alert-success').addClass('alert-warning');
        $('#validationMessage').text('Memvalidasi data...');
        $('#submitScannedBtn').prop('disabled', true);
        
        if (nomorAsset && kodeBarang) {
            const validation = await checkAssetExists(nomorAsset, kodeBarang);
            if (validation.isDuplicate) {
                // Show info state instead of error
                $('#validationStatus').removeClass('alert-warning alert-success').addClass('alert-info');
                $('#validationMessage').text(`Asset sudah ada. Qty akan ditambahkan ke asset yang sudah ada.`);
                $('#submitScannedBtn').prop('disabled', false);
                
                Swal.fire({
                    title: 'Asset Sudah Ada',
                    text: validation.message,
                    icon: 'info',
                    timer: 3000,
                    showConfirmButton: false
                });
                return false; // No longer blocking
            } else {
                // Show success state
                $('#validationStatus').removeClass('alert-warning alert-info').addClass('alert-success');
                $('#validationMessage').text('Data valid, siap untuk disimpan!');
                $('#submitScannedBtn').prop('disabled', false);
                return false;
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
        
        // Check if asset exists for information (no longer blocking)
        const validation = await checkAssetExists(nomorAsset, kodeBarang);
        if (validation.isDuplicate) {
            // Show confirmation dialog for existing asset
            const result = await Swal.fire({
                title: 'Asset Sudah Ada',
                text: validation.message,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Ya, Tambahkan Qty',
                cancelButtonText: 'Batal',
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d'
            });
            
            if (!result.isConfirmed) {
                return;
            }
        }
        
        // Create FormData for file upload
        const formData = new FormData();
        formData.append('NamaBarang', $('#scan_namaBarang').val());
        formData.append('NomorAsset', nomorAsset);
        formData.append('KodeBarang', kodeBarang);
        formData.append('KategoriBarang', $('#scan_kategoriBarang').val());
        formData.append('Qty', parseInt($('#scan_qty').val()));
        
        // Add manual serial data
        const isManualSerial = $('#scan_manualSerial').is(':checked');
        formData.append('ManualSerial', isManualSerial);
        if (isManualSerial) {
            formData.append('SerialNumbers', $('#scan_serialNumbers').val().trim());
        }
        
        // Add PO data
        formData.append('PoNumber', $('#scan_poNumber').val() || '');
        formData.append('PoItem', $('#scan_poItem').val() || '');
        formData.append('StateId', $('#scan_state').val() || '');
        formData.append('DstrctIn', $('#scan_dstrctIn').val() || '');
        
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

    // Load categories from database
    async function loadCategories() {
        try {
            const response = await fetch('/api/AssetIn/GetCategories');
            const result = await response.json();
            
            if (result.success) {
                const categorySelects = ['#kategoriBarang', '#scan_kategoriBarang'];
                
                categorySelects.forEach(selector => {
                    const selectElement = $(selector);
                    if (selectElement.length) {
                        // Keep the first option (placeholder)
                        const placeholder = selectElement.find('option:first');
                        selectElement.empty().append(placeholder);
                        
                        // Add categories from database
                        result.data.forEach(category => {
                            selectElement.append(`<option value="${category.value}">${category.text}</option>`);
                        });
                    }
                });
            }
        } catch (error) {
            console.error('Error loading categories:', error);
        }
    }
    
    async function loadStates() {
        try {
            const response = await fetch('/api/AssetIn/GetStates');
            const result = await response.json();
            
            if (result.success) {
                const stateSelects = ['#state', '#scan_state'];
                
                stateSelects.forEach(selector => {
                    const selectElement = $(selector);
                    if (selectElement.length) {
                        // Keep the first option (placeholder)
                        const placeholder = selectElement.find('option:first');
                        selectElement.empty().append(placeholder);
                        
                        // Add states from database
                        result.data.forEach(state => {
                            selectElement.append(`<option value="${state.value}">${state.text}</option>`);
                        });
                    }
                });
            }
        } catch (error) {
            console.error('Error loading states:', error);
        }
    }
    
    async function loadDistricts() {
        try {
            const response = await fetch('/api/AssetIn/GetDistricts');
            const result = await response.json();
            
            if (result.success) {
                const districtSelects = ['#dstrctIn', '#scan_dstrctIn'];
                
                districtSelects.forEach(selector => {
                    const selectElement = $(selector);
                    if (selectElement.length) {
                        // Keep the first option (placeholder)
                        const placeholder = selectElement.find('option:first');
                        selectElement.empty().append(placeholder);
                        
                        // Add districts from database
                        result.data.forEach(district => {
                            selectElement.append(`<option value="${district.value}">${district.text}</option>`);
                        });
                    }
                });
            }
        } catch (error) {
            console.error('Error loading districts:', error);
        }
    }

    // ===== SCAN TAB STEP NAVIGATION =====
    
    // Scan tab step navigation variables
    let scanAssetData = {};
    let scanGeneratedSerials = [];
    
    // Function to validate scan step 1 
    function validateScanStep1() {
        const namaBarang = $('#scan_namaBarang').val()?.trim();
        const nomorAsset = $('#scan_nomorAsset').val()?.trim();
        const kategoriBarang = $('#scan_kategoriBarang').val();
        const kodeBarang = $('#scan_kodeBarang').val()?.trim();
        const qty = parseInt($('#scan_qty').val());

        console.log('Validating scan step 1:', { namaBarang, nomorAsset, kategoriBarang, kodeBarang, qty });

        if (!namaBarang || !nomorAsset || !kategoriBarang || !kodeBarang || !qty || qty <= 0) {
            Swal.fire({
                title: 'Data Tidak Lengkap!',
                text: 'Mohon lengkapi semua field yang wajib diisi.',
                icon: 'warning',
                confirmButtonText: 'OK'
            });
            return false;
        }

        return true;
    }

    // Function to generate serial numbers for scan
    async function generateScanSerialNumbers() {
        const kategoriBarang = $('#scan_kategoriBarang').val();
        const qty = parseInt($('#scan_qty').val());

        console.log('Generating scan serial numbers:', { kategoriBarang, qty });

        try {
            const response = await fetch('/api/AssetIn/GenerateSerialNumbers', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    KategoriBarang: kategoriBarang,
                    Quantity: qty
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const result = await response.json();
            console.log('GenerateScanSerialNumbers response:', result);
            
            if (result.success) {
                const serials = result.data || result.serialNumbers || [];
                console.log('Generated scan serials:', serials);
                
                if (!Array.isArray(serials) || serials.length === 0) {
                    throw new Error('No serial numbers generated');
                }
                
                return serials;
            } else {
                throw new Error(result.message || 'Failed to generate serial numbers');
            }
        } catch (error) {
            console.error('Error in generateScanSerialNumbers:', error);
            throw error;
        }
    }

    // Function to create serial PO mapping for scan
    function createScanSerialPoMapping(serials) {
        if (!serials || !Array.isArray(serials) || serials.length === 0) {
            $('#scan-serialPoMapping').html('<div class="alert alert-warning"><i class="fa fa-exclamation-triangle me-1"></i>Tidak ada serial numbers yang dapat ditampilkan.</div>');
            return;
        }
        
        const poNumber = $('#scan_poNumber').val() || '';
        let html = '<div class="row">';
        
        serials.forEach((serial, index) => {
            html += `
                <div class="col-md-6 mb-3">
                    <div class="serial-po-item">
                        <div class="row">
                            <div class="col-12 mb-2">
                                <label class="form-label fw-bold">Serial Number ${index + 1}</label>
                                <div class="serial-number-display">${serial}</div>
                                <input type="hidden" name="scan_serials[${index}]" value="${serial}">
                            </div>
                            <div class="col-12">
                                <label class="form-label" for="scan_poItem_${index}">PO Item</label>
                                <input type="text" class="form-control" id="scan_poItem_${index}" 
                                       name="scan_poItems[${index}]" placeholder="Contoh: 001, 002">
                                <small class="form-text text-muted">
                                    PO Item untuk serial ${serial}
                                </small>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });
        
        html += '</div>';
        
        if (poNumber) {
            html = `<div class="alert alert-info mb-3">
                        <i class="fa fa-info-circle me-1"></i>
                        <strong>PO Number:</strong> ${poNumber}
                    </div>` + html;
        }
        
        $('#scan-serialPoMapping').html(html);
    }

    // Event handler for scan next to step 2
    $('#scanNextToStep2').on('click', async function() {
        if (!validateScanStep1()) {
            return;
        }

        // Store scan step 1 data
        scanAssetData = {
            namaBarang: $('#scan_namaBarang').val(),
            nomorAsset: $('#scan_nomorAsset').val(),
            kategoriBarang: $('#scan_kategoriBarang').val(),
            kodeBarang: $('#scan_kodeBarang').val(),
            qty: parseInt($('#scan_qty').val()),
            poNumber: $('#scan_poNumber').val(),
            state: $('#scan_state').val(),
            dstrctIn: $('#scan_dstrctIn').val()
        };

        try {
            // Show loading
            $(this).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Generating Serial Numbers...');
            
            // Generate serial numbers
            scanGeneratedSerials = await generateScanSerialNumbers();
            
            // Validate generated serials
            if (!scanGeneratedSerials || !Array.isArray(scanGeneratedSerials) || scanGeneratedSerials.length === 0) {
                throw new Error('Serial numbers tidak dapat di-generate. Silakan coba lagi.');
            }
            
            // Create mapping
            createScanSerialPoMapping(scanGeneratedSerials);
            
            // Show step 2 and hide step 1
            $('#scan-step-1').hide();
            $('#scan-step-2').show();
            
            // Update step indicators
            $('.scan-step-indicator').removeClass('active completed');
            $('#scan-step1-indicator').addClass('completed');
            $('#scan-step2-indicator').addClass('active');
            
        } catch (error) {
            Swal.fire({
                title: 'Error!',
                text: 'Gagal generate serial numbers: ' + error.message,
                icon: 'error',
                confirmButtonText: 'OK'
            });
        } finally {
            $(this).prop('disabled', false).html('<i class="fa fa-arrow-right me-1"></i> Lanjut ke Serial Numbers & PO Items');
        }
    });

    // Event handler for scan back to step 1
    $('#scanBackToStep1').on('click', function() {
        // Hide step 2 and show step 1
        $('#scan-step-2').hide();
        $('#scan-step-1').show();
        
        // Update step indicators
        $('.scan-step-indicator').removeClass('active completed');
        $('#scan-step1-indicator').addClass('active');
    });

    // Event handler for scan form submission
    $('#submitScanAssetIn').on('click', async function(e) {
        e.preventDefault();
        
        // Collect PO items data
        const poItems = [];
        $('input[name^="scan_poItems"]').each(function() {
            poItems.push($(this).val() || '');
        });

        // Prepare form data
        const formData = new FormData();
        
        // Add asset data from step 1
        Object.keys(scanAssetData).forEach(key => {
            if (scanAssetData[key] !== null && scanAssetData[key] !== undefined) {
                formData.append(key, scanAssetData[key]);
            }
        });

        // Add serial numbers and PO items
        scanGeneratedSerials.forEach((serial, index) => {
            formData.append(`serialNumbers[${index}]`, serial);
            formData.append(`poItems[${index}]`, poItems[index] || '');
        });

        // Add photo if selected
        const fileInput = $('#scan_fotoFile')[0];
        if (fileInput.files.length > 0) {
            formData.append('fotoFile', fileInput.files[0]);
        }

        // Add additional flags
        formData.append('manualSerial', 'false');
        formData.append('useStepProcess', 'true');

        try {
            $(this).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Menyimpan...');
            
            const response = await fetch('/api/AssetIn/CreateWithSteps', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            
            if (result.success || result.remarks) {
                Swal.fire({
                    title: 'Berhasil!',
                    text: result.message || 'Asset In berhasil disimpan',
                    icon: 'success',
                    confirmButtonText: 'OK'
                }).then(() => {
                    // Reset scan forms and go back to step 1
                    resetScanForms();
                    
                    // Refresh notifications
                    if (typeof refreshNotifications === 'function') {
                        refreshNotifications();
                    }
                    
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
                throw new Error(result.message || 'Terjadi kesalahan saat menyimpan data');
            }
            
        } catch (error) {
            Swal.fire({
                title: 'Error!',
                text: error.message,
                icon: 'error',
                confirmButtonText: 'OK'
            });
        } finally {
            $(this).prop('disabled', false).html('<i class="fa fa-save me-1"></i> Submit Asset In');
        }
    });

    // Function to reset scan forms
    function resetScanForms() {
        // Reset form data
        $('#scanAssetDataForm')[0].reset();
        $('#scan_imagePreview').hide();
        scanAssetData = {};
        scanGeneratedSerials = [];
        $('#scan-serialPoMapping').empty();
        
        // Reset step navigation
        $('#scan-step-2').hide();
        $('#scan-step-1').show();
        $('#waiting-scan').show();
        $('#scanAssetDataForm').parent().hide();
        
        // Update step indicators
        $('.scan-step-indicator').removeClass('active completed');
        $('#scan-step1-indicator').addClass('active');
        
        // Reset buttons
        $('#scanNextToStep2').prop('disabled', true);
    }

    // Enable/disable scan next button based on form validation
    function validateScanFormFields() {
        const isValid = 
            $('#scan_namaBarang').val()?.trim() &&
            $('#scan_nomorAsset').val()?.trim() &&
            $('#scan_kategoriBarang').val() &&
            $('#scan_kodeBarang').val()?.trim() &&
            $('#scan_qty').val() && 
            parseInt($('#scan_qty').val()) > 0;
            
        $('#scanNextToStep2').prop('disabled', !isValid);
    }

    // Add event listeners for scan form validation
    $('#scan_namaBarang, #scan_nomorAsset, #scan_kategoriBarang, #scan_kodeBarang, #scan_qty').on('input change', function() {
        validateScanFormFields();
    });

    // Update the original onScanSuccess function to show form properly
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
                    foto: assetData.foto || '',
                    qty: assetData.qty || 1,
                    state: assetData.state || '',
                    district: assetData.district || '',
                    serialNumber: assetData.serialNumber || ''
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
        
        // Show form in step 1
        showScannedFormStep1();
        
        // Validate form fields and enable next button
        validateScanFormFields();
    }

    // Updated function to show scanned form in step 1
    function showScannedFormStep1() {
        $('#waiting-scan').hide();
        $('#scanAssetDataForm').parent().show();
        
        // Ensure we're in step 1
        $('#scan-step-2').hide();
        $('#scan-step-1').show();
        
        // Update step indicators
        $('.scan-step-indicator').removeClass('active completed');
        $('#scan-step1-indicator').addClass('active');
        
        // Focus on quantity field for quick input
        $('#scan_qty').focus().select();
    }

    // Update resetScannerForm to work with step navigation
    function resetScannerForm() {
        resetScanForms();
        
        // Stop scanning if active
        if (isScanning) {
            stopScanning();
        }
    }

});
