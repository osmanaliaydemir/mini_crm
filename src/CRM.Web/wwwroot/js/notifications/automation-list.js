'use strict';

(function () {
    const languageMap = {
        tr: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/tr.json',
        en: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/en-GB.json',
        ar: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/ar.json'
    };

    const escapeHtml = (value) => {
        if (value === null || value === undefined) {
            return '';
        }
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    };

    const getTableData = () => {
        const data = window.emailAutomationTableData;
        if (Array.isArray(data)) {
            return data;
        }
        return [];
    };

    const initializeDataTable = () => {
        if (!window.jQuery || !window.jQuery.fn || !window.jQuery.fn.dataTable) {
            return null;
        }

        const tableEl = document.getElementById('automationRulesTable');
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
        const data = getTableData();
        const config = window.emailAutomationTableConfig || {};

        const options = {
            data,
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
            },
            columns: [
                {
                    data: 'Name',
                    render: (value, type, row) => {
                        const cron = row.CronExpression ? `<div class="muted">${escapeHtml(row.CronExpression)}</div>` : '';
                        return `<div class="cell-strong">${escapeHtml(value)}</div>${cron}`;
                    }
                },
                {
                    data: 'ResourceLabel',
                    render: (value) => `<strong>${escapeHtml(value)}</strong>`
                },
                {
                    data: 'TriggerLabel',
                    render: (value) => escapeHtml(value)
                },
                {
                    data: 'ExecutionLabel',
                    render: (value) => escapeHtml(value)
                },
                {
                    data: 'RecipientSummary',
                    render: (value) => escapeHtml(value)
                },
                {
                    data: 'StatusLabel',
                    render: (value, type, row) => {
                        const statusClass = row.StatusCssClass === 'success' ? 'success' : '';
                        return `<span class="status-badge ${statusClass}">${escapeHtml(value)}</span>`;
                    }
                },
                {
                    data: null,
                    orderable: false,
                    searchable: false,
                    render: (value, type, row) => {
                        const viewLabel = escapeHtml(config.viewLabel || 'View');
                        const editLabel = escapeHtml(config.editLabel || 'Edit');
                        const deleteLabel = escapeHtml(config.deleteLabel || 'Delete');
                        const toggleLabel = escapeHtml(row.ToggleLabel || '');
                        const deleteClass = escapeHtml(config.deleteClass || 'text-danger');

                        const detailsLink = `<a class="btn btn-link" href="${escapeHtml(row.DetailsUrl)}">${viewLabel}</a>`;
                        const editLink = `<a class="btn btn-link" href="${escapeHtml(row.EditUrl)}">${editLabel}</a>`;
                        const toggleButton = row.ToggleFormId
                            ? `<button type="button" class="btn btn-link" data-toggle-form="${escapeHtml(row.ToggleFormId)}">${toggleLabel}</button>`
                            : '';
                        const deleteLink = `<a class="btn btn-link ${deleteClass}" href="${escapeHtml(row.DeleteUrl)}">${deleteLabel}</a>`;

                        return `<div class="action-buttons text-right">${detailsLink}${editLink}${toggleButton}${deleteLink}</div>`;
                    }
                }
            ],
            error: function (xhr, error, thrown) {
                console.error('Automation DataTable error:', error, thrown);
            }
        };

        if (typeof orderColumnAttr !== 'undefined') {
            const idx = parseInt(orderColumnAttr, 10);
            if (!Number.isNaN(idx)) {
                const dir = orderDirectionAttr === 'desc' ? 'desc' : 'asc';
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
            console.error('Automation DataTable initialization failed:', error);
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

        const searchInput = document.querySelector('[data-automation-search]');
        const clearButton = document.querySelector('[data-automation-search-clear]');

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

    const bindToggleButtons = () => {
        document.addEventListener('click', (event) => {
            const button = event.target.closest('[data-toggle-form]');
            if (!button) {
                return;
            }

            event.preventDefault();
            const formId = button.getAttribute('data-toggle-form');
            if (!formId) {
                return;
            }

            const form = document.getElementById(formId);
            if (form) {
                form.submit();
            }
        });
    };

    document.addEventListener('DOMContentLoaded', () => {
        const table = initializeDataTable();
        bindSearchControls(table);
        bindToggleButtons();
    });
})();


