const dashboardState = {
    theme: 'light',
    shipments: [
        {
            id: 'SHP-23108',
            name: 'SHP-23108 • Kereste Sevkiyatı',
            status: 'Depoda',
            statusClass: 'status-on-track',
            customer: 'Kuzey Orman Ltd.',
            eta: '12 Kas 2025',
            risk: 'Düşük',
            riskClass: 'text-emerald-600',
            segment: 'wood'
        },
        {
            id: 'SHP-23112',
            name: 'SHP-23112 • Çelik Profil',
            status: 'Gümrükte Bekliyor',
            statusClass: 'status-delayed',
            customer: 'Megalit Yapı',
            eta: '15 Kas 2025',
            risk: 'Orta',
            riskClass: 'text-amber-600',
            segment: 'steel'
        },
        {
            id: 'SHP-23115',
            name: 'SHP-23115 • Lamine Panel',
            status: 'Gemide',
            statusClass: 'status-on-track',
            customer: 'Nova Mobilya',
            eta: '18 Kas 2025',
            risk: 'Düşük',
            riskClass: 'text-emerald-600',
            segment: 'wood'
        },
        {
            id: 'SHP-23119',
            name: 'SHP-23119 • Medikal Kabin',
            status: 'Vagonda',
            statusClass: 'status-critical',
            customer: 'LifePlus Sağlık',
            eta: '11 Kas 2025',
            risk: 'Yüksek',
            riskClass: 'text-rose-600',
            segment: 'medical'
        }
    ],
    kpis: [
        {
            label: 'Aktif Sevkiyat',
            value: 128,
            delta: '+12%',
            deltaLabel: 'Aylık artış',
            icon: 'fa-truck-fast',
            accent: 'from-sky-500 to-blue-500'
        },
        {
            label: 'Net Kâr',
            value: '₺2.4M',
            delta: '+8%',
            deltaLabel: 'Son 30 gün',
            icon: 'fa-sack-dollar',
            accent: 'from-emerald-500 to-teal-500'
        },
        {
            label: 'Tahsilat Oranı',
            value: '92%',
            delta: '+3%',
            deltaLabel: 'Tahsil edilen',
            icon: 'fa-chart-pie',
            accent: 'from-violet-500 to-indigo-500'
        },
        {
            label: 'Müşteri Sadakati',
            value: '4.6/5',
            delta: '+0.3',
            deltaLabel: 'NPS skor',
            icon: 'fa-handshake',
            accent: 'from-amber-500 to-orange-500'
        }
    ],
    tasks: [
        {
            title: 'Gümrük belgelerini doğrula',
            owner: 'Serkan Genç',
            due: 'Bugün • 16:30',
            progress: 70,
            tags: ['Operasyon', 'Acil']
        },
        {
            title: 'Lamine panel stok analizini paylaş',
            owner: 'Büşra Demir',
            due: 'Yarın • 10:00',
            progress: 40,
            tags: ['Stok', 'Analiz']
        },
        {
            title: 'Müşteri sadakat anketini başlat',
            owner: 'İlker Yılmaz',
            due: '14 Kas • 09:00',
            progress: 10,
            tags: ['CRM', 'Anket']
        }
    ],
    metrics: [
        { label: 'Operasyon Verimi', value: 86, accent: 'text-emerald-500' },
        { label: 'Finansal Sağlamlık', value: 78, accent: 'text-indigo-500' },
        { label: 'Müşteri Memnuniyeti', value: 91, accent: 'text-amber-500' }
    ],
    profitTrend: {
        labels: ['Ara 24', 'Oca 25', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas'],
        values: [120, 150, 132, 168, 190, 205, 220, 240, 260, 250, 270, 288]
    }
};

const notyf = new Notyf({
    duration: 2500,
    position: {
        x: 'right',
        y: 'top'
    }
});

function renderKpis() {
    const $container = $('#kpi-section').empty();
    dashboardState.kpis.forEach(kpi => {
        const card = $(`
            <article class="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-slate-100 transition hover:-translate-y-1 hover:shadow-xl">
                <div class="flex items-center justify-between">
                    <div>
                        <p class="text-sm font-medium text-slate-500">${kpi.label}</p>
                        <h3 class="mt-2 text-3xl font-semibold tracking-tight text-slate-900">${kpi.value}</h3>
                    </div>
                    <span class="h-12 w-12 rounded-2xl bg-gradient-to-br ${kpi.accent} text-white grid place-items-center text-lg">
                        <i class="fa-solid ${kpi.icon}"></i>
                    </span>
                </div>
                <div class="mt-4 flex items-center gap-2 text-sm text-emerald-600">
                    <i class="fa-solid fa-arrow-trend-up"></i>
                    <span class="font-semibold">${kpi.delta}</span>
                    <span class="text-slate-500">${kpi.deltaLabel}</span>
                </div>
            </article>
        `);
        $container.append(card);
    });
}

function renderShipments(filter = 'all') {
    const $table = $('#shipment-table').empty();

    dashboardState.shipments
        .filter(shipment => filter === 'all' || shipment.segment === filter)
        .forEach(shipment => {
            const row = $(`
                <tr class="hover:bg-slate-50 transition">
                    <td class="whitespace-nowrap px-6 py-4 text-sm font-semibold text-slate-900">${shipment.name}</td>
                    <td class="whitespace-nowrap px-6 py-4">
                        <span class="status-chip ${shipment.statusClass}">
                            <i class="fa-solid fa-signal"></i>
                            ${shipment.status}
                        </span>
                    </td>
                    <td class="whitespace-nowrap px-6 py-4 text-sm text-slate-600">${shipment.customer}</td>
                    <td class="whitespace-nowrap px-6 py-4 text-sm text-slate-600">${shipment.eta}</td>
                    <td class="whitespace-nowrap px-6 py-4 text-sm font-semibold ${shipment.riskClass}">${shipment.risk}</td>
                    <td class="whitespace-nowrap px-6 py-4 text-right text-sm">
                        <button class="inline-flex items-center gap-2 rounded-lg border border-slate-200 px-3 py-1.5 text-xs font-semibold uppercase tracking-wide text-slate-600 hover:bg-slate-100">
                            Detay
                            <i class="fa-solid fa-arrow-right-long"></i>
                        </button>
                    </td>
                </tr>
            `);
            $table.append(row);
        });
}

function renderFilterOptions() {
    const uniqueSegments = new Set(dashboardState.shipments.map(s => s.segment));
    const $filter = $('#shipment-filter');
    uniqueSegments.forEach(segment => {
        const name = segment === 'wood' ? 'Kereste' :
            segment === 'steel' ? 'Çelik' :
            segment === 'medical' ? 'Medikal' : segment;
        $filter.append(`<option value="${segment}">${name}</option>`);
    });
}

function renderTasks() {
    const $list = $('#task-list').empty();
    dashboardState.tasks.forEach(task => {
        const tags = task.tags.map(tag =>
            `<span class="rounded-full bg-indigo-50 px-3 py-1 text-xs font-semibold text-indigo-600">${tag}</span>`
        ).join('');

        const card = $(`
            <article class="task-card bg-white">
                <div class="flex items-start justify-between">
                    <div>
                        <h3 class="text-lg font-semibold text-slate-900">${task.title}</h3>
                        <p class="mt-1 text-sm text-slate-500">${task.owner}</p>
                    </div>
                    <span class="rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-600">
                        ${task.due}
                    </span>
                </div>
                <div class="mt-4 flex flex-wrap items-center gap-2">
                    ${tags}
                </div>
                <div class="mt-4">
                    <div class="flex items-center justify-between text-xs font-semibold text-slate-500">
                        <span>İlerleme</span>
                        <span>${task.progress}%</span>
                    </div>
                    <div class="progress-bar mt-2">
                        <span style="width: ${task.progress}%"></span>
                    </div>
                </div>
            </article>
        `);

        $list.append(card);
    });
}

function renderHealthMetrics() {
    const $metrics = $('#health-metrics').empty();
    dashboardState.metrics.forEach(metric => {
        $metrics.append(`
            <li class="flex items-center justify-between rounded-2xl bg-slate-50 px-4 py-3">
                <span class="text-sm font-medium text-slate-600">${metric.label}</span>
                <span class="text-lg font-semibold ${metric.accent}">${metric.value}</span>
            </li>
        `);
    });
}

function initCharts() {
    const gaugeCtx = document.getElementById('healthGauge');
    const profitCtx = document.getElementById('profitChart');

    const gaugeData = dashboardState.metrics.map(m => m.value);
    const gaugeChart = new Chart(gaugeCtx, {
        type: 'doughnut',
        data: {
            labels: dashboardState.metrics.map(m => m.label),
            datasets: [{
                label: 'Sağlık Puanı',
                data: gaugeData,
                backgroundColor: ['#10b981', '#6366f1', '#f59e0b'],
                borderColor: '#ffffff',
                borderWidth: 4,
                cutout: '75%'
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: { position: 'bottom' }
            }
        }
    });

    const gradient = profitCtx.getContext('2d').createLinearGradient(0, 0, 0, 300);
    gradient.addColorStop(0, 'rgba(99, 102, 241, 0.35)');
    gradient.addColorStop(1, 'rgba(79, 70, 229, 0)');

    const profitChart = new Chart(profitCtx, {
        type: 'line',
        data: {
            labels: dashboardState.profitTrend.labels,
            datasets: [{
                label: 'Net Kâr (₺M)',
                data: dashboardState.profitTrend.values,
                fill: true,
                backgroundColor: gradient,
                borderColor: '#6366f1',
                borderWidth: 3,
                tension: 0.35,
                pointBackgroundColor: '#ffffff',
                pointBorderColor: '#6366f1',
                pointBorderWidth: 2,
                pointHoverRadius: 5
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    ticks: {
                        callback: value => `₺${value}`
                    },
                    grid: { color: 'rgba(148, 163, 184, 0.1)' }
                },
                x: {
                    grid: { display: false }
                }
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: ctx => `Net kâr: ₺${ctx.formattedValue}M`
                    }
                }
            }
        }
    });

    return { gaugeChart, profitChart };
}

function toggleTheme() {
    dashboardState.theme = dashboardState.theme === 'light' ? 'dark' : 'light';
    $('body').toggleClass('dark', dashboardState.theme === 'dark');
    localStorage.setItem('dashboardTheme', dashboardState.theme);
}

function boot() {
    const savedTheme = localStorage.getItem('dashboardTheme');
    if (savedTheme === 'dark') {
        dashboardState.theme = 'dark';
        $('body').addClass('dark');
    }

    renderKpis();
    renderShipments();
    renderFilterOptions();
    renderTasks();
    renderHealthMetrics();
    initCharts();

    $('#theme-toggle').on('click', () => {
        toggleTheme();
        notyf.success(`Tema ${dashboardState.theme === 'dark' ? 'gece' : 'gündüz'} moduna alındı`);
    });

    $('#shipment-filter').on('change', function () {
        const segment = $(this).val();
        renderShipments(segment);
        notyf.open({
            type: 'info',
            message: segment === 'all'
                ? 'Tüm sevkiyatlar gösteriliyor'
                : `Filtre: ${$(this).find('option:selected').text()}`
        });
    });

    $('#add-task').on('click', () => {
        notyf.success('Yeni görev ekleme akışı başlatılabilir.');
    });
}

$(boot);


