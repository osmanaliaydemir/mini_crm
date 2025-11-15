/* global $, document */

$(function () {
    if (!$.fn.dataTable) {
        return;
    }

    const languageMap = {
        tr: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/tr.json',
        en: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/en-GB.json',
        ar: 'https://cdn.datatables.net/plug-ins/2.1.3/i18n/ar.json'
    };

    const htmlElement = document.documentElement;
    const culture = (htmlElement && htmlElement.lang ? htmlElement.lang.toLowerCase() : 'en');
    const languageUrl = languageMap[culture] ?? languageMap.en;

    const responsiveFeatureAvailable = typeof $.fn.dataTable.Responsive !== 'undefined';

    $('table[data-crm-datatable="true"]').each(function () {
        const $table = $(this);
        if ($.fn.dataTable.isDataTable(this)) {
            return;
        }
        const disableLastColumn = $table.data('disable-last-column') === true || $table.data('disable-last-column') === 'true';
        const responsiveRequested = $table.data('responsive');
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

        $table.DataTable(options);
    });
});
