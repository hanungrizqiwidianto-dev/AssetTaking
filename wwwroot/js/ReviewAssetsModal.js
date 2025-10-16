// Modal Asset Details with Pagination and View Toggle
$(document).ready(function () {
    // Modal state variables
    let modalCurrentView = 'card';
    let modalCurrentPage = 1;
    let modalPageSize = 6;
    let modalCurrentKodeBarang = '';
    let modalCurrentNomorAsset = '';
    let modalAssetsData = [];
    let modalPaginationData = {};

    // Initialize modal events
    initializeModalEvents();

    function initializeModalEvents() {
        // Modal view toggle buttons
        $('#modalCardViewBtn').on('click', function() {
            switchToModalCardView();
        });

        $('#modalListViewBtn').on('click', function() {
            switchToModalListView();
        });

        // Modal page size change
        $('#modalPageSizeSelect').on('change', function() {
            modalPageSize = parseInt($(this).val());
            modalCurrentPage = 1;
            loadModalAssetDetails();
        });

        // Override the original detail button click handler
        $(document).off('click', '.btn-detail').on('click', '.btn-detail', function () {
            modalCurrentKodeBarang = $(this).data('kode');
            modalCurrentNomorAsset = $(this).data('nomor');
            
            // Reset modal state
            modalCurrentPage = 1;
            modalCurrentView = 'card';
            modalPageSize = 6;
            
            // Reset view buttons
            $('#modalCardViewBtn').removeClass('btn-outline-primary').addClass('btn-primary');
            $('#modalListViewBtn').removeClass('btn-primary').addClass('btn-outline-primary');
            $('#modalCardView').show();
            $('#modalListView').hide();
            $('#modalPageSizeSelect').val('6');
            
            // Clear previous data
            $("#assetDetailsContainer").empty();
            $("#assetDetailsTable tbody").empty();
            
            // Show modal
            $('#assetDetailModal').modal('show');
            
            // Load asset details
            loadModalAssetDetails();
        });
    }

    function switchToModalCardView() {
        modalCurrentView = 'card';
        $('#modalCardViewBtn').removeClass('btn-outline-primary').addClass('btn-primary');
        $('#modalListViewBtn').removeClass('btn-primary').addClass('btn-outline-primary');
        $('#modalCardView').show();
        $('#modalListView').hide();
        loadModalAssetDetails();
    }

    function switchToModalListView() {
        modalCurrentView = 'list';
        $('#modalListViewBtn').removeClass('btn-outline-primary').addClass('btn-primary');
        $('#modalCardViewBtn').removeClass('btn-primary').addClass('btn-outline-primary');
        $('#modalCardView').hide();
        $('#modalListView').show();
        loadModalAssetDetails();
    }

    function loadModalAssetDetails() {
        // Show loading
        if (modalCurrentView === 'card') {
            $("#assetDetailsContainer").html('<div class="col-12 text-center py-4"><i class="fa fa-spinner fa-spin fa-2x"></i><br><small>Loading asset details...</small></div>');
        } else {
            $("#assetDetailsTable tbody").html('<tr><td colspan="10" class="text-center"><i class="fa fa-spinner fa-spin"></i> Loading...</td></tr>');
        }

        $.ajax({
            url: '/api/Review/GetAssetDetailsPaginated',
            type: 'GET',
            data: {
                kodeBarang: modalCurrentKodeBarang,
                nomorAsset: modalCurrentNomorAsset,
                page: modalCurrentPage,
                pageSize: modalPageSize
            },
            success: function (response) {
                modalAssetsData = response.data;
                modalPaginationData = response.pagination;
                
                if (modalCurrentView === 'card') {
                    renderModalCardView();
                } else {
                    renderModalListView();
                }
                
                renderModalPagination();
                updateModalPaginationInfo();
            },
            error: function (xhr, status, error) {
                console.error('Error loading asset details:', error);
                if (modalCurrentView === 'card') {
                    $("#assetDetailsContainer").html('<div class="col-12 text-center py-4 text-danger"><i class="fa fa-exclamation-triangle fa-2x"></i><br><h5 class="mt-2">Error loading asset details</h5></div>');
                } else {
                    $("#assetDetailsTable tbody").html('<tr><td colspan="10" class="text-center text-danger">Error loading data</td></tr>');
                }
            }
        });
    }

    function renderModalCardView() {
        var html = '';
        
        if (modalAssetsData && modalAssetsData.length > 0) {
            modalAssetsData.forEach(function (asset, index) {
                var statusBadge = '';
                if (asset.status === 1) {
                    statusBadge = '<span class="badge bg-success">Asset In</span>';
                } else if (asset.status === 2) {
                    statusBadge = '<span class="badge bg-warning">Asset Out</span>';
                } else {
                    statusBadge = '<span class="badge bg-secondary">Unknown</span>';
                }
                
                var fotoHtml = asset.foto ? 
                    '<img src="' + asset.foto + '" alt="Asset Photo" class="img-fluid rounded" style="max-height: 150px;">' : 
                    '<div class="bg-light rounded d-flex align-items-center justify-content-center" style="height: 150px;"><i class="fa fa-image fa-2x text-muted"></i></div>';
                    
                html += '<div class="col-md-6 mb-4">' +
                        '<div class="block block-rounded h-100">' +
                            '<div class="block-header block-header-default">' +
                                '<h4 class="block-title">Asset #' + asset.id + '</h4>' +
                                '<div class="block-options">' +
                                    statusBadge +
                                '</div>' +
                            '</div>' +
                            '<div class="block-content">' +
                                '<div class="row">' +
                                    '<div class="col-4">' +
                                        fotoHtml +
                                    '</div>' +
                                    '<div class="col-8">' +
                                        '<table class="table table-borderless table-sm">' +
                                            '<tr>' +
                                                '<td><strong>Nama Barang:</strong></td>' +
                                                '<td>' + (asset.namaBarang || '-') + '</td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>Kode Barang:</strong></td>' +
                                                '<td><span class="badge bg-primary">' + (asset.kodeBarang || '-') + '</span></td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>Nomor Asset:</strong></td>' +
                                                '<td><span class="badge bg-info">' + (asset.nomorAsset || '-') + '</span></td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>Kategori:</strong></td>' +
                                                '<td>' + (asset.kategoriBarang || '-') + '</td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>Quantity:</strong></td>' +
                                                '<td><span class="badge bg-dark">' + (asset.qty || 0) + '</span></td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>State:</strong></td>' +
                                                '<td>' + (asset.state || '-') + '</td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>District:</strong></td>' +
                                                '<td>' + (asset.statusText === 'Asset In' ? (asset.dstrctIn || '-') : (asset.dstrctOut || '-')) + '</td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>PO Numbers:</strong></td>' +
                                                '<td>' + 
                                                    '<button type="button" class="btn btn-sm btn-outline-secondary btn-view-pos" ' +
                                                        'data-id="' + asset.id + '" ' +
                                                        'data-nama="' + asset.namaBarang + '">' +
                                                        '<i class="fa fa-file-text"></i> View PO' +
                                                    '</button>' +
                                                '</td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>Serial Numbers:</strong></td>' +
                                                '<td>' + 
                                                    '<button type="button" class="btn btn-sm btn-outline-primary btn-view-serials" ' +
                                                        'data-id="' + asset.id + '" ' +
                                                        'data-nama="' + asset.namaBarang + '">' +
                                                        '<i class="fa fa-list"></i> View Serials' +
                                                    '</button>' +
                                                '</td>' +
                                            '</tr>' +
                                            '<tr>' +
                                                '<td><strong>Tanggal Masuk:</strong></td>' +
                                                '<td>' + (asset.tanggalMasuk ? moment(asset.tanggalMasuk).format("DD/MM/YYYY") : '-') + '</td>' +
                                            '</tr>' +
                                        '</table>' +
                                    '</div>' +
                                '</div>' +
                                '<hr>' +
                                '<div class="row">' +
                                    '<div class="col-6">' +
                                        '<small class="text-muted">' +
                                            '<strong>Created:</strong><br>' +
                                            (asset.createdAt ? moment(asset.createdAt).format("DD/MM/YYYY HH:mm") : '-') + '<br>' +
                                            'by: ' + (asset.createdBy || '-') +
                                        '</small>' +
                                    '</div>' +
                                    '<div class="col-6">' +
                                        '<small class="text-muted">' +
                                            '<strong>Modified:</strong><br>' +
                                            (asset.modifiedAt ? moment(asset.modifiedAt).format("DD/MM/YYYY HH:mm") : '-') + '<br>' +
                                            'by: ' + (asset.modifiedBy || '-') +
                                        '</small>' +
                                    '</div>' +
                                '</div>' +
                                '<div class="text-end mt-3">' +
                                    '<button type="button" class="btn btn-sm btn-warning btn-edit-asset" ' +
                                        'data-id="' + asset.id + '"' +
                                        'data-nama="' + asset.namaBarang + '"' +
                                        'data-nomor="' + asset.nomorAsset + '" ' +
                                        'data-kode="' + asset.kodeBarang + '"' +
                                        'data-kategori="' + asset.kategoriBarang + '"' +
                                        'data-qty="' + asset.qty + '"' +
                                        'data-foto="' + (asset.foto || '') + '"' +
                                        'title="Edit Asset">' +
                                        '<i class="fa fa-edit"></i> Edit' +
                                    '</button>' +
                                    '<button type="button" class="btn btn-sm btn-danger btn-delete-asset ms-2" ' +
                                        'data-id="' + asset.id + '"' +
                                        'data-nama="' + asset.namaBarang + '"' +
                                        'title="Delete Asset">' +
                                        '<i class="fa fa-trash"></i> Delete' +
                                    '</button>' +
                                '</div>' +
                            '</div>' +
                        '</div>' +
                    '</div>';
            });
        } else {
            html = '<div class="col-12 text-center py-5"><i class="fa fa-info-circle fa-2x text-muted"></i><br><h5 class="mt-2">No asset details found</h5></div>';
        }
        
        $("#assetDetailsContainer").html(html);
    }

    function renderModalListView() {
        var tbody = '';
        
        if (modalAssetsData && modalAssetsData.length > 0) {
            modalAssetsData.forEach(function(asset) {
                var statusBadge = '';
                if (asset.status === 1) {
                    statusBadge = '<span class="badge bg-success">Asset In</span>';
                } else if (asset.status === 2) {
                    statusBadge = '<span class="badge bg-warning">Asset Out</span>';
                } else {
                    statusBadge = '<span class="badge bg-secondary">Unknown</span>';
                }

                var fotoHtml = asset.foto ? 
                    '<img src="' + asset.foto + '" alt="Asset Photo" class="img-thumbnail" style="max-height: 50px; max-width: 50px;">' : 
                    '<div class="bg-light rounded d-flex align-items-center justify-content-center" style="height: 50px; width: 50px;"><i class="fa fa-image text-muted"></i></div>';

                tbody += '<tr class="text-center">' +
                    '<td>' + asset.id + '</td>' +
                    '<td>' + statusBadge + '</td>' +
                    '<td>' + fotoHtml + '</td>' +
                    '<td class="text-start">' + (asset.namaBarang || '-') + '</td>' +
                    '<td><span class="badge bg-primary">' + (asset.kodeBarang || '-') + '</span></td>' +
                    '<td><span class="badge bg-info">' + (asset.nomorAsset || '-') + '</span></td>' +
                    '<td>' + (asset.kategoriBarang || '-') + '</td>' +
                    '<td><span class="badge bg-dark">' + (asset.qty || 0) + '</span></td>' +
                    '<td>' + (asset.state || '-') + '</td>' +
                    '<td>' + (asset.statusText === 'Asset In' ? (asset.dstrctIn || '-') : (asset.dstrctOut || '-')) + '</td>' +
                    '<td>' + 
                        '<button type="button" class="btn btn-sm btn-outline-secondary btn-view-pos me-1" ' +
                            'data-id="' + asset.id + '" ' +
                            'data-nama="' + asset.namaBarang + '" title="View PO Numbers">' +
                            '<i class="fa fa-file-text"></i>' +
                        '</button>' +
                        '<button type="button" class="btn btn-sm btn-outline-primary btn-view-serials" ' +
                            'data-id="' + asset.id + '" ' +
                            'data-nama="' + asset.namaBarang + '" title="View Serial Numbers">' +
                            '<i class="fa fa-list"></i>' +
                        '</button>' +
                    '</td>' +
                    '<td>' + (asset.tanggalMasuk ? moment(asset.tanggalMasuk).format("DD/MM/YYYY") : '-') + '</td>' +
                    '<td>' +
                        '<div class="btn-group" role="group">' +
                            '<button type="button" class="btn btn-sm btn-warning btn-edit-asset" ' +
                                'data-id="' + asset.id + '"' +
                                'data-nama="' + asset.namaBarang + '"' +
                                'data-nomor="' + asset.nomorAsset + '" ' +
                                'data-kode="' + asset.kodeBarang + '"' +
                                'data-kategori="' + asset.kategoriBarang + '"' +
                                'data-qty="' + asset.qty + '"' +
                                'data-foto="' + (asset.foto || '') + '"' +
                                'title="Edit Asset">' +
                                '<i class="fa fa-edit"></i>' +
                            '</button>' +
                            '<button type="button" class="btn btn-sm btn-danger btn-delete-asset ms-1" ' +
                                'data-id="' + asset.id + '"' +
                                'data-nama="' + asset.namaBarang + '"' +
                                'title="Delete Asset">' +
                                '<i class="fa fa-trash"></i>' +
                            '</button>' +
                        '</div>' +
                    '</td>' +
                '</tr>';
            });
        } else {
            tbody = '<tr><td colspan="16" class="text-center">No data available</td></tr>';
        }
        
        $('#assetDetailsTable tbody').html(tbody);
    }

    function renderModalPagination() {
        if (!modalPaginationData || modalPaginationData.totalPages <= 1) {
            $('#modalPaginationNav').empty();
            return;
        }

        var paginationHtml = '';
        
        // Previous button
        if (modalPaginationData.hasPreviousPage) {
            paginationHtml += '<li class="page-item">' +
                '<a class="page-link" href="#" data-page="' + (modalCurrentPage - 1) + '">' +
                    '<i class="fa fa-angle-left"></i>' +
                '</a>' +
            '</li>';
        }

        // Page numbers
        var startPage = Math.max(1, modalCurrentPage - 2);
        var endPage = Math.min(modalPaginationData.totalPages, modalCurrentPage + 2);

        if (startPage > 1) {
            paginationHtml += '<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>';
            if (startPage > 2) {
                paginationHtml += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }
        }

        for (var i = startPage; i <= endPage; i++) {
            paginationHtml += '<li class="page-item ' + (i === modalCurrentPage ? 'active' : '') + '">' +
                '<a class="page-link" href="#" data-page="' + i + '">' + i + '</a>' +
            '</li>';
        }

        if (endPage < modalPaginationData.totalPages) {
            if (endPage < modalPaginationData.totalPages - 1) {
                paginationHtml += '<li class="page-item disabled"><span class="page-link">...</span></li>';
            }
            paginationHtml += '<li class="page-item"><a class="page-link" href="#" data-page="' + modalPaginationData.totalPages + '">' + modalPaginationData.totalPages + '</a></li>';
        }

        // Next button
        if (modalPaginationData.hasNextPage) {
            paginationHtml += '<li class="page-item">' +
                '<a class="page-link" href="#" data-page="' + (modalCurrentPage + 1) + '">' +
                    '<i class="fa fa-angle-right"></i>' +
                '</a>' +
            '</li>';
        }

        $('#modalPaginationNav').html(paginationHtml);

        // Bind pagination click events
        $('#modalPaginationNav .page-link').on('click', function(e) {
            e.preventDefault();
            var page = parseInt($(this).data('page'));
            if (page && page !== modalCurrentPage) {
                modalCurrentPage = page;
                loadModalAssetDetails();
            }
        });
    }

    function updateModalPaginationInfo() {
        if (!modalPaginationData) {
            $('#modalPaginationInfo').text('Loading...');
            return;
        }

        var start = ((modalCurrentPage - 1) * modalPageSize) + 1;
        var end = Math.min(modalCurrentPage * modalPageSize, modalPaginationData.totalRecords);
        
        $('#modalPaginationInfo').text('Showing ' + start + ' to ' + end + ' of ' + modalPaginationData.totalRecords + ' results');
    }

    // Handle view serials button click
    $(document).on('click', '.btn-view-serials', function() {
        const assetId = $(this).data('id');
        const assetNama = $(this).data('nama');
        
        // Find asset data from modalAssetsData
        const assetData = modalAssetsData.find(asset => asset.id == assetId);
        
        // Store asset data globally for QR generation
        window.currentAssetForSerial = assetData;
        
        // Update modal title
        $('#serialDetailsModal .block-title').text('Serial Numbers - ' + assetNama);
        
        // Load serial numbers for this asset
        loadSerialNumbers(assetId);
        
        // Show modal
        $('#serialDetailsModal').modal('show');
    });

    // Handle view PO numbers button click
    $(document).on('click', '.btn-view-pos', function() {
        const assetId = $(this).data('id');
        const assetNama = $(this).data('nama');
        
        // Update modal title
        $('#poDetailsModal .block-title').text('PO Numbers - ' + assetNama);
        
        // Load PO numbers for this asset
        loadPoNumbers(assetId);
        
        // Show modal
        $('#poDetailsModal').modal('show');
    });

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
                                '<button type="button" class="btn btn-sm btn-success btn-generate-qr-serial" ' +
                                    'data-serial="' + serial.serialNumber + '" ' +
                                    'title="Generate QR Code">' +
                                    '<i class="fa fa-qrcode"></i> QR' +
                                '</button>' +
                            '</td>' +
                        '</tr>';
                    });
                    $('#serialDetailsTable tbody').html(html);
                } else {
                    $('#serialDetailsTable tbody').html('<tr><td colspan="6" class="text-center">No serial numbers found</td></tr>');
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
                $('#poDetailsTable tbody').html('<tr><td colspan="4" class="text-center"><i class="fa fa-spinner fa-spin"></i> Loading...</td></tr>');
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
                        '</tr>';
                    });
                    $('#poDetailsTable tbody').html(html);
                } else {
                    $('#poDetailsTable tbody').html('<tr><td colspan="4" class="text-center">No PO numbers found</td></tr>');
                }
            },
            error: function() {
                $('#poDetailsTable tbody').html('<tr><td colspan="4" class="text-center text-danger">Error loading PO numbers</td></tr>');
            }
        });
    }

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
        const assetData = window.currentAssetForSerial;
        
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
            fromReview: 'true',
            fromSerial: 'true'  // Flag untuk menandakan generate dari serial specific
        });
        
        // Redirect ke halaman Generate QR dengan parameter
        window.location.href = `/Asset/GenerateQR?${params.toString()}`;
    }
});