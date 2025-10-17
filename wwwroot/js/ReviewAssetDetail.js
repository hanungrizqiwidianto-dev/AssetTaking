const ReviewAssetDetail = {
    currentKodeBarang: null,
    currentNomorAsset: null,
    currentPage: 1,
    pageSize: 6,
    totalItems: 0,
    allAssets: [],
    currentView: 'card',

    init: function(kodeBarang, nomorAsset) {
        console.log('Initializing ReviewAssetDetail with:', { kodeBarang, nomorAsset });
        this.currentKodeBarang = kodeBarang;
        this.currentNomorAsset = nomorAsset;
        
        this.setupEventHandlers();
        this.loadAssetDetails();
    },

    setupEventHandlers: function() {
        // Remove any existing event handlers to prevent duplicates
        $('#cardViewBtn').off('click').on('click', () => {
            this.switchView('card');
        });
        
        $('#listViewBtn').off('click').on('click', () => {
            this.switchView('list');
        });

        // Page size selector
        $('#pageSizeSelect').off('change').on('change', (e) => {
            this.pageSize = parseInt(e.target.value);
            this.currentPage = 1;
            this.renderCurrentView();
        });

        // Setup pagination click handler (remove previous handlers)
        $(document).off('click', '.pagination .page-link').on('click', '.pagination .page-link', (e) => {
            e.preventDefault();
            const page = $(e.target).data('page');
            if (page && page !== this.currentPage) {
                this.currentPage = page;
                this.renderCurrentView();
            }
        });
    },

    loadAssetDetails: function() {
        $('#loadingIndicator').show();
        $('#assetDetailContent').hide();
        $('#errorMessage').hide();

        $.ajax({
            url: '/Api/Review/GetAssetOverview',
            type: 'GET',
            data: {
                kodeBarang: this.currentKodeBarang,
                nomorAsset: this.currentNomorAsset
            },
            success: (response) => {
                if (response.success && response.data) {
                    this.allAssets = response.data.assets || [];
                    this.totalItems = this.allAssets.length;
                    
                    // Update overview information
                    if (response.data.overview) {
                        this.updateOverview(response.data.overview);
                    }
                    
                    // Only render if we have data
                    if (this.allAssets.length > 0) {
                        this.renderCurrentView();
                    } else {
                        this.showError('No asset details found for this asset.');
                        return;
                    }
                    
                    $('#loadingIndicator').hide();
                    $('#assetDetailContent').show();
                } else {
                    this.showError(response.message || 'Failed to load asset details');
                }
            },
            error: (xhr, status, error) => {
                this.showError('Network error occurred while loading asset details');
            }
        });
    },

    updateOverview: function(overview) {
        // Update title
        $('#assetDetailTitle').text(`${overview.namaBarang} - ${overview.kodeBarang}`);
        
        // Update pagination info
        this.updatePaginationInfo();
    },

    switchView: function(view) {
        this.currentView = view;
        
        // Update button states
        if (view === 'card') {
            $('#cardViewBtn').removeClass('btn-outline-primary').addClass('btn-primary');
            $('#listViewBtn').removeClass('btn-primary').addClass('btn-outline-primary');
            $('#cardView').show();
            $('#listView').hide();
        } else {
            $('#cardViewBtn').removeClass('btn-primary').addClass('btn-outline-primary');
            $('#listViewBtn').removeClass('btn-outline-primary').addClass('btn-primary');
            $('#cardView').hide();
            $('#listView').show();
        }
        
        this.renderCurrentView();
    },

    renderCurrentView: function() {
        // Check if containers exist
        if (!$('#assetDetailsContainer').length) {
            console.error('Card container not found!');
            return;
        }
        if (!$('#assetDetailsTable').length) {
            console.error('Table container not found!');
            return;
        }
        
        if (this.currentView === 'card') {
            this.renderCardView();
        } else {
            this.renderListView();
        }
        this.renderPagination();
        this.updatePaginationInfo();
    },

    renderCardView: function() {
        const startIndex = (this.currentPage - 1) * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        const assetsToShow = this.allAssets.slice(startIndex, endIndex);
        
        let cardsHtml = '';
        
        assetsToShow.forEach((asset, index) => {
            const globalIndex = startIndex + index + 1;
            const statusBadge = this.getStatusBadge(asset.statusAsset);
            // Check for photo from Asset In or Asset Out
            const hasImage = asset.foto && asset.foto !== '' && asset.foto !== null;
            
            // Create image section only if there's an uploaded photo
            const imageSection = hasImage ? `
                <!-- Image Section -->
                <div class="text-center py-3 bg-light" style="border-bottom: 1px solid #eee;">
                    <div class="card-img-wrapper position-relative d-inline-block">
                        <img src="${asset.foto}" class="rounded-3 shadow-sm" 
                             style="width: 120px; height: 120px; object-fit: cover; border: 3px solid #fff;" 
                             alt="Asset Image">
                        <div class="position-absolute bottom-0 end-0 translate-middle">
                            <span class="badge bg-primary rounded-pill px-2 py-1">
                                <i class="fa fa-boxes me-1"></i>${asset.qty || 0}
                            </span>
                        </div>
                    </div>
                </div>
            ` : `
                <!-- No Image Section -->
                <div class="text-center py-3 bg-light" style="border-bottom: 1px solid #eee;">
                    <div class="d-flex align-items-center justify-content-center" style="height: 120px;">
                        <div class="text-muted">
                            <i class="fa fa-image fa-3x mb-2"></i>
                            <br><small>No Image</small>
                        </div>
                    </div>
                    <div class="position-absolute bottom-0 end-0 translate-middle">
                        <span class="badge bg-primary rounded-pill px-2 py-1">
                            <i class="fa fa-boxes me-1"></i>${asset.qty || 0}
                        </span>
                    </div>
                </div>
            `;
            
            cardsHtml += `
                <div class="col-md-6 col-xl-4 mb-4">
                    <div class="card shadow-sm border-0 h-100" style="transition: all 0.3s ease; border-radius: 15px;">
                        <div class="position-relative">
                            <div class="card-header text-white border-0" style="border-radius: 15px 15px 0 0; background: linear-gradient(135deg, #17a2b8 0%, #20c997 100%) !important;">
                                <div class="d-flex justify-content-between align-items-center">
                                    <h6 class="card-title mb-0 fw-bold">
                                        <i class="fa fa-cube me-2"></i>Asset #${globalIndex}
                                    </h6>
                                    ${statusBadge}
                                </div>
                            </div>
                            <div class="position-absolute" style="top: 45px; right: 10px; z-index: 10;">
                                <span class="badge bg-light text-dark px-2 py-1 shadow-sm" style="font-size: 0.75rem; border-radius: 20px;">
                                    <i class="fa fa-tag me-1"></i>${asset.kategori || '-'}
                                </span>
                            </div>
                        </div>
                        
                        <div class="card-body p-0">
                            ${imageSection}
                            
                            <!-- Content Section -->
                            <div class="p-3">
                                <div class="mb-3">
                                    <h5 class="card-title text-primary mb-1 fw-bold" style="font-size: 1.1rem; line-height: 1.3;">
                                        ${asset.namaBarang || '-'}
                                    </h5>
                                    <p class="text-muted small mb-0">
                                        <i class="fa fa-qrcode me-1"></i>${asset.kodeBarang || '-'} / ${asset.nomorAsset || '-'}
                                    </p>
                                </div>
                                
                                <div class="row g-2 mb-3">
                                    <div class="col-6">
                                        <div class="bg-light rounded p-2 text-center">
                                            <div class="text-muted small">PO Number</div>
                                            <div class="fw-semibold text-dark" style="font-size: 0.9rem;">${asset.poNumber || '-'}</div>
                                        </div>
                                    </div>
                                    <div class="col-6">
                                        <div class="bg-light rounded p-2 text-center">
                                            <div class="text-muted small">District</div>
                                            <div class="fw-semibold text-dark" style="font-size: 0.9rem;">${asset.district || '-'}</div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="d-flex align-items-center justify-content-between">
                                    <div class="d-flex align-items-center text-muted small">
                                        <i class="fa fa-calendar-alt me-1"></i>
                                        ${asset.tanggalMasuk ? new Date(asset.tanggalMasuk).toLocaleDateString('id-ID') : '-'}
                                    </div>
                                    <div class="d-flex align-items-center">
                                        <button class="btn btn-outline-primary btn-sm rounded-pill px-3" 
                                                onclick="viewAssetDetail('${asset.kodeBarang}', '${asset.nomorAsset}')" 
                                                title="View Details">
                                            <i class="fa fa-eye me-1"></i>Detail
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        });
        
        $('#assetDetailsContainer').html(cardsHtml);
        
        // Add enhanced hover effects and modal styling using CSS
        if (!$('#cardHoverStyles').length) {
            $('head').append(`
                <style id="cardHoverStyles">
                    .card:hover {
                        transform: translateY(-8px);
                        box-shadow: 0 15px 35px rgba(0,0,0,0.2) !important;
                        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
                    }
                    .card {
                        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
                        position: relative;
                        overflow: hidden;
                    }
                    .card::before {
                        content: '';
                        position: absolute;
                        top: 0;
                        left: 0;
                        right: 0;
                        height: 3px;
                        background: linear-gradient(45deg, #667eea, #764ba2);
                        z-index: 1;
                    }
                    .bg-gradient-primary {
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%) !important;
                    }
                    .card-img-wrapper {
                        overflow: hidden;
                        border-radius: 10px;
                        position: relative;
                    }
                    .card-img-wrapper img {
                        transition: transform 0.3s ease;
                    }
                    .card:hover .card-img-wrapper img {
                        transform: scale(1.05);
                    }
                    
                    /* Enhanced Modal Styling */
                    .modal-content {
                        border: none;
                        border-radius: 20px;
                        overflow: hidden;
                        box-shadow: 0 25px 50px rgba(0, 0, 0, 0.25);
                    }
                    .modal-header {
                        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                        color: white;
                        border: none;
                        padding: 25px 30px;
                    }
                    .modal-title {
                        font-weight: 600;
                        font-size: 1.4rem;
                        text-shadow: 0 2px 4px rgba(0,0,0,0.2);
                    }
                    .modal-body {
                        padding: 30px;
                        background: linear-gradient(180deg, #f8f9fa 0%, #e9ecef 100%);
                    }
                    .asset-detail-section {
                        background: white;
                        border-radius: 15px;
                        padding: 20px;
                        margin-bottom: 20px;
                        box-shadow: 0 8px 25px rgba(0,0,0,0.08);
                        border-left: 4px solid #667eea;
                    }
                    .asset-detail-section h6 {
                        color: #667eea;
                        font-weight: 600;
                        margin-bottom: 15px;
                        padding-bottom: 10px;
                        border-bottom: 2px solid #e9ecef;
                    }
                    .asset-detail-row {
                        padding: 8px 0;
                        border-radius: 8px;
                        transition: background-color 0.2s ease;
                    }
                    .asset-detail-row:hover {
                        background-color: rgba(102, 126, 234, 0.05);
                    }
                    
                    /* Badge Enhancements */
                    .badge {
                        font-size: 0.75rem;
                        font-weight: 600;
                        padding: 6px 12px;
                        text-transform: uppercase;
                        letter-spacing: 0.5px;
                    }
                    .badge.rounded-pill {
                        border-radius: 50px !important;
                    }
                </style>
            `);
        }
    },

    renderListView: function() {
        const startIndex = (this.currentPage - 1) * this.pageSize;
        const endIndex = startIndex + this.pageSize;
        const assetsToShow = this.allAssets.slice(startIndex, endIndex);
        
        let rowsHtml = '';
        
        assetsToShow.forEach((asset, index) => {
            const globalIndex = startIndex + index + 1;
            const statusBadge = this.getStatusBadge(asset.statusAsset);
            const hasImage = asset.foto && asset.foto !== '';
            
            // Create image cell - show image only if exists, otherwise show placeholder text
            const imageCell = hasImage ? 
                `<img src="/uploads/${asset.foto}" class="img-thumbnail" style="width: 40px; height: 40px; object-fit: cover;" alt="Asset">` :
                `<span class="text-muted small"><i class="fa fa-image"></i> No Image</span>`;
            
            rowsHtml += `
                <tr>
                    <td class="text-center fw-semibold">${globalIndex}</td>
                    <td class="text-center">${statusBadge}</td>
                    <td class="text-center">
                        ${imageCell}
                    </td>
                    <td>${asset.namaBarang || '-'}</td>
                    <td>${asset.kodeBarang || '-'}</td>
                    <td>${asset.nomorAsset || '-'}</td>
                    <td>${asset.kategori || '-'}</td>
                    <td class="text-center"><span class="badge bg-primary">${asset.qty || 0}</span></td>
                    <td>${asset.poNumber || '-'}</td>
                    <td>${asset.district || '-'}</td>
                    <td>${asset.tanggalMasuk ? new Date(asset.tanggalMasuk).toLocaleDateString('id-ID') : '-'}</td>
                </tr>
            `;
        });
        
        $('#assetDetailsTable tbody').html(rowsHtml);
    },

    getStatusBadge: function(status) {
        if (!status) return '<span class="badge bg-secondary rounded-pill px-2 py-1"><i class="fa fa-minus me-1"></i>-</span>';
        
        const statusLower = status.toLowerCase();
        if (statusLower.includes('in')) {
            return '<span class="badge bg-success rounded-pill px-2 py-1"><i class="fa fa-arrow-down me-1"></i>Asset In</span>';
        } else if (statusLower.includes('out')) {
            return '<span class="badge bg-warning text-dark rounded-pill px-2 py-1"><i class="fa fa-arrow-up me-1"></i>Asset Out</span>';
        } else {
            return `<span class="badge bg-info rounded-pill px-2 py-1"><i class="fa fa-question me-1"></i>${status}</span>`;
        }
    },

    updatePaginationInfo: function() {
        const startIndex = (this.currentPage - 1) * this.pageSize + 1;
        const endIndex = Math.min(this.currentPage * this.pageSize, this.totalItems);
        
        const infoText = `Showing ${startIndex} to ${endIndex} of ${this.totalItems} assets`;
        console.log('Updating pagination info:', infoText);
        $('#paginationInfo').text(infoText);
    },

    renderPagination: function() {
        const totalPages = Math.ceil(this.totalItems / this.pageSize);
        let paginationHtml = '';
        
        if (totalPages > 1) {
            // Previous button
            paginationHtml += `
                <li class="page-item ${this.currentPage === 1 ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${this.currentPage - 1}" aria-label="Previous">
                        <span aria-hidden="true">&laquo;</span>
                    </a>
                </li>
            `;
            
            // Page numbers
            const startPage = Math.max(1, this.currentPage - 2);
            const endPage = Math.min(totalPages, this.currentPage + 2);
            
            if (startPage > 1) {
                paginationHtml += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="1">1</a>
                    </li>
                `;
                if (startPage > 2) {
                    paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
                }
            }
            
            for (let i = startPage; i <= endPage; i++) {
                paginationHtml += `
                    <li class="page-item ${i === this.currentPage ? 'active' : ''}">
                        <a class="page-link" href="#" data-page="${i}">${i}</a>
                    </li>
                `;
            }
            
            if (endPage < totalPages) {
                if (endPage < totalPages - 1) {
                    paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
                }
                paginationHtml += `
                    <li class="page-item">
                        <a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a>
                    </li>
                `;
            }
            
            // Next button
            paginationHtml += `
                <li class="page-item ${this.currentPage === totalPages ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${this.currentPage + 1}" aria-label="Next">
                        <span aria-hidden="true">&raquo;</span>
                    </a>
                </li>
            `;
        }
        
        $('#paginationNav').html(paginationHtml);
    },

    showError: function(message) {
        $('#loadingIndicator').hide();
        $('#assetDetailContent').hide();
        $('#errorText').text(message);
        $('#errorMessage').show();
    }
};

// Global function to show asset detail modal
function viewAssetDetail(kodeBarang, nomorAsset) {
    // Find the asset in the current data
    const asset = ReviewAssetDetail.allAssets.find(a => 
        a.kodeBarang === kodeBarang && a.nomorAsset === nomorAsset
    );
    
    if (!asset) {
        Swal.fire({
            icon: 'error',
            title: 'Asset Not Found',
            text: 'Asset detail not found in current data.'
        });
        return;
    }
    
    const hasImage = asset.foto && asset.foto !== '';
    const statusBadge = ReviewAssetDetail.getStatusBadge(asset.statusAsset);
    
    // Create image section only if photo exists
    const imageSection = hasImage ? `
        <div class="asset-detail-section text-center">
            <div class="card-img-wrapper position-relative d-inline-block mb-3">
                <img src="${asset.foto}" class="rounded-3 shadow-sm" 
                     style="width: 200px; height: 200px; object-fit: cover; border: 4px solid #fff;" 
                     alt="Asset Image">
            </div>
        </div>
    ` : '';
    
    // Create standard Codebase modal HTML
    const modalHtml = `
        <div class="modal fade" id="assetDetailModal" tabindex="-1" aria-labelledby="assetDetailModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-xl modal-dialog-centered modal-dialog-scrollable">
                <div class="modal-content">
                    <div class="modal-header py-2">
                        <h5 class="modal-title" id="assetDetailModalLabel">
                            <i class="fa fa-cube me-2"></i>${asset.namaBarang || 'Asset Detail'}
                        </h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        ${hasImage ? `
                        <div class="row mb-4">
                            <div class="col-md-4 text-center">
                                <img src="${asset.foto}" class="img-fluid rounded" 
                                     style="max-height: 200px; object-fit: cover;" alt="Asset Image">
                                <div class="mt-2">
                                    ${statusBadge}
                                </div>
                            </div>
                            <div class="col-md-8">
                                <h5 class="mb-2">${asset.namaBarang || '-'}</h5>
                                <p class="text-muted mb-2"><strong>Code:</strong> ${asset.kodeBarang || '-'} / ${asset.nomorAsset || '-'}</p>
                                <div class="mb-3">
                                    <span class="badge bg-secondary me-2">
                                        <i class="fa fa-tag me-1"></i>${asset.kategori || '-'}
                                    </span>
                                    <span class="badge bg-info">
                                        <i class="fa fa-boxes me-1"></i>Qty: ${asset.qty || 0}
                                    </span>
                                </div>
                            </div>
                        </div>
                        ` : `
                        <div class="text-center mb-4">
                            <div class="mb-3">
                                ${statusBadge}
                                <span class="badge bg-info ms-2">
                                    <i class="fa fa-boxes me-1"></i>Qty: ${asset.qty || 0}
                                </span>
                            </div>
                            <h5 class="mb-2">${asset.namaBarang || '-'}</h5>
                            <p class="text-muted mb-3"><strong>Code:</strong> ${asset.kodeBarang || '-'} / ${asset.nomorAsset || '-'}</p>
                            <span class="badge bg-secondary">
                                <i class="fa fa-tag me-1"></i>${asset.kategori || '-'}
                            </span>
                        </div>
                        `}
                        
                        <div class="row">
                            <div class="col-md-6">
                                <h6 class="text-primary"><i class="fa fa-info-circle me-1"></i>Asset Details</h6>
                                <table class="table table-sm table-borderless">
                                    <tr>
                                        <td class="text-muted">Asset Code</td>
                                        <td><strong>${asset.kodeBarang || '-'}</strong></td>
                                    </tr>
                                    <tr>
                                        <td class="text-muted">Asset Number</td>
                                        <td><strong>${asset.nomorAsset || '-'}</strong></td>
                                    </tr>
                                    <tr>
                                        <td class="text-muted">Category</td>
                                        <td><strong>${asset.kategori || '-'}</strong></td>
                                    </tr>
                                </table>
                            </div>
                            <div class="col-md-6">
                                <h6 class="text-primary"><i class="fa fa-map-marker-alt me-1"></i>Location & PO</h6>
                                <table class="table table-sm table-borderless">
                                    <tr>
                                        <td class="text-muted">PO Number</td>
                                        <td><strong>${asset.poNumber || '-'}</strong></td>
                                    </tr>
                                    <tr>
                                        <td class="text-muted">District</td>
                                        <td><strong>${asset.district || '-'}</strong></td>
                                    </tr>
                                    <tr>
                                        <td class="text-muted">Date Created</td>
                                        <td><strong>${asset.tanggalInput ? new Date(asset.tanggalInput).toLocaleDateString('id-ID') : '-'}</strong></td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                        
                        <div class="row mt-3">
                            <div class="col-md-4 mb-2">
                                <button type="button" class="btn btn-primary w-100" id="viewSerialNumbersBtn"
                                        data-kode-barang="${asset.kodeBarang}" data-nomor-asset="${asset.nomorAsset}">
                                    <i class="fa fa-barcode me-2"></i>View Serial Numbers
                                </button>
                            </div>
                            <div class="col-md-4 mb-2">
                                <button type="button" class="btn btn-info w-100" id="viewPODetailsBtn"
                                        data-po-number="${asset.poNumber || ''}" data-kode-barang="${asset.kodeBarang}" data-nomor-asset="${asset.nomorAsset}">
                                    <i class="fa fa-file-invoice me-2"></i>View PO Details
                                </button>
                            </div>
                            <div class="col-md-4 mb-2">
                                <button type="button" class="btn btn-warning w-100" id="debugDataBtn"
                                        data-kode-barang="${asset.kodeBarang}" data-nomor-asset="${asset.nomorAsset}">
                                    <i class="fa fa-bug me-2"></i>Debug Data
                                </button>
                            </div>
                        </div>
                        
                        <!-- Dynamic content area for lists -->
                        <div id="dynamicContentArea" class="mt-4" style="display: none;">
                            <hr>
                            <div class="d-flex justify-content-between align-items-center mb-3">
                                <h6 id="dynamicContentTitle" class="text-primary mb-0"></h6>
                                <button type="button" class="btn btn-sm btn-outline-secondary" id="hideDynamicContent">
                                    <i class="fa fa-times"></i> Hide
                                </button>
                            </div>
                            
                            <!-- Search and pagination controls -->
                            <div class="row mb-3">
                                <div class="col-md-6">
                                    <div class="input-group input-group-sm">
                                        <span class="input-group-text"><i class="fa fa-search"></i></span>
                                        <input type="text" id="dynamicSearchInput" class="form-control" placeholder="Search...">
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <select id="dynamicPageSize" class="form-select form-select-sm">
                                        <option value="5">5 per page</option>
                                        <option value="10" selected>10 per page</option>
                                        <option value="25">25 per page</option>
                                        <option value="50">50 per page</option>
                                    </select>
                                </div>
                                <div class="col-md-3 text-end">
                                    <button type="button" class="btn btn-sm btn-success" id="addNewItemBtn" style="display: none;">
                                        <i class="fa fa-plus"></i> Add New
                                    </button>
                                </div>
                            </div>
                            
                            <!-- Data table container -->
                            <div id="dynamicTableContainer" style="max-height: 400px; overflow-y: auto;">
                                <!-- Table will be dynamically populated -->
                            </div>
                            
                            <!-- Pagination -->
                            <div class="row mt-3">
                                <div class="col-md-6">
                                    <div id="dynamicPaginationInfo" class="text-muted"></div>
                                </div>
                                <div class="col-md-6">
                                    <nav>
                                        <ul id="dynamicPagination" class="pagination pagination-sm justify-content-end mb-0">
                                            <!-- Pagination will be dynamically populated -->
                                        </ul>
                                    </nav>
                                </div>
                            </div>
                        </div>
                    </div>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    // Remove existing modal if any
    $('#assetDetailModal').remove();
    
    // Add modal to body and show
    $('body').append(modalHtml);
    
    // Show modal with animation
    const modal = new bootstrap.Modal(document.getElementById('assetDetailModal'));
    modal.show();
    
    // Setup dynamic content handlers after modal is shown
    setupDynamicContentHandlers(asset);
    
    // Clean up when modal is hidden
    $('#assetDetailModal').on('hidden.bs.modal', function () {
        $(this).remove();
    });
}

// Setup dynamic content handlers
function setupDynamicContentHandlers(asset) {
    let currentContentType = null;
    let currentData = [];
    let filteredData = [];
    let currentPage = 1;
    let pageSize = 10;
    
    // View Serial Numbers button handler
    $('#viewSerialNumbersBtn').off('click').on('click', function() {
        const kodeBarang = $(this).data('kode-barang');
        const nomorAsset = $(this).data('nomor-asset');
        loadSerialNumbers(kodeBarang, nomorAsset);
    });
    
    // View PO Details button handler
    $('#viewPODetailsBtn').off('click').on('click', function() {
        const poNumber = $(this).data('po-number');
        const kodeBarang = $(this).data('kode-barang');
        const nomorAsset = $(this).data('nomor-asset');
        loadPODetails(poNumber, kodeBarang, nomorAsset);
    });
    
    // Debug button handler
    $('#debugDataBtn').off('click').on('click', function() {
        const kodeBarang = $(this).data('kode-barang');
        const nomorAsset = $(this).data('nomor-asset');
        debugAssetData(kodeBarang, nomorAsset);
    });
    
    // Hide dynamic content button
    $('#hideDynamicContent').off('click').on('click', function() {
        $('#dynamicContentArea').slideUp();
    });
    
    // Search input handler
    $('#dynamicSearchInput').off('input').on('input', function() {
        const searchTerm = $(this).val().toLowerCase();
        applySearch(searchTerm);
    });
    
    // Page size change handler
    $('#dynamicPageSize').off('change').on('change', function() {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        renderTable();
    });
    
    // Add new item handler
    $('#addNewItemBtn').off('click').on('click', function() {
        if (currentContentType === 'serial') {
            addNewSerial();
        } else if (currentContentType === 'po') {
            addNewPO();
        }
    });
    
    function loadSerialNumbers(kodeBarang, nomorAsset) {
        console.log('Loading serial numbers for:', kodeBarang, nomorAsset); // Debug log
        showLoading('Loading serial numbers...');
        
        $.ajax({
            url: '/Api/Review/GetAssetSerialNumbersPaginated',
            type: 'GET',
            data: {
                kodeBarang: kodeBarang,
                nomorAsset: nomorAsset,
                page: 1,
                pageSize: 100, // Get all for client-side pagination
                search: ''
            },
            success: function(response) {
                console.log('Serial numbers response:', response); // Debug log
                hideLoading();
                if (response.success && response.data) {
                    currentContentType = 'serial';
                    currentData = response.data.items || response.data || [];
                    filteredData = [...currentData];
                    currentPage = 1;
                    
                    console.log('Serial numbers data:', currentData); // Debug log
                    
                    $('#dynamicContentTitle').html('<i class="fa fa-barcode me-2"></i>Serial Numbers');
                    $('#addNewItemBtn').show();
                    $('#dynamicContentArea').slideDown();
                    renderTable();
                } else {
                    console.log('Failed response:', response); // Debug log
                    showError(response.message || 'Failed to load serial numbers');
                }
            },
            error: function() {
                hideLoading();
                showError('Failed to fetch serial numbers');
            }
        });
    }
    
    function loadPODetails(poNumber, kodeBarang, nomorAsset) {
        console.log('Loading PO details for:', poNumber, kodeBarang, nomorAsset); // Debug log
        showLoading('Loading PO details...');
        
        $.ajax({
            url: '/Api/Review/GetPODetailsPaginated',
            type: 'GET',
            data: {
                poNumber: poNumber,
                kodeBarang: kodeBarang,
                nomorAsset: nomorAsset,
                page: 1,
                pageSize: 100, // Get all for client-side pagination
                search: ''
            },
            success: function(response) {
                console.log('PO details response:', response); // Debug log
                hideLoading();
                if (response.success && response.data) {
                    currentContentType = 'po';
                    currentData = response.data.items || response.data || [];
                    filteredData = [...currentData];
                    currentPage = 1;
                    
                    console.log('PO details data:', currentData); // Debug log
                    
                    $('#dynamicContentTitle').html('<i class="fa fa-file-invoice me-2"></i>PO Details');
                    $('#addNewItemBtn').show();
                    $('#dynamicContentArea').slideDown();
                    renderTable();
                } else {
                    console.log('Failed PO response:', response); // Debug log
                    showError(response.message || 'Failed to load PO details');
                }
            },
            error: function() {
                hideLoading();
                showError('Failed to fetch PO details');
            }
        });
    }
    
    function applySearch(searchTerm) {
        if (!searchTerm) {
            filteredData = [...currentData];
        } else {
            if (currentContentType === 'serial') {
                filteredData = currentData.filter(item => 
                    (item.serialNumber || '').toLowerCase().includes(searchTerm) ||
                    (item.description || '').toLowerCase().includes(searchTerm)
                );
            } else if (currentContentType === 'po') {
                filteredData = currentData.filter(item => 
                    (item.poNumber || '').toLowerCase().includes(searchTerm) ||
                    (item.poItem || '').toLowerCase().includes(searchTerm)
                );
            }
        }
        currentPage = 1;
        renderTable();
    }
    
    function renderTable() {
        const totalItems = filteredData.length;
        const totalPages = Math.ceil(totalItems / pageSize);
        const startIndex = (currentPage - 1) * pageSize;
        const endIndex = Math.min(startIndex + pageSize, totalItems);
        const pageData = filteredData.slice(startIndex, endIndex);
        
        let tableHtml = '';
        
        if (currentContentType === 'serial') {
            tableHtml = renderSerialTable(pageData);
        } else if (currentContentType === 'po') {
            tableHtml = renderPOTable(pageData);
        }
        
        $('#dynamicTableContainer').html(tableHtml);
        
        // Update pagination info
        const paginationInfo = totalItems > 0 ? 
            `Showing ${startIndex + 1} to ${endIndex} of ${totalItems} entries` : 
            'No entries found';
        $('#dynamicPaginationInfo').text(paginationInfo);
        
        // Render pagination
        renderPagination(totalPages);
    }
    
    function renderSerialTable(data) {
        if (data.length === 0) {
            return `
                <div class="alert alert-info text-center">
                    <i class="fa fa-info-circle me-2"></i>No serial numbers found
                </div>
            `;
        }
        
        let tableHtml = `
            <table class="table table-striped table-sm">
                <thead class="table-dark">
                    <tr>
                        <th width="10%">#</th>
                        <th width="40%">Serial Number</th>
                        <th width="30%">Description</th>
                        <th width="15%">Status</th>
                        <th width="5%" class="text-center">Action</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        data.forEach((item, index) => {
            const actualIndex = (currentPage - 1) * pageSize + index + 1;
            const statusBadge = item.status === 'Active' ? 
                '<span class="badge bg-success">Active</span>' : 
                '<span class="badge bg-secondary">Inactive</span>';
                
            tableHtml += `
                <tr>
                    <td>${actualIndex}</td>
                    <td>
                        <input type="text" class="form-control form-control-sm serial-input" 
                               value="${item.serialNumber || ''}" 
                               data-id="${item.id || 'new'}"
                               placeholder="Enter serial number">
                    </td>
                    <td>
                        <input type="text" class="form-control form-control-sm description-input" 
                               value="${item.description || ''}" 
                               data-id="${item.id || 'new'}"
                               placeholder="Enter description">
                    </td>
                    <td>${statusBadge}</td>
                    <td class="text-center">
                        <button class="btn btn-outline-danger btn-sm delete-btn" 
                                data-id="${item.id}" 
                                title="Delete">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
        
        tableHtml += `
                </tbody>
            </table>
        `;
        
        return tableHtml;
    }
    
    function renderPOTable(data) {
        if (data.length === 0) {
            return `
                <div class="alert alert-info text-center">
                    <i class="fa fa-info-circle me-2"></i>No PO details found
                </div>
            `;
        }
        
        let tableHtml = `
            <table class="table table-striped table-sm">
                <thead class="table-dark">
                    <tr>
                        <th width="10%">#</th>
                        <th width="25%">PO Number</th>
                        <th width="40%">PO Item</th>
                        <th width="20%">Date</th>
                        <th width="5%" class="text-center">Action</th>
                    </tr>
                </thead>
                <tbody>
        `;
        
        data.forEach((item, index) => {
            const actualIndex = (currentPage - 1) * pageSize + index + 1;
            const formattedDate = item.poDate ? new Date(item.poDate).toLocaleDateString('id-ID') : '-';
                
            tableHtml += `
                <tr>
                    <td>${actualIndex}</td>
                    <td>
                        <input type="text" class="form-control form-control-sm po-number-input" 
                               value="${item.poNumber || ''}" 
                               data-id="${item.id || 'new'}"
                               placeholder="Enter PO number">
                    </td>
                    <td>
                        <input type="text" class="form-control form-control-sm po-item-input" 
                               value="${item.poItem || ''}" 
                               data-id="${item.id || 'new'}"
                               placeholder="Enter PO item">
                    </td>
                    <td>${formattedDate}</td>
                    <td class="text-center">
                        <button class="btn btn-outline-danger btn-sm delete-btn" 
                                data-id="${item.id}" 
                                title="Delete">
                            <i class="fa fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
        
        tableHtml += `
                </tbody>
            </table>
        `;
        
        return tableHtml;
    }
    
    function renderPagination(totalPages) {
        if (totalPages <= 1) {
            $('#dynamicPagination').empty();
            return;
        }
        
        let paginationHtml = '';
        
        // Previous button
        if (currentPage > 1) {
            paginationHtml += `
                <li class="page-item">
                    <a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>
                </li>
            `;
        }
        
        // Page numbers
        const startPage = Math.max(1, currentPage - 2);
        const endPage = Math.min(totalPages, currentPage + 2);
        
        if (startPage > 1) {
            paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
            if (startPage > 2) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }
        
        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === currentPage ? 'active' : '';
            paginationHtml += `
                <li class="page-item ${activeClass}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }
        
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
        }
        
        // Next button
        if (currentPage < totalPages) {
            paginationHtml += `
                <li class="page-item">
                    <a class="page-link" href="#" data-page="${currentPage + 1}">Next</a>
                </li>
            `;
        }
        
        $('#dynamicPagination').html(paginationHtml);
        
        // Setup pagination click handlers
        $('#dynamicPagination .page-link').off('click').on('click', function(e) {
            e.preventDefault();
            const page = parseInt($(this).data('page'));
            if (page && page !== currentPage) {
                currentPage = page;
                renderTable();
            }
        });
    }
    
    function addNewSerial() {
        const newSerial = {
            id: 'new_' + Date.now(),
            serialNumber: '',
            description: '',
            status: 'Active'
        };
        currentData.unshift(newSerial);
        filteredData = [...currentData];
        currentPage = 1;
        renderTable();
    }
    
    function addNewPO() {
        const newPO = {
            id: 'new_' + Date.now(),
            poNumber: '',
            poItem: '',
            poDate: new Date().toISOString()
        };
        currentData.unshift(newPO);
        filteredData = [...currentData];
        currentPage = 1;
        renderTable();
    }
    
    function showLoading(message) {
        $('#dynamicTableContainer').html(`
            <div class="text-center py-4">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <div class="mt-2">${message}</div>
            </div>
        `);
    }
    
    function hideLoading() {
        // Loading will be replaced by actual content
    }
    
    function showError(message) {
        Swal.fire({
            icon: 'error',
            title: 'Error',
            text: message
        });
    }
    
    function debugAssetData(kodeBarang, nomorAsset) {
        console.log('Debug asset data for:', kodeBarang, nomorAsset);
        
        $.ajax({
            url: '/Api/Review/DebugAssetData',
            type: 'GET',
            data: {
                kodeBarang: kodeBarang,
                nomorAsset: nomorAsset
            },
            success: function(response) {
                console.log('Debug response:', response);
                
                // Show debug info in a modal
                let debugHtml = `
                    <h6>Asset Information:</h6>
                    <pre>${JSON.stringify(response.Assets, null, 2)}</pre>
                    
                    <h6>Serial Numbers:</h6>
                    <pre>${JSON.stringify(response.Serials, null, 2)}</pre>
                    
                    <h6>PO Details:</h6>
                    <pre>${JSON.stringify(response.POs, null, 2)}</pre>
                    
                    <h6>Search Parameters:</h6>
                    <pre>${JSON.stringify(response.SearchParams, null, 2)}</pre>
                `;
                
                Swal.fire({
                    title: 'Debug Information',
                    html: debugHtml,
                    width: '80%',
                    showCloseButton: true,
                    showConfirmButton: false
                });
            },
            error: function(xhr, status, error) {
                console.error('Debug error:', error);
                showError('Failed to fetch debug data');
            }
        });
    }
}

// Function to view serial numbers (OLD VERSION - DEPRECATED)
// This function is replaced by the new dynamic content system
/*
function viewSerialNumbers(kodeBarang, nomorAsset) {
    $('#serialNumbersModal').remove();
    
    // Show loading
    Swal.fire({
        title: 'Loading...',
        text: 'Fetching serial numbers',
        allowOutsideClick: false,
        showConfirmButton: false,
        willOpen: () => {
            Swal.showLoading();
        }
    });
    
    // Fetch serial numbers from API
    $.ajax({
        url: '/Api/Review/GetAssetSerialNumbers',
        type: 'GET',
        data: {
            kodeBarang: kodeBarang,
            nomorAsset: nomorAsset
        },
        success: function(response) {
            Swal.close();
            
            if (response.success && response.data) {
                const serials = response.data;
                let serialsHtml = '';
                
                if (serials.length > 0) {
                    serials.forEach((serial, index) => {
                        serialsHtml += `
                            <tr>
                                <td class="text-center">${index + 1}</td>
                                <td>
                                    <input type="text" class="form-control form-control-sm" 
                                           value="${serial.serialNumber || ''}" 
                                           data-serial-id="${serial.id}"
                                           placeholder="Enter serial number">
                                </td>
                                <td class="text-center">
                                    <button class="btn btn-outline-danger btn-sm" 
                                            onclick="deleteSerial(${serial.id})" 
                                            title="Delete">
                                        <i class="fa fa-trash"></i>
                                    </button>
                                </td>
                            </tr>
                        `;
                    });
                } else {
                    serialsHtml = `
                        <tr>
                            <td colspan="3" class="text-center text-muted">
                                <i class="fa fa-info-circle me-1"></i>No serial numbers found
                            </td>
                        </tr>
                    `;
                }
                
                const modalHtml = `
                    <div class="modal fade" id="serialNumbersModal" tabindex="-1">
                        <div class="modal-dialog modal-lg modal-dialog-centered">
                            <div class="modal-content">
                                <div class="modal-header py-2">
                                    <h5 class="modal-title">
                                        <i class="fa fa-barcode me-2"></i>Serial Numbers
                                    </h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                </div>
                                <div class="modal-body">
                                    <div class="row mb-3">
                                        <div class="col-md-6">
                                            <strong>Asset Code:</strong> ${kodeBarang}
                                        </div>
                                        <div class="col-md-6">
                                            <strong>Asset Number:</strong> ${nomorAsset}
                                        </div>
                                    </div>
                                    ${response.data.length > 0 ? `
                                    <div class="table-responsive">
                                        <table class="table table-striped table-sm">
                                            <thead class="table-dark">
                                                <tr>
                                                    <th width="10%">#</th>
                                                    <th width="70%">Serial Number</th>
                                                    <th width="20%" class="text-center">Action</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                ${serialsHtml}
                                            </tbody>
                                        </table>
                                        <div class="mt-3">
                                            <button class="btn btn-sm btn-success" onclick="addNewSerial()">
                                                <i class="fa fa-plus me-1"></i>Add New Serial
                                            </button>
                                        </div>
                                    </div>
                                    ` : `
                                    <div class="alert alert-info text-center">
                                        <i class="fa fa-info-circle me-2"></i>No serial numbers found
                                        <br>
                                        <button class="btn btn-sm btn-success mt-2" onclick="addNewSerial()">
                                            <i class="fa fa-plus me-1"></i>Add New Serial
                                        </button>
                                    </div>
                                    `}
                                </div>
                                <div class="modal-footer py-2">
                                    <button type="button" class="btn btn-sm btn-primary" onclick="saveSerialNumbers()">
                                        <i class="fa fa-save me-1"></i>Save Changes
                                    </button>
                                    <button type="button" class="btn btn-sm btn-secondary" data-bs-dismiss="modal">Close</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                $('body').append(modalHtml);
                const modal = new bootstrap.Modal(document.getElementById('serialNumbersModal'));
                modal.show();
                
                $('#serialNumbersModal').on('hidden.bs.modal', function () {
                    $(this).remove();
                });
                
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Failed to load serial numbers'
                });
            }
        },
        error: function() {
            Swal.close();
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to fetch serial numbers'
            });
        }
    });
}
*/

