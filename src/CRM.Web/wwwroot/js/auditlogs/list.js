'use strict';

(function () {
    const languageMap = {
        tr: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/tr.json',
        en: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/en-GB.json',
        ar: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/ar.json'
    };

    const escapeHtml = (value) => {
        if (typeof value !== 'string') {
            return '';
        }
        return value
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    };

    const getData = () => {
        const tableData = window.auditLogsTableData;
        if (Array.isArray(tableData)) {
            return tableData;
        }
        return [];
    };

    const initializeDataTable = () => {
        if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.dataTable) {
            return null;
        }

        const tableEl = document.getElementById('auditLogsTable');
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
        const data = getData();

        const options = {
            data,
            paging: true,
            searching: true,
            ordering: true,
            responsive: responsiveFeatureAvailable,
            info: true,
            lengthChange: false,
            pageLength: 20,
            dom: 'rtip',
            order: [],
            language: {
                url: languageUrl
            },
            columns: [
                {
                    data: 'Timestamp',
                    render: (value) => `<div class="cell-strong">${escapeHtml(value)}</div>`
                },
                {
                    data: 'EntityType',
                    render: (value) => `<strong>${escapeHtml(value)}</strong>`
                },
                {
                    data: 'EntityIdShort',
                    render: (value) => `<code>${escapeHtml(value)}</code>`
                },
                {
                    data: 'ActionDisplay',
                    render: (value, type, row) => {
                        const cssClass = escapeHtml(row.ActionCssClass ?? 'unknown');
                        return `<span class="action-badge action-${cssClass}">${escapeHtml(value)}</span>`;
                    }
                },
                {
                    data: 'UserLabel',
                    render: (value) => escapeHtml(value)
                },
                {
                    data: 'IpAddress',
                    render: (value) => escapeHtml(value)
                },
                {
                    data: 'DetailsUrl',
                    orderable: false,
                    searchable: false,
                    render: (value) => {
                        const config = window.auditLogsTableConfig || {};
                        const viewLabel = escapeHtml(config.viewLabel || 'View');
                        const url = typeof value === 'string' ? value : '#';
                        return `
                            <div class="action-buttons">
                                <a href="${escapeHtml(url)}" class="btn-icon" title="${viewLabel}">
                                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="16" height="16">
                                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
                                        <circle cx="12" cy="12" r="3"></circle>
                                    </svg>
                                </a>
                            </div>`;
                    }
                }
            ],
            error: function (xhr, error, thrown) {
                console.error('AuditLogs DataTable error:', error, thrown);
            }
        };

        if (typeof orderColumnAttr !== 'undefined') {
            const idx = parseInt(orderColumnAttr, 10);
            if (!Number.isNaN(idx)) {
                const dir = orderDirectionAttr === 'asc' ? 'asc' : 'desc';
                options.order = [[idx, dir]];
            }
        }

        try {
            const dataTable = $table.DataTable(options);
            if (dataTable.rows().count() === 0) {
                const emptyMessage = tableEl.closest('.table-wrapper')?.querySelector('.table-empty-message');
                if (emptyMessage) {
                    emptyMessage.style.display = 'block';
                }
            }
            return dataTable;
        } catch (error) {
            console.error('AuditLogs DataTable initialization failed:', error);
            const emptyMessage = tableEl.closest('.table-wrapper')?.querySelector('.table-empty-message');
            if (emptyMessage) {
                emptyMessage.style.display = 'block';
            }
            return null;
        }
    };

    const bindSearchControls = (table) => {
        if (!table) {
            return;
        }

        const searchInput = document.querySelector('[data-auditlogs-search]');
        const clearButton = document.querySelector('[data-auditlogs-search-clear]');

        if (searchInput) {
            searchInput.addEventListener('input', () => {
                table.search(searchInput.value).draw();
            });
        }

        if (clearButton && searchInput) {
            clearButton.addEventListener('click', () => {
                searchInput.value = '';
                table.search('').draw();
                searchInput.focus();
            });
        }
    };

    document.addEventListener('DOMContentLoaded', () => {
        const table = initializeDataTable();
        bindSearchControls(table);
    });
})();

