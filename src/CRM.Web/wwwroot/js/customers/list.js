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

        const tableEl = document.getElementById('customersTable');
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
            const idx = parseInt(orderColumnAttr, 10);
            if (!Number.isNaN(idx)) {
                const dir = orderDirectionAttr === 'desc' ? 'desc' : 'asc';
                options.order = [[idx, dir]];
            }
        }

        return $table.DataTable(options);
    };

    const bindSearchControls = (table) => {
        if (!table) {
            return;
        }

        const searchInput = document.querySelector('[data-customers-search]');
        const clearButton = document.querySelector('[data-customers-search-clear]');

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

    const initializeInteractionsChart = () => {
        const config = window.customersDashboard && window.customersDashboard.chart;
        if (!config) {
            return;
        }

        const canvas = document.querySelector('[data-customers-chart]');
        if (!canvas || typeof window.Chart === 'undefined') {
            return;
        }

        const labels = Array.isArray(config.labels) ? config.labels : [];
        const data = Array.isArray(config.data) ? config.data : [];

        if (!labels.length || !data.length || data.every((value) => Number(value) === 0)) {
            const container = canvas.parentElement;
            if (container) {
                container.innerHTML = `<div class="chart-empty">${config.noDataMessage || ''}</div>`;
            }
            return;
        }

        const locale = document.documentElement.lang || 'en';
        const countFormatter = new Intl.NumberFormat(locale, { minimumFractionDigits: 0, maximumFractionDigits: 0 });

        new window.Chart(canvas, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        data: data,
                        fill: true,
                        tension: 0.35,
                        borderColor: '#ec4899',
                        backgroundColor: 'rgba(236, 72, 153, 0.18)',
                        pointBackgroundColor: '#ec4899',
                        pointBorderColor: '#fff',
                        pointBorderWidth: 2,
                        pointRadius: 4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#93a5be' }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            color: '#93a5be',
                            callback: function (value) {
                                return countFormatter.format(value);
                            }
                        }
                    }
                },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const formatted = countFormatter.format(context.parsed.y || 0);
                                return `${formatted} ${config.pointLabel || ''}`.trim();
                            }
                        }
                    }
                }
            }
        });
    };

    const onReady = () => {
        const table = initializeDataTable();
        bindSearchControls(table);
        initializeInteractionsChart();
    };

    document.addEventListener('DOMContentLoaded', onReady);
})();