// Function to view PO details (OLD VERSION - DEPRECATED)
// This function is replaced by the new dynamic content system
/*
function viewPODetails(poNumber) {
    if (!poNumber || poNumber === '') {
        Swal.fire({
            icon: 'info',
            title: 'No PO Information',
            text: 'This asset does not have PO information.'
        });
        return;
    }
    
    $('#poDetailsModal').remove();
    
    // Show loading
    Swal.fire({
        title: 'Loading...',
        text: 'Fetching PO details',
        allowOutsideClick: false,
        showConfirmButton: false,
        willOpen: () => {
            Swal.showLoading();
        }
    });
    
    // Fetch PO details from API
    $.ajax({
        url: '/Api/Review/GetPODetails',
        type: 'GET',
        data: {
            poNumber: poNumber
        },
        success: function(response) {
            Swal.close();
            
            if (response.success && response.data) {
                const po = response.data;
                
                const modalHtml = `
                    <div class="modal fade" id="poDetailsModal" tabindex="-1">
                        <div class="modal-dialog modal-lg modal-dialog-centered">
                            <div class="modal-content">
                                <div class="modal-header py-2">
                                    <h5 class="modal-title">
                                        <i class="fa fa-file-invoice me-2"></i>PO Details
                                    </h5>
                                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                                </div>
                                <div class="modal-body">
                                    <div class="row">
                                        <div class="col-md-12">
                                            <table class="table table-sm table-borderless">
                                                <tr>
                                                    <td class="text-muted" width="30%">PO Number</td>
                                                    <td>
                                                        <input type="text" class="form-control form-control-sm" 
                                                               value="${po.poNumber || ''}" 
                                                               data-field="poNumber"
                                                               placeholder="Enter PO Number">
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="text-muted">PO Item</td>
                                                    <td>
                                                        <textarea class="form-control form-control-sm" 
                                                                  rows="2"
                                                                  data-field="poItem"
                                                                  placeholder="Enter PO Item Description">${po.poItem || ''}</textarea>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </div>
                                </div>
                                <div class="modal-footer py-2">
                                    <button type="button" class="btn btn-sm btn-primary" onclick="savePODetails()">
                                        <i class="fa fa-save me-1"></i>Save Changes
                                    </button>
                                    <button type="button" class="btn btn-sm btn-secondary" data-bs-dismiss="modal">Close</button>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
                
                $('body').append(modalHtml);
                const modal = new bootstrap.Modal(document.getElementById('poDetailsModal'));
                modal.show();
                
                $('#poDetailsModal').on('hidden.bs.modal', function () {
                    $(this).remove();
                });
                
            } else {
                Swal.fire({
                    icon: 'error',
                    title: 'Error',
                    text: response.message || 'Failed to load PO details'
                });
            }
        },
        error: function() {
            Swal.close();
            Swal.fire({
                icon: 'error',
                title: 'Error',
                text: 'Failed to fetch PO details'
            });
        }
    });
}
*/

