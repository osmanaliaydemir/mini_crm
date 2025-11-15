'use strict';

(function () {
    if (typeof window.Chart === 'undefined') {
        return;
    }

    const data = window.operationsAnalyticsData || {};

    const statusCtx = document.getElementById('opsStatusChart');
    if (statusCtx && Array.isArray(data.status) && data.status.length) {
        const labels = data.status.map((item) => item.label);
        const values = data.status.map((item) => item.value);
        new window.Chart(statusCtx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    label: 'Shipments',
                    data: values,
                    backgroundColor: [
                        'rgba(37, 99, 235, 0.75)',
                        'rgba(14, 165, 233, 0.72)',
                        'rgba(16, 185, 129, 0.72)',
                        'rgba(245, 158, 11, 0.75)',
                        'rgba(239, 68, 68, 0.75)'
                    ],
                    borderRadius: 12,
                    maxBarThickness: 42
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#a0aec0' }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { color: '#a0aec0' },
                        grid: { color: 'rgba(148, 163, 184, 0.12)' }
                    }
                },
                plugins: {
                    legend: { display: false }
                }
            }
        });
    }

    const trendCtx = document.getElementById('opsTrendChart');
    if (trendCtx && data.trend && Array.isArray(data.trend.labels)) {
        new window.Chart(trendCtx, {
            type: 'line',
            data: {
                labels: data.trend.labels,
                datasets: [{
                    label: 'Completed',
                    data: data.trend.values || [],
                    borderColor: 'rgba(14, 165, 233, 1)',
                    backgroundColor: 'rgba(14, 165, 233, 0.18)',
                    fill: true,
                    tension: 0.38,
                    pointRadius: 4,
                    pointBackgroundColor: 'rgba(14, 165, 233, 1)'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        ticks: { color: '#a0aec0' },
                        grid: { color: 'transparent' }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { color: '#a0aec0' },
                        grid: { color: 'rgba(148, 163, 184, 0.12)' }
                    }
                },
                plugins: {
                    legend: { display: false }
                }
            }
        });
    }

    const throughputCtx = document.getElementById('opsThroughputChart');
    if (throughputCtx && Array.isArray(data.throughput) && data.throughput.length) {
        const labels = data.throughput.map((item) => item.label);
        const unloadings = data.throughput.map((item) => item.unloadings);
        new window.Chart(throughputCtx, {
            type: 'bar',
            data: {
                labels,
                datasets: [{
                    label: 'Unloadings',
                    data: unloadings,
                    backgroundColor: 'rgba(16, 185, 129, 0.75)',
                    borderRadius: 12,
                    maxBarThickness: 36
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: { color: '#a0aec0' },
                        grid: { color: 'rgba(148, 163, 184, 0.12)' }
                    },
                    y: {
                        ticks: { color: '#a0aec0' },
                        grid: { color: 'transparent' }
                    }
                },
                plugins: {
                    legend: { display: false }
                }
            }
        });
    }
})();

