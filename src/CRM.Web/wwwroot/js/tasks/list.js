document.addEventListener('DOMContentLoaded', function () {
    const table = document.querySelector('[data-tasks-table]');
    if (!table) return;

    // DataTable initialization if needed
    if (typeof $ !== 'undefined' && $.fn.DataTable) {
        $(table).DataTable({
            responsive: true,
            pageLength: 25,
            order: [[0, 'asc']],
            language: {
                search: '',
                searchPlaceholder: 'Search tasks...'
            }
        });
    }
});

