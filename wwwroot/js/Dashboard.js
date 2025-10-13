// Dashboard JavaScript for Asset Management
console.log('DashboardNew.js loaded');

var categoryChart = null;
var monthlyTrendChart = null;
var statusChart = null;

$(document).ready(function () {
    console.log('Dashboard ready');
    loadDashboardData();
    loadTopAssetIn();
    loadTopAssetOut();
    loadOldestAssets();
    initializeCategoryChart();
    initializeMonthlyTrendChart();
    initializeStatusChart();
});

function loadDashboardData() {
    $.ajax({
        url: '/api/Dashboard/GetDashboardStats',
        type: 'GET',
        success: function (data) {
            console.log('Dashboard stats loaded:', data);
            $('#statTotalAssets').text(data.totalAssets || 0);
            $('#statAssetIn').text(data.totalAssetIn || 0);
            $('#statAssetOut').text(data.totalAssetOut || 0);
            $('#statCategories').text(data.totalCategories || 0);
            $('#monthlyAssetIn').text(data.monthlyAssetIn || 0);
            $('#monthlyAssetOut').text(data.monthlyAssetOut || 0);
            
            var total = (data.monthlyAssetIn || 0) + (data.monthlyAssetOut || 0);
            if (total > 0) {
                var inPercent = ((data.monthlyAssetIn || 0) / total) * 100;
                var outPercent = ((data.monthlyAssetOut || 0) / total) * 100;
                $('#monthlyInProgress').css('width', inPercent + '%');
                $('#monthlyOutProgress').css('width', outPercent + '%');
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading dashboard stats:', error);
        }
    });
}

function loadTopAssetIn() {
    $.ajax({
        url: '/api/Dashboard/GetTopAssetIn',
        type: 'GET',
        success: function (data) {
            console.log('Top Asset In loaded:', data);
            var html = '';
            if (data && data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    var asset = data[i];
                    var badgeClass = i === 0 ? 'bg-success' : i === 1 ? 'bg-warning' : 'bg-info';
                    var position = i + 1;
                    var borderClass = i < data.length - 1 ? 'border-bottom' : '';
                    
                    html += '<div class="d-flex align-items-center py-2 ' + borderClass + '">';
                    html += '<div class="flex-shrink-0 me-3">';
                    html += '<span class="badge ' + badgeClass + ' rounded-circle p-2">' + position + '</span>';
                    html += '</div>';
                    html += '<div class="flex-grow-1">';
                    html += '<div class="fw-semibold">' + (asset.namaBarang || 'N/A') + '</div>';
                    html += '<div class="fs-sm text-muted">' + (asset.kodeBarang || 'N/A') + ' - ' + (asset.nomorAsset || 'N/A') + '</div>';
                    html += '<div class="fs-sm text-muted">' + (asset.kategoriBarang || 'N/A') + '</div>';
                    html += '</div>';
                    html += '<div class="flex-shrink-0">';
                    html += '<span class="badge bg-success">' + (asset.qty || 0) + ' pcs</span>';
                    html += '</div>';
                    html += '</div>';
                }
            } else {
                html = '<div class="text-center py-4 text-muted">No data available</div>';
            }
            $('#topAssetInList').html(html);
        },
        error: function (xhr, status, error) {
            console.error('Error loading top asset in:', error);
            $('#topAssetInList').html('<div class="text-center py-4 text-danger">Error loading data</div>');
        }
    });
}

function loadTopAssetOut() {
    $.ajax({
        url: '/api/Dashboard/GetTopAssetOut',
        type: 'GET',
        success: function (data) {
            console.log('Top Asset Out loaded:', data);
            var html = '';
            if (data && data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    var asset = data[i];
                    var badgeClass = i === 0 ? 'bg-success' : i === 1 ? 'bg-warning' : 'bg-info';
                    var position = i + 1;
                    var borderClass = i < data.length - 1 ? 'border-bottom' : '';
                    
                    html += '<div class="d-flex align-items-center py-2 ' + borderClass + '">';
                    html += '<div class="flex-shrink-0 me-3">';
                    html += '<span class="badge ' + badgeClass + ' rounded-circle p-2">' + position + '</span>';
                    html += '</div>';
                    html += '<div class="flex-grow-1">';
                    html += '<div class="fw-semibold">' + (asset.namaBarang || 'N/A') + '</div>';
                    html += '<div class="fs-sm text-muted">' + (asset.kodeBarang || 'N/A') + ' - ' + (asset.nomorAsset || 'N/A') + '</div>';
                    html += '<div class="fs-sm text-muted">' + (asset.kategoriBarang || 'N/A') + '</div>';
                    html += '</div>';
                    html += '<div class="flex-shrink-0">';
                    html += '<span class="badge bg-warning">' + (asset.qty || 0) + ' pcs</span>';
                    html += '</div>';
                    html += '</div>';
                }
            } else {
                html = '<div class="text-center py-4 text-muted">No data available</div>';
            }
            $('#topAssetOutList').html(html);
        },
        error: function (xhr, status, error) {
            console.error('Error loading top asset out:', error);
            $('#topAssetOutList').html('<div class="text-center py-4 text-danger">Error loading data</div>');
        }
    });
}

