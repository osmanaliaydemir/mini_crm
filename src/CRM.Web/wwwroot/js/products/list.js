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

        const tableEl = document.getElementById('productsTable');
        if (!tableEl) {
            return null;
        }

        const $table = window.jQuery(tableEl);
        if (window.jQuery.fn.dataTable.isDataTable(tableEl)) {
            $table.DataTable().destroy();
        }

        const htmlElement = document.documentElement;
        const culture = (htmlElement && htmlElement.lang ? htmlElement.lang.toLowerCase() : 'en');
        const languageUrl = languageMap[culture] ?? languageMap.en;
        const responsiveFeatureAvailable = typeof window.jQuery.fn.dataTable.Responsive !== 'undefined';
        const orderColumnAttr = $table.data('order-column');
        const orderDirectionAttr = ($table.data('order-direction') || 'asc').toString().toLowerCase();

        // Query string'den search filter'ı al
        const urlParams = new URLSearchParams(window.location.search);
        const searchFilter = urlParams.get('search') || '';

        const options = {
            processing: true,
            serverSide: true,
            ajax: {
                url: window.location.pathname + '?handler=Data',
                type: 'GET',
                data: function (d) {
                    // Search filter'ı koru
                    if (searchFilter) {
                        d.search = searchFilter;
                    }
                    // DataTables'in kendi search parametresini kullan
                    return d;
                },
                error: function (xhr, error, thrown) {
                    console.error('DataTables error:', error);
                }
            },
            paging: true,
            searching: true,
            ordering: true,
            responsive: responsiveFeatureAvailable,
            info: true,
            lengthChange: true,
            pageLength: 10,
            dom: 'rtip',
            order: [],
            language: {
                url: languageUrl
            },
            columns: [
                { data: 0, name: 'Name', orderable: true },
                { data: 1, name: 'Species', orderable: true },
                { data: 2, name: 'Grade', orderable: true },
                { data: 3, name: 'UnitOfMeasure', orderable: true },
                { data: 4, name: 'Actions', orderable: false }
            ]
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

    const onReady = () => {
        initialiseDataTable();
    };

    document.addEventListener('DOMContentLoaded', onReady);
})();

