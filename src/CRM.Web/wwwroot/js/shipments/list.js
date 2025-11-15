'use strict';

(function () {
    const initializeDataTable = (table) => {
        const $table = window.jQuery(table);
        if (!$table.length) {
            return null;
        }

        if (window.jQuery.fn.dataTable.isDataTable(table)) {
            return $table.DataTable();
        }

        const languageMap = {
            tr: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/tr.json',
            en: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/en-GB.json',
            ar: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/ar.json'
        };

        const htmlElement = document.documentElement;
        const culture = (htmlElement && htmlElement.lang ? htmlElement.lang.toLowerCase() : 'en');
        const languageUrl = languageMap[culture] ?? languageMap.en;
        const disableLastColumn = $table.data('disable-last-column') === true || $table.data('disable-last-column') === 'true';
        const responsiveRequested = $table.data('responsive');
        const responsiveFeatureAvailable = typeof window.jQuery.fn.dataTable.Responsive !== 'undefined';
        const responsive =
            responsiveFeatureAvailable &&
            (typeof responsiveRequested === 'undefined' || responsiveRequested === true || responsiveRequested === 'true');
        const defaultOrderColumn = $table.data('order-column');
        const defaultOrderDirection = ($table.data('order-direction') || 'asc').toString().toLowerCase();

        const options = {
            paging: true,
            searching: true,
            ordering: true,
            responsive: responsive,
            info: true,
            lengthChange: false,
            pageLength: 10,
            dom: 'rtip',
            language: {
                url: languageUrl
            },
            order: []
        };

        if (typeof defaultOrderColumn !== 'undefined') {
            const columnIndex = parseInt(defaultOrderColumn, 10);
            if (!Number.isNaN(columnIndex)) {
                const dir = defaultOrderDirection === 'desc' ? 'desc' : 'asc';
                options.order = [[columnIndex, dir]];
            }
        }

        if (disableLastColumn) {
            options.columnDefs = [
                {
                    targets: -1,
                    orderable: false,
                    searchable: false
                }
            ];
        }

        return $table.DataTable(options);
    };

    const getStatusColumnIndex = (tableEl) => {
        const attrValue = tableEl.getAttribute('data-status-column');
        if (!attrValue) return 3;
        const parsed = parseInt(attrValue, 10);
        return Number.isNaN(parsed) ? 3 : parsed;
    };

    const escapeRegex = (value) => value.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');

    const formatDate = (date) => {
        if (!date) return '';
        try {
            return new Intl.DateTimeFormat(document.documentElement.lang || 'tr', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            }).format(date);
        } catch (err) {
            return date.toLocaleString();
        }
    };

    const updateKpiValues = (table) => {
        const totalEl = document.querySelector('[data-kpi=\"total\"] .kpi-value');
        const activeEl = document.querySelector('[data-kpi=\"active\"] .kpi-value');
        const deliveredEl = document.querySelector('[data-kpi=\"delivered\"] .kpi-value');
        const customsEl = document.querySelector('[data-kpi=\"customs\"] .kpi-value');

        if (!totalEl || !activeEl || !deliveredEl || !customsEl) return;

        const rows = table.rows({ filter: 'applied' }).data();
        let total = rows.length;
        let active = 0;
        let delivered = 0;
        let customs = 0;

        rows.each((row) => {
            const statusText = window.jQuery(row[3]).text().trim();
            const statusNormalized = statusText.toLowerCase();

            if (!statusNormalized || total === 0) return;

            if (statusNormalized.includes('teslim')) {
                delivered += 1;
            }

            if (statusNormalized.includes('gümrük')) {
                customs += 1;
            }

            if (!statusNormalized.includes('iptal') && !statusNormalized.includes('teslim')) {
                active += 1;
            }
        });

        totalEl.textContent = total;
        activeEl.textContent = active;
        deliveredEl.textContent = delivered;
        customsEl.textContent = customs;
    };

    const normalizeText = (text) => (text || '').toString().trim().toLowerCase();

    const updateLastUpdatedInfo = (table) => {
        const infoContainer = document.querySelector('[data-shipments-toolbar]');
        if (!infoContainer) return;

        const rows = table.rows({ filter: 'applied' }).data();
        let latestDate = null;

        rows.each((row) => {
            const cell = window.jQuery('<div/>').html(row[4]).text().trim();
            if (!cell) return;

            const parsedDate = new Date(cell);
            if (!Number.isNaN(parsedDate.getTime())) {
                if (!latestDate || parsedDate > latestDate) {
                    latestDate = parsedDate;
                }
            }
        });

        if (latestDate) {
            infoContainer.setAttribute('data-last-updated', formatDate(latestDate));
        } else {
            infoContainer.removeAttribute('data-last-updated');
        }
    };

    const registerFilters = (table, tableEl) => {
        const statusColumnIndex = getStatusColumnIndex(tableEl);
        const chips = document.querySelectorAll('[data-status-filter]');
        const searchInput = document.querySelector('[data-table-search]');
        const clearButton = document.querySelector('[data-clear-search]');
        const totalCountEl = document.querySelector('[data-total-count]');

        const applyStatusFilter = (filterValue, filterLabel) => {
            if (!filterValue || filterValue === 'all') {
                table.column(statusColumnIndex).search('').draw();
                if (totalCountEl) {
                    totalCountEl.textContent = table.rows().data().length;
                }
                return;
            }

            const regex = '^' + escapeRegex(filterLabel) + '$';
            table.column(statusColumnIndex).search(regex, true, false).draw();
            if (totalCountEl) {
                totalCountEl.textContent = table.rows({ filter: 'applied' }).data().length;
            }
        };

        chips.forEach((chip) => {
            chip.addEventListener('click', () => {
                if (chip.classList.contains('active')) {
                    return;
                }

                chips.forEach((c) => {
                    c.classList.remove('active');
                    c.setAttribute('aria-pressed', 'false');
                });

                chip.classList.add('active');
                chip.setAttribute('aria-pressed', 'true');

                const filterValue = chip.dataset.statusFilter;
                const filterLabel = chip.dataset.statusLabel || '';
                applyStatusFilter(filterValue, filterLabel);
                updateKpiValues(table);
                updateLastUpdatedInfo(table);
            });
        });

        if (searchInput) {
            searchInput.addEventListener('input', () => {
                table.search(searchInput.value).draw();
                updateKpiValues(table);
                updateLastUpdatedInfo(table);
            });
        }

        if (clearButton && searchInput) {
            clearButton.addEventListener('click', () => {
                searchInput.value = '';
                table.search('').draw();
                chips.forEach((chip) => {
                    const isAll = chip.dataset.statusFilter === 'all';
                    chip.classList.toggle('active', isAll);
                    chip.setAttribute('aria-pressed', String(isAll));
                });
                table.column(statusColumnIndex).search('').draw();
                if (totalCountEl) {
                    totalCountEl.textContent = table.rows().data().length;
                }
                updateKpiValues(table);
                updateLastUpdatedInfo(table);
            });
        }

        table.on('draw', () => {
            updateKpiValues(table);
            updateLastUpdatedInfo(table);
        });
    };

    const initShipmentsList = () => {
        if (!window.jQuery || !window.jQuery.fn.dataTable) {
            return;
        }

        const tableEl = document.getElementById('shipmentsTable');
        if (!tableEl) {
            return;
        }

        const dataTable = initializeDataTable(tableEl);
        if (!dataTable) {
            return;
        }

        registerFilters(dataTable, tableEl);
        updateKpiValues(dataTable);
        updateLastUpdatedInfo(dataTable);
    };

    document.addEventListener('DOMContentLoaded', initShipmentsList);
})();