function loadOldestAssets() {
    $.ajax({
        url: '/api/Dashboard/GetOldestAssets',
        type: 'GET',
        success: function (data) {
            console.log('Oldest Assets loaded:', data);
            var html = '';
            if (data && data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    var asset = data[i];
                    var badgeClass = i === 0 ? 'bg-danger' : i === 1 ? 'bg-warning' : 'bg-info';
                    var position = i + 1;
                    var borderClass = i < data.length - 1 ? 'border-bottom' : '';
                    
                    html += '<div class="d-flex align-items-center py-2 ' + borderClass + '">';
                    html += '<div class="flex-shrink-0 me-3">';
                    html += '<span class="badge ' + badgeClass + ' rounded-circle p-2">' + position + '</span>';
                    html += '</div>';
                    html += '<div class="flex-grow-1">';
                    html += '<div class="fw-semibold">' + (asset.namaBarang || 'N/A') + '</div>';
                    html += '<div class="fs-sm text-muted">' + (asset.kodeBarang || 'N/A') + ' - ' + (asset.nomorAsset || 'N/A') + '</div>';
                    if (asset.tanggalMasuk) {
                        html += '<div class="fs-sm text-muted">Stored: ' + moment(asset.tanggalMasuk).format('DD/MM/YYYY') + '</div>';
                    } else {
                        html += '<div class="fs-sm text-muted">Stored: N/A</div>';
                    }
                    html += '</div>';
                    html += '<div class="flex-shrink-0">';
                    html += '<span class="badge bg-info">' + (asset.daysStored || 0) + ' days</span>';
                    html += '</div>';
                    html += '</div>';
                }
            } else {
                html = '<div class="text-center py-4 text-muted">No data available</div>';
            }
            $('#oldestAssetsList').html(html);
        },
        error: function (xhr, status, error) {
            console.error('Error loading oldest assets:', error);
            $('#oldestAssetsList').html('<div class="text-center py-4 text-danger">Error loading data</div>');
        }
    });
}

function initializeCategoryChart() {
    var ctx = document.getElementById('categoryChart');
    if (!ctx) {
        console.error('Category chart canvas not found');
        return;
    }
    
    categoryChart = new Chart(ctx.getContext('2d'), {
        type: 'doughnut',
        data: {
            labels: [],
            datasets: [{
                data: [],
                backgroundColor: [
                    '#3b82f6', '#ef4444', '#10b981', '#f59e0b', '#8b5cf6',
                    '#06b6d4', '#ec4899', '#84cc16', '#f97316', '#6366f1'
                ],
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        padding: 20,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            return context.label + ': ' + context.raw + ' assets';
                        }
                    }
                }
            }
        }
    });
    
    updateCategoryChart(null);
}

function updateCategoryChart(status) {
    var requestData = {};
    if (status !== null) {
        requestData.status = status;
    }
    
    $.ajax({
        url: '/api/Dashboard/GetAssetsByCategory',
        type: 'GET',
        data: requestData,
        success: function (data) {
            console.log('Category chart data loaded:', data);
            if (categoryChart && data) {
                var labels = [];
                var counts = [];
                
                for (var i = 0; i < data.length; i++) {
                    labels.push(data[i].category);
                    counts.push(data[i].count);
                }
                
                categoryChart.data.labels = labels;
                categoryChart.data.datasets[0].data = counts;
                categoryChart.update();
            }
        },
        error: function (xhr, status, error) {
            console.error('Error loading category chart data:', error);
        }
    });
}

function initializeMonthlyTrendChart() {
    var ctx = document.getElementById('monthlyTrendChart');
    if (!ctx) {
        console.error('Monthly trend chart canvas not found');
        return;
    }
    
    $.ajax({
        url: '/api/Dashboard/GetMonthlyTrend',
        type: 'GET',
        success: function (data) {
            console.log('Monthly trend data loaded:', data);
            
            var labels = [];
            var assetInData = [];
            var assetOutData = [];
            
            for (var i = 0; i < data.length; i++) {
                labels.push(data[i].monthName);
                assetInData.push(data[i].assetIn);
                assetOutData.push(data[i].assetOut);
            }
            
            monthlyTrendChart = new Chart(ctx.getContext('2d'), {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'Asset In',
                        data: assetInData,
                        borderColor: '#10b981',
                        backgroundColor: 'rgba(16, 185, 129, 0.1)',
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4
                    }, {
                        label: 'Asset Out',
                        data: assetOutData,
                        borderColor: '#f59e0b',
                        backgroundColor: 'rgba(245, 158, 11, 0.1)',
                        borderWidth: 3,
                        fill: true,
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    },
                    interaction: {
                        intersect: false,
                        mode: 'index'
                    }
                }
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading monthly trend data:', error);
        }
    });
}

function initializeStatusChart() {
    var ctx = document.getElementById('statusChart');
    if (!ctx) {
        console.error('Status chart canvas not found');
        return;
    }
    
    $.ajax({
        url: '/api/Dashboard/GetDashboardStats',
        type: 'GET',
        success: function (data) {
            console.log('Status chart data loaded:', data);
            
            statusChart = new Chart(ctx.getContext('2d'), {
                type: 'bar',
                data: {
                    labels: ['Asset In', 'Asset Out'],
                    datasets: [{
                        label: 'Count',
                        data: [data.totalAssetIn || 0, data.totalAssetOut || 0],
                        backgroundColor: ['#10b981', '#f59e0b'],
                        borderColor: ['#059669', '#d97706'],
                        borderWidth: 2,
                        borderRadius: 8,
                        borderSkipped: false
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: false
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                stepSize: 1
                            }
                        }
                    }
                }
            });
        },
        error: function (xhr, status, error) {
            console.error('Error loading status chart data:', error);
        }
    });
}