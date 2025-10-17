// Unified Asset Details Modal with Pagination and View Toggle
// This file handles the unified modal that can be accessed from both main review and card detail

$(document).ready(function () {
    // ===== UNIFIED MODAL FUNCTIONALITY =====

    // Global variables for unified modal
    window.currentAssetDetails = null;
    window.currentAssetForSerial = null;

    // Tab change event handlers for unified modal
    $('#assetDetailsTabs button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        const targetTab = $(e.target).attr('aria-controls');
        const assetId = window.currentAssetDetails?.id;
        
        if (!assetId) return;
        
        switch(targetTab) {
            case 'serials':
                loadSerialNumbers(assetId);
                break;
            case 'pos':
                loadPoNumbers(assetId);
                break;
        }
    });

    // Function to open asset details modal
    function openAssetDetailsModal(assetId, assetNama, assetData, activeTab = 'overview') {
        // Update modal title
        $('#assetDetailsTitle').text('Asset Details - ' + assetNama);
        
        // Store current asset data
        window.currentAssetDetails = {
            id: assetId,
            nama: assetNama,
            data: assetData
        };
        
        // Populate overview tab with asset data
        populateOverviewTab(assetData);
        
        // Load data based on active tab
        if (activeTab === 'serials') {
            loadSerialNumbers(assetId);
            // Switch to serials tab
            $('#serials-tab').tab('show');
        } else if (activeTab === 'pos') {
            loadPoNumbers(assetId);
            // Switch to POs tab  
            $('#pos-tab').tab('show');
        }
        
        // Show modal
        $('#assetDetailsModal').modal('show');
    }

    // Function to populate overview tab
    function populateOverviewTab(assetData) {
        $('#overview-namaBarang').text(assetData.namaBarang || '-');
        $('#overview-nomorAsset').text(assetData.nomorAsset || '-');
        $('#overview-kodeBarang').text(assetData.kodeBarang || '-');
        $('#overview-kategori').text(assetData.kategoriBarang || '-');
        $('#overview-qty').text(assetData.qty || '-');
        $('#overview-poNumber').text(assetData.poNumber || '-');
        $('#overview-district').text(assetData.district || '-');
        $('#overview-tanggalMasuk').text(assetData.tanggalMasuk ? 
            new Date(assetData.tanggalMasuk).toLocaleDateString('id-ID') : '-');
        
        // Handle foto
        if (assetData.foto && assetData.foto.trim() !== '') {
            $('#overview-foto').attr('src', assetData.foto);
            $('#overview-foto-container').show();
        } else {
            $('#overview-foto-container').hide();
        }
    }

    function loadSerialNumbers(assetId) {
        $.ajax({
            url: '/api/Review/GetSerialNumbers/' + assetId,
            method: 'GET',
            beforeSend: function() {
                $('#serialDetailsTable tbody').html('<tr><td colspan="6" class="text-center"><i class="fa fa-spinner fa-spin"></i> Loading...</td></tr>');
            },
            success: function(response) {
                if (response && response.length > 0) {
                    let html = '';
                    response.forEach(function(serial) {
                        const statusBadge = serial.status === 1 ? 
                            '<span class="badge bg-success">Active</span>' : 
                            '<span class="badge bg-secondary">Inactive</span>';
                            
                        const stateBadge = serial.stateName ? 
                            '<span class="badge bg-info">' + serial.stateName + '</span>' : 
                            '<span class="badge bg-light text-dark">No State</span>';
                            
                        html += '<tr>' +
                            '<td><strong>' + (serial.serialNumber || '-') + '</strong></td>' +
                            '<td>' + statusBadge + '</td>' +
                            '<td>' + stateBadge + '</td>' +
                            '<td>' + (serial.notes || '-') + '</td>' +
                            '<td>' + (serial.createdAt ? moment(serial.createdAt).format("DD/MM/YYYY HH:mm") : '-') + '</td>' +
                            '<td>' +
                                '<button type="button" class="btn btn-sm btn-success me-1 btn-generate-qr-serial" ' +
                                    'data-serial="' + serial.serialNumber + '" ' +
                                    'title="Generate QR Code">' +
                                    '<i class="fa fa-qrcode"></i>' +
                                '</button>' +
                            '</td>' +
                        '</tr>';
                    });
                    $('#serialDetailsTable tbody').html(html);
                    // Update serial count badge
                    $('#serialCount').text(response.length);
                } else {
                    $('#serialDetailsTable tbody').html('<tr><td colspan="6" class="text-center">No serial numbers found</td></tr>');
                    $('#serialCount').text('0');
                }
            },
            error: function() {
                $('#serialDetailsTable tbody').html('<tr><td colspan="6" class="text-center text-danger">Error loading serial numbers</td></tr>');
            }
        });
    }

    function loadPoNumbers(assetId) {
        $.ajax({
            url: '/api/Review/GetPoNumbers/' + assetId,
            method: 'GET',
            beforeSend: function() {
                $('#poDetailsTable tbody').html('<tr><td colspan="5" class="text-center"><i class="fa fa-spinner fa-spin"></i> Loading...</td></tr>');
            },
            success: function(response) {
                if (response && response.length > 0) {
                    let html = '';
                    response.forEach(function(po) {
                        html += '<tr>' +
                            '<td><strong>' + (po.poNumber || '-') + '</strong></td>' +
                            '<td>' + (po.poItem || '-') + '</td>' +
                            '<td>' + (po.createdAt ? moment(po.createdAt).format("DD/MM/YYYY HH:mm") : '-') + '</td>' +
                            '<td>' + (po.createdBy || '-') + '</td>' +
                            '<td>-</td>' +
                        '</tr>';
                    });
                    $('#poDetailsTable tbody').html(html);
                    // Update PO count badge
                    $('#poCount').text(response.length);
                } else {
                    $('#poDetailsTable tbody').html('<tr><td colspan="5" class="text-center">No PO items found</td></tr>');
                    $('#poCount').text('0');
                }
            },
            error: function() {
                $('#poDetailsTable tbody').html('<tr><td colspan="4" class="text-center text-danger">Error loading PO items</td></tr>');
            }
        });
    }

    // ===== EVENT HANDLERS =====

    // Handle QR generation for serial numbers  
    $(document).on('click', '.btn-generate-qr-serial', function() {
        const serialNumber = $(this).data('serial');
        
        if (!serialNumber) {
            Swal.fire({
                title: 'Error!',
                text: 'Serial number not found.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Get asset data from global variable
        const assetData = window.currentAssetDetails?.data;
        
        if (!assetData) {
            Swal.fire({
                title: 'Error!',
                text: 'Asset data not found.',
                icon: 'error',
                confirmButtonText: 'OK'
            });
            return;
        }

        // Use the same QR generation function as main review list
        generateQRFromAssetSerial(assetData.namaBarang, assetData.nomorAsset, assetData.kodeBarang, assetData.kategoriBarang, assetData.qty, serialNumber);
    });

    // Function untuk generate QR dari asset data dengan serial number
    function generateQRFromAssetSerial(namaBarang, nomorAsset, kodeBarang, kategoriBarang, qty, serialNumber) {
        // Encode parameters untuk URL - sama seperti generateQRFromAsset tapi dengan serial number
        const params = new URLSearchParams({
            nama: namaBarang || '',
            nomor: nomorAsset || '',
            kode: kodeBarang || '',
            kategori: kategoriBarang || '',
            qty: '1',  // Dari serial specific, qty harus 1
            serial: serialNumber || '',  // Tambahan serial number
            state: '',  // State tidak tersedia di asset detail modal
            district: '',  // District tidak tersedia di asset detail modal
        });

        // Redirect to QR generator page  
        window.open(`/Asset/GenerateQR?${params.toString()}`, '_blank');
    }

    // Expose functions to global scope for use by other scripts
    window.openAssetDetailsModal = openAssetDetailsModal;
    window.loadSerialNumbers = loadSerialNumbers;
    window.loadPoNumbers = loadPoNumbers;
    window.populateOverviewTab = populateOverviewTab;

    // Additional style for better UX
    $('<style>').prop('type', 'text/css').html(`
        .swal-backdrop-blur {
            filter: blur(3px) !important;
            pointer-events: none !important;
            transition: filter 0.3s ease !important;
        }
        
        .swal2-container {
            z-index: 9999 !important;
        }
        
        .swal2-backdrop-show {
            backdrop-filter: blur(3px) !important;
            background-color: rgba(0, 0, 0, 0.75) !important;
        }
    `).appendTo('head');
    
});
