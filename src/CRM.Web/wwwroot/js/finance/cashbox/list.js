'use strict';

(function () {
    const languageMap = {
        tr: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/tr.json',
        en: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/en-GB.json',
        ar: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/ar.json'
    };

    const initialiseDataTable = () => {
        if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.dataTable) {
            return null;
        }

        const tableEl = document.getElementById('cashboxTable');
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
        const orderDirectionAttr = ($table.data('order-direction') || 'desc').toString().toLowerCase();

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
                const dir = orderDirectionAttr === 'asc' ? 'asc' : 'desc';
                options.order = [[idx, dir]];
            }
        }

        return $table.DataTable(options);
    };

    const bindSearchControls = (table) => {
        if (!table) {
            return;
        }

        const searchInput = document.querySelector('[data-cashbox-search]');
        const clearButton = document.querySelector('[data-cashbox-search-clear]');

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

    const initialiseChart = () => {
        const config = window.cashboxDashboard && window.cashboxDashboard.chart;
        if (!config) {
            return;
        }

        const canvas = document.querySelector('[data-cashbox-chart]');
        if (!canvas || typeof window.Chart === 'undefined') {
            return;
        }

        const labels = Array.isArray(config.labels) ? config.labels : [];
        const income = Array.isArray(config.income) ? config.income : [];
        const expense = Array.isArray(config.expense) ? config.expense : [];
        const net = Array.isArray(config.net) ? config.net : [];

        const dataAvailable =
            labels.length &&
            (income.length || expense.length || net.length) &&
            (income.some((value) => Number(value) !== 0) ||
                expense.some((value) => Number(value) !== 0) ||
                net.some((value) => Number(value) !== 0));

        if (!dataAvailable) {
            const container = canvas.parentElement;
            if (container) {
                container.innerHTML = `<div class="chart-empty">${config.noDataMessage || ''}</div>`;
            }
            return;
        }

        const locale = document.documentElement.lang || 'en';
        const numberFormatter = new Intl.NumberFormat(locale, {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });

        new window.Chart(canvas, {
            data: {
                labels: labels,
                datasets: [
                    {
                        type: 'bar',
                        label: 'Income',
                        data: income,
                        backgroundColor: 'rgba(34, 197, 94, 0.7)',
                        borderRadius: 8,
                        maxBarThickness: 48
                    },
                    {
                        type: 'bar',
                        label: 'Expense',
                        data: expense,
                        backgroundColor: 'rgba(239, 68, 68, 0.62)',
                        borderRadius: 8,
                        maxBarThickness: 48
                    },
                    {
                        type: 'line',
                        label: 'Net',
                        data: net,
                        borderColor: '#0ea5e9',
                        backgroundColor: 'rgba(14, 165, 233, 0.2)',
                        tension: 0.35,
                        borderWidth: 2,
                        pointRadius: 4,
                        pointBackgroundColor: '#0ea5e9',
                        fill: false,
                        yAxisID: 'y'
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    mode: 'index',
                    intersect: false
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        }
                    },
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(148, 163, 184, 0.2)'
                        },
                        ticks: {
                            callback: function (value) {
                                return numberFormatter.format(value);
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                const datasetLabel = context.dataset.label;
                                const value = context.parsed.y ?? context.parsed;
                                return `${datasetLabel}: ${numberFormatter.format(value)}`;
                            }
                        }
                    }
                }
            }
        });
    };

    const onReady = () => {
        const table = initialiseDataTable();
        bindSearchControls(table);
        initialiseChart();
    };

    document.addEventListener('DOMContentLoaded', onReady);
})();