// Function to add new serial number row (OLD VERSION - DEPRECATED)
// This function is replaced by the new dynamic content system
/*
function addNewSerial() {
    const tbody = $('#serialNumbersModal table tbody');
    const newIndex = tbody.find('tr').length + 1;
    
    const newRow = `
        <tr>
            <td class="text-center">${newIndex}</td>
            <td>
                <input type="text" class="form-control form-control-sm" 
                       value="" 
                       data-serial-id="new"
                       placeholder="Enter serial number">
            </td>
            <td class="text-center">
                <button class="btn btn-outline-danger btn-sm" 
                        onclick="$(this).closest('tr').remove(); updateRowNumbers();" 
                        title="Delete">
                    <i class="fa fa-trash"></i>
                </button>
            </td>
        </tr>
    `;
    
    if (tbody.find('td[colspan="3"]').length > 0) {
        // Replace "no data" row
        tbody.html(newRow);
    } else {
        tbody.append(newRow);
    }
}

// Function to delete serial number
function deleteSerial(serialId) {
    if (confirm('Are you sure you want to delete this serial number?')) {
        // Remove the row
        $(`input[data-serial-id="${serialId}"]`).closest('tr').remove();
        updateRowNumbers();
    }
}

// Function to update row numbers
function updateRowNumbers() {
    $('#serialNumbersModal table tbody tr').each(function(index) {
        $(this).find('td:first').text(index + 1);
    });
}

// Function to save serial numbers
function saveSerialNumbers() {
    const serialInputs = $('#serialNumbersModal input[data-serial-id]');
    const serials = [];
    
    serialInputs.each(function() {
        const input = $(this);
        const serialNumber = input.val().trim();
        const serialId = input.data('serial-id');
        
        if (serialNumber) {
            serials.push({
                id: serialId === 'new' ? null : serialId,
                serialNumber: serialNumber
            });
        }
    });
    
    if (serials.length === 0) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning',
            text: 'Please enter at least one serial number'
        });
        return;
    }
    
    // Here you would typically send the data to the server
    console.log('Saving serials:', serials);
    
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: 'Serial numbers saved successfully',
        timer: 1500,
        showConfirmButton: false
    }).then(() => {
        $('#serialNumbersModal').modal('hide');
    });
}

// Function to save PO details
function savePODetails() {
    const poNumber = $('#poDetailsModal input[data-field="poNumber"]').val().trim();
    const poItem = $('#poDetailsModal textarea[data-field="poItem"]').val().trim();
    
    if (!poNumber) {
        Swal.fire({
            icon: 'warning',
            title: 'Warning',
            text: 'Please enter PO Number'
        });
        return;
    }
    
    const poData = {
        poNumber: poNumber,
        poItem: poItem
    };
    
    // Here you would typically send the data to the server
    console.log('Saving PO details:', poData);
    
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: 'PO details saved successfully',
        timer: 1500,
        showConfirmButton: false
    }).then(() => {
        $('#poDetailsModal').modal('hide');
    });
}
*/
