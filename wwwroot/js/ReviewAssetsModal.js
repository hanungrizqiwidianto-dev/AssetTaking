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
            tbody = '<tr><td colspan="10" class="text-center">No data available</td></tr>';
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
});