'use strict';

(function () {
    const languageMap = {
        tr: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/tr.json',
        en: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/en-GB.json',
        ar: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/ar.json'
    };

    const initializeDataTable = () => {
        if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.dataTable) {
            return null;
        }

        const tableEl = document.getElementById('suppliersTable');
        if (!tableEl) {
            return null;
        }

        const $table = window.jQuery(tableEl);
        if (window.jQuery.fn.dataTable.isDataTable(tableEl)) {
            return $table.DataTable();
        }

        const htmlElement = document.documentElement;
        const culture = (htmlElement && htmlElement.lang ? htmlElement.lang.toLowerCase() : 'en');
        const languageUrl = languageMap[culture] ?? languageMap.en;
        const responsiveFeatureAvailable = typeof window.jQuery.fn.dataTable.Responsive !== 'undefined';
        const orderColumnAttr = $table.data('order-column');
        const orderDirectionAttr = ($table.data('order-direction') || 'asc').toString().toLowerCase();

        const options = {
            paging: true,
            searching: true,
            ordering: true,
            responsive: responsiveFeatureAvailable,
            info: true,
            lengthChange: false,
            pageLength: 10,
            dom: 'rtip',
            order: [],
            language: {
                url: languageUrl
            }
        };

        if (typeof orderColumnAttr !== 'undefined') {
            const index = parseInt(orderColumnAttr, 10);
            if (!Number.isNaN(index)) {
                const dir = orderDirectionAttr === 'desc' ? 'desc' : 'asc';
                options.order = [[index, dir]];
            }
        }

        return $table.DataTable(options);
    };

    const bindSearchControls = (table) => {
        if (!table) {
            return;
        }

        const searchInput = document.querySelector('[data-suppliers-search]');
        const clearButton = document.querySelector('[data-suppliers-search-clear]');

        if (searchInput) {
            searchInput.addEventListener('input', () => {
                table.search(searchInput.value).draw();
            });
        }

        if (clearButton && searchInput) {
            clearButton.addEventListener('click', () => {
                searchInput.value = '';
                table.search('').draw();
            });
        }
    };

    const initializeCountryChart = () => {
        const chartConfig = window.suppliersDashboard && window.suppliersDashboard.chart;
        if (!chartConfig) {
            return;
        }

        const canvas = document.querySelector('[data-suppliers-chart]');
        if (!canvas || typeof window.Chart === 'undefined') {
            return;
        }

        const labels = Array.isArray(chartConfig.labels) ? chartConfig.labels : [];
        const data = Array.isArray(chartConfig.data) ? chartConfig.data : [];

        if (!labels.length || !data.length) {
            const container = canvas.parentElement;
            if (container) {
                container.innerHTML = `<div class="chart-empty">${chartConfig.noDataMessage || ''}</div>`;
            }
            return;
        }

        new window.Chart(canvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: chartConfig.datasetLabel || '',
                        data: data,
                        backgroundColor: '#1f7a8c',
                        hoverBackgroundColor: '#195f6a',
                        borderRadius: 10,
                        maxBarThickness: 48
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: '#93a5be'
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            precision: 0,
                            color: '#93a5be'
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    }
                }
            }
        });
    };

    const onReady = () => {
        const table = initializeDataTable();
        bindSearchControls(table);
        initializeCountryChart();
    };

    document.addEventListener('DOMContentLoaded', onReady);
})();

