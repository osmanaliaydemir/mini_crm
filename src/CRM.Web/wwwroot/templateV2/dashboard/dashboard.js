/* eslint-disable no-undef */
const STORAGE_KEY = 'dashboardTheme';
const LANG_STORAGE_KEY = 'dashboardLanguage';
const DEFAULT_RANGE = '30d';

function getDashboardData() {
    if (typeof window !== 'undefined' && window.dashboardData) {
        return window.dashboardData;
    }
    return fallbackData;
}

function getDefaultRange() {
    if (typeof window !== 'undefined' && window.dashboardDefaultRange) {
        return window.dashboardDefaultRange;
    }
    return DEFAULT_RANGE;
}

function getLocale() {
    if (typeof document !== 'undefined') {
        return document.documentElement.lang || 'tr-TR';
    }
    return 'tr-TR';
}

const locale = getLocale();
const currencyFormatter = new Intl.NumberFormat(locale, { style: 'currency', currency: 'TRY', maximumFractionDigits: 0 });
const numberFormatter = new Intl.NumberFormat(locale, { maximumFractionDigits: 1 });

function formatCurrency(value) {
    return currencyFormatter.format(value);
}

function formatNumber(value) {
    return numberFormatter.format(value);
}

function formatPercent(value) {
    return new Intl.NumberFormat(locale, { style: 'percent', maximumFractionDigits: 0 }).format(value);
}

const fallbackData = {
    status: [
        { label: 'API', value: '89 ms', state: 'good' },
        { label: 'Broker Kanalı', value: 'Stabil', state: 'good' },
        { label: 'Gümrük Botu', value: 'Bakım 12:45', state: 'warn' },
    ],
    tasks: [
        { title: 'Çelik profil sevkiyatını doğrula', owner: 'Serkan Genç', due: 'Bugün · 14:30', progress: 72 },
        { title: 'Tedarikçi kalite raporunu paylaş', owner: 'Büşra Demir', due: 'Yarın · 10:00', progress: 45 },
        { title: 'Yeni CRM müşterilerini içeri aktar', owner: 'İlker Yılmaz', due: '11 Ağu · 09:00', progress: 20 },
    ],
    suppliers: {
        labels: ['Finlandiya', 'Letonya', 'İsveç', 'Estonya', 'Diğer'],
        values: [12, 9, 6, 4, 3],
    },
    ranges: {
        '7d': {
            kpis: {
                activeShipments: { value: '118', trendLabel: '+6%', tone: 'positive', meta: 'Son 7 gün' },
                customsShipments: { value: '14', trendLabel: '3 risk', tone: 'warning', meta: 'Takip edilen dosya' },
                cashNet: { value: '₺1.85M', trendLabel: '+4%', tone: 'positive', meta: '7 günlük net' },
                satisfaction: { value: '4.5 / 5', trendLabel: '-0.1', tone: 'negative', meta: 'Son anket' },
            },
            shipmentStatus: {
                labels: ['Depoda', 'Gümrükte', 'Yolda', 'Bayi Teslim'],
                values: [28, 14, 46, 22],
            },
            cashFlow: {
                labels: ['Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem'],
                income: [380, 420, 450, 470, 510, 540],
                expense: [280, 300, 310, 320, 340, 360],
            },
            riskShipments: [
                { code: 'SHP-23098', customer: 'Mega Yapı', status: 'Belge onayı bekleniyor', eta: '10 Ağu 2025' },
                { code: 'SHP-23107', customer: 'Orion İç Mekan', status: 'İstanbul deposu yoğun', eta: '12 Ağu 2025' },
            ],
            activities: [
                { time: '14:20', title: 'SHP-23098 teslim edildi', detail: 'Mega Yapı sevkiyatı bayi depoya ulaştı.', type: 'shipment', typeLabel: 'Sevkiyat' },
                { time: '12:45', title: 'Yeni depo transferi', detail: 'İzmir depo kapasitesi %82 seviyesine çıktı.', type: 'ops', typeLabel: 'Operasyon' },
                { time: '09:30', title: 'CRM güncellemesi', detail: 'Üç yeni müşteri kaydı CRM’e aktarıldı.', type: 'crm', typeLabel: 'CRM' },
            ],
        },
        '30d': {
            kpis: {
                activeShipments: { value: '132', trendLabel: '+12%', tone: 'positive', meta: 'Son 30 gün' },
                customsShipments: { value: '17', trendLabel: '5 risk', tone: 'warning', meta: 'Takip edilen dosya' },
                cashNet: { value: '₺2.45M', trendLabel: '+8%', tone: 'positive', meta: '30 günlük net' },
                satisfaction: { value: '4.6 / 5', trendLabel: 'Sabitledi', tone: 'neutral', meta: 'Son anket' },
            },
            shipmentStatus: {
                labels: ['Depoda', 'Gümrükte', 'Yolda', 'Bayi Teslim'],
                values: [32, 17, 54, 29],
            },
            cashFlow: {
                labels: ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu'],
                income: [410, 450, 480, 520, 600, 650, 700, 740],
                expense: [320, 330, 340, 360, 390, 420, 440, 460],
            },
            riskShipments: [
                { code: 'SHP-23108', customer: 'Nova Mobilya', status: 'Gümrükte Bekliyor', eta: '12 Ağu 2025' },
                { code: 'SHP-23112', customer: 'Kuzey Orman Ltd.', status: 'Gecikme uyarısı gönderildi', eta: '09 Ağu 2025' },
                { code: 'SHP-23119', customer: 'LifePlus Sağlık', status: 'Depoda hasar incelemesi', eta: 'Beklemede' },
                { code: 'SHP-23124', customer: 'Megalit Yapı', status: 'Evrak bekleniyor', eta: '18 Ağu 2025' },
            ],
            activities: [
                { time: '14:40', title: 'SHP-23108 gümrük onayı', detail: 'Nova Mobilya sevkiyatı belgelendi.', type: 'shipment', typeLabel: 'Sevkiyat' },
                { time: '13:05', title: 'Yeni müşteri kaydı', detail: 'Akdeniz Park için CRM kartı oluşturuldu.', type: 'crm', typeLabel: 'CRM' },
                { time: '10:20', title: 'Nakit akışı güncellendi', detail: 'Ağustos projeksiyonu güncellendi.', type: 'finance', typeLabel: 'Finans' },
            ],
        },
        '90d': {
            kpis: {
                activeShipments: { value: '348', trendLabel: '+21%', tone: 'positive', meta: 'Son 90 gün' },
                customsShipments: { value: '39', trendLabel: '9 risk', tone: 'warning', meta: 'Kritik dosya' },
                cashNet: { value: '₺7.62M', trendLabel: '+12%', tone: 'positive', meta: 'Çeyreklik net' },
                satisfaction: { value: '4.7 / 5', trendLabel: '+0.2', tone: 'positive', meta: 'Çeyreklik anket' },
            },
            shipmentStatus: {
                labels: ['Depoda', 'Gümrükte', 'Yolda', 'Bayi Teslim'],
                values: [78, 39, 146, 85],
            },
            cashFlow: {
                labels: ['Ara', 'Kas', 'Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem'],
                income: [320, 360, 410, 450, 480, 520, 575, 630, 710],
                expense: [260, 280, 320, 330, 350, 370, 410, 430, 455],
            },
            riskShipments: [
                { code: 'SHP-22988', customer: 'Atlas İnşaat', status: 'Yükleme tekrarı planlandı', eta: '24 Ağu 2025' },
                { code: 'SHP-23005', customer: 'Ege Kereste', status: 'Depoda kalite sorunu', eta: '19 Ağu 2025' },
                { code: 'SHP-23056', customer: 'Orbital Yapı', status: 'Eksik evrak tamamlanacak', eta: '23 Ağu 2025' },
            ],
            activities: [
                { time: '15:05', title: 'Çeyreklik rapor hazır', detail: 'Finans ekibi Q3 raporunu paylaştı.', type: 'finance', typeLabel: 'Finans' },
                { time: '11:50', title: 'Yeni depo entegrasyonu', detail: 'Sakarya deposu sistemle entegre edildi.', type: 'ops', typeLabel: 'Operasyon' },
                { time: '08:30', title: 'CRM skorları yükseldi', detail: 'Müşteri memnuniyeti %94 seviyesine çıktı.', type: 'crm', typeLabel: 'CRM' },
            ],
        },
    },
};

let currentRange = getDefaultRange();
let shipmentChart;
let cashFlowChart;
let supplierChart;

function handleSidebarToggle() {
    const sidebar = document.querySelector('[data-sidebar]');
    const toggleBtn = document.querySelector('[data-sidebar-toggle]');
    if (!sidebar || !toggleBtn) return;

    let backdrop = document.querySelector('.sidebar-backdrop');
    if (!backdrop) {
        backdrop = document.createElement('div');
        backdrop.className = 'sidebar-backdrop';
        document.body.appendChild(backdrop);
    }

    const setSidebarState = (open) => {
        sidebar.setAttribute('data-open', String(open));
        backdrop.setAttribute('data-visible', String(open));
    };

    toggleBtn.addEventListener('click', () => {
        const isOpen = sidebar.getAttribute('data-open') === 'true';
        setSidebarState(!isOpen);
    });

    backdrop.addEventListener('click', () => setSidebarState(false));

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            setSidebarState(false);
        }
    });
}

function renderStatusBar() {
    const statusContainer = document.querySelector('[data-status-bar]');
    if (!statusContainer) return;

    const data = getDashboardData();
    const items = Array.isArray(data.status) ? data.status : fallbackData.status;
    if (!items.length) {
        statusContainer.innerHTML = '<span class="status-empty">Sistem durumu bilgisi yok</span>';
        return;
    }

    statusContainer.innerHTML = items.map((item) => `
        <div class="status-item status-${item.state}">
            <span class="status-dot"></span>
            <div class="status-text">
                <span>${item.label}</span>
                <strong>${item.value}</strong>
            </div>
        </div>
    `).join('');
}

function renderKpis() {
    const rangeData = getRangeData();
    const kpis = rangeData?.kpis ?? {};
    document.querySelectorAll('[data-kpi]').forEach((card) => {
        const key = card.dataset.kpi;
        const kpi = kpis[key];
        if (!kpi) return;

        const valueEl = card.querySelector('[data-kpi-value]');
        const metaEl = card.querySelector('[data-kpi-meta]');
        const trendChip = card.querySelector('[data-kpi-trend]');
        const trendValue = card.querySelector('[data-kpi-trend-value]');

        if (valueEl) valueEl.textContent = kpi.value;
        if (metaEl) metaEl.textContent = kpi.meta;
        if (trendValue) trendValue.textContent = kpi.trendLabel;
        if (trendChip) {
            trendChip.classList.remove('positive', 'warning', 'negative', 'neutral');
            trendChip.classList.add(kpi.tone || 'neutral');
        }
    });
}

function renderRiskShipments() {
    const tbody = document.getElementById('riskShipmentTable');
    if (!tbody) return;

    const rangeData = getRangeData();
    const shipments = Array.isArray(rangeData?.riskShipments) ? rangeData.riskShipments : [];
    if (!shipments.length) {
        tbody.innerHTML = '<tr><td colspan="4" class="empty-state">Riskli sevkiyat bulunmuyor.</td></tr>';
        return;
    }

    tbody.innerHTML = shipments.map((item) => `
        <tr>
            <td data-label="Sevkiyat"><strong>${item.code}</strong></td>
            <td data-label="Müşteri">${item.customer}</td>
            <td data-label="Durum">${item.status}</td>
            <td data-label="Tahmini Varış">${item.eta}</td>
        </tr>
    `).join('');
}

function renderTasks() {
    const taskList = document.getElementById('taskList');
    if (!taskList) return;

    const data = getDashboardData();
    const tasks = Array.isArray(data.tasks) ? data.tasks : fallbackData.tasks;
    if (!tasks.length) {
        taskList.innerHTML = '<li class="task-card empty-state">Görev verisi bulunmuyor.</li>';
        return;
    }

    taskList.innerHTML = tasks.map((task) => `
        <li class="task-card">
            <strong>${task.title}</strong>
            <div class="task-meta">
                <span>${task.owner}</span>
                <span>${task.due}</span>
            </div>
            <div class="progress-bar">
                <span style="width:${task.progress}%"></span>
            </div>
        </li>
    `).join('');
}

function renderActivityFeed() {
    const feed = document.getElementById('activityFeed');
    if (!feed) return;

    const rangeData = getRangeData();
    const activities = Array.isArray(rangeData?.activities) ? rangeData.activities : [];
    if (!activities.length) {
        feed.innerHTML = '<li class="activity-item empty"><span>Henüz aktivite kaydı yok.</span></li>';
        return;
    }

    feed.innerHTML = activities.map((activity) => `
        <li class="activity-item">
            <div class="activity-main">
                <span class="activity-time">${activity.time}</span>
                <div>
                    <strong>${activity.title}</strong>
                    <p>${activity.detail}</p>
                </div>
            </div>
            <span class="activity-chip ${activity.type || 'default'}">${activity.typeLabel || 'Genel'}</span>
        </li>
    `).join('');
}

function initShipmentStatusChart() {
    const ctx = document.getElementById('shipmentStatusChart');
    if (!ctx) return null;

    const gradient = ctx.getContext('2d').createLinearGradient(0, 0, 0, 260);
    gradient.addColorStop(0, 'rgba(37, 99, 235, 0.9)');
    gradient.addColorStop(1, 'rgba(37, 99, 235, 0.4)');

    const data = getRangeData()?.shipmentStatus;
    if (!data) return null;
    return new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.labels,
            datasets: [{
                data: data.values,
                backgroundColor: [
                    gradient,
                    'rgba(14, 165, 233, 0.85)',
                    'rgba(16, 185, 129, 0.8)',
                    'rgba(245, 158, 11, 0.8)',
                ],
                borderWidth: 0,
            }],
        },
        options: {
            cutout: '72%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        color: '#a0aec0',
                        usePointStyle: true,
                    },
                },
            },
        },
    });
}

function updateShipmentChart() {
    if (!shipmentChart) return;
    const data = getRangeData()?.shipmentStatus;
    if (!data) return;
    shipmentChart.data.labels = data.labels;
    shipmentChart.data.datasets[0].data = data.values;
    shipmentChart.update('none');
}

function initCashFlowChart() {
    const ctx = document.getElementById('cashFlowChart');
    if (!ctx) return null;

    const data = getRangeData()?.cashFlow;
    if (!data) return null;

    return new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [
                {
                    label: 'Gelir',
                    data: data.income,
                    borderColor: 'rgba(16, 185, 129, 1)',
                    backgroundColor: 'rgba(16, 185, 129, 0.18)',
                    fill: true,
                    tension: 0.35,
                    pointRadius: 4,
                },
                {
                    label: 'Gider',
                    data: data.expense,
                    borderColor: 'rgba(249, 115, 22, 1)',
                    backgroundColor: 'rgba(249, 115, 22, 0.15)',
                    fill: true,
                    tension: 0.35,
                    pointRadius: 4,
                },
            ],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: (value) => formatCurrency(value),
                        color: '#a0aec0',
                    },
                    grid: {
                        color: 'rgba(148, 163, 184, 0.1)',
                    },
                },
                x: {
                    ticks: { color: '#a0aec0' },
                    grid: { color: 'transparent' },
                },
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: (context) => `${context.dataset.label}: ${formatCurrency(context.parsed.y)}`,
                    },
                },
            },
        },
    });
}

function updateCashFlowChart() {
    if (!cashFlowChart) return;
    const data = getRangeData()?.cashFlow;
    if (!data) return;
    cashFlowChart.data.labels = data.labels;
    cashFlowChart.data.datasets[0].data = data.income;
    cashFlowChart.data.datasets[1].data = data.expense;
    cashFlowChart.update('none');
}

function updateSupplierChart() {
    if (!supplierChart) return;
    const data = getDashboardData();
    if (data?.suppliers && Array.isArray(data.suppliers.labels) && data.suppliers.labels.length === 0) {
        supplierChart.data.labels = [];
        supplierChart.data.datasets[0].data = [];
        supplierChart.update('none');
        return;
    }
    const suppliers = resolveSupplierData();
    supplierChart.data.labels = suppliers.labels;
    supplierChart.data.datasets[0].data = suppliers.values;
    supplierChart.update('none');
}

function initSupplierChart() {
    const ctx = document.getElementById('supplierCountryChart');
    if (!ctx) return null;

    const data = getDashboardData();
    if (data?.suppliers && Array.isArray(data.suppliers.labels) && data.suppliers.labels.length === 0) {
        ctx.style.display = 'none';
        ctx.insertAdjacentHTML('afterend', '<p class="empty-state">Tedarikçi dağılım verisi bulunmuyor.</p>');
        return null;
    }

    const suppliers = resolveSupplierData();

    return new Chart(ctx, {
        type: 'bar',
        data: {
            labels: suppliers.labels,
            datasets: [{
                label: 'Aktif Sipariş',
                data: suppliers.values,
                backgroundColor: 'rgba(37, 99, 235, 0.8)',
                borderRadius: 10,
            }],
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: { color: '#a0aec0' },
                    grid: { color: 'rgba(148, 163, 184, 0.1)' },
                },
                x: {
                    ticks: { color: '#a0aec0' },
                    grid: { color: 'transparent' },
                },
            },
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: (context) => `${context.parsed.y} sipariş`,
                    },
                },
            },
        },
    });
}

function updateChartDescriptions() {
    const shipmentDesc = document.getElementById('shipmentStatusDesc');
    if (shipmentDesc) {
        const data = getRangeData()?.shipmentStatus;
        if (data?.labels?.length) {
            const summary = data.labels.map((label, idx) => `${label} ${data.values[idx]}`).join(', ');
            shipmentDesc.textContent = `Sevkiyatların durum dağılımı: ${summary}.`;
        }
    }

    const cashDesc = document.getElementById('cashFlowDesc');
    if (cashDesc) {
        const cash = getRangeData()?.cashFlow;
        if (cash?.income?.length && cash?.expense?.length) {
            const lastIncome = cash.income[cash.income.length - 1] ?? 0;
            const lastExpense = cash.expense[cash.expense.length - 1] ?? 0;
            cashDesc.textContent = `Gelir ${formatCurrency(lastIncome)}, gider ${formatCurrency(lastExpense)} olarak ölçüldü.`;
        }
    }
}

function getRangeData(range = currentRange) {
    const data = getDashboardData();
    if (data?.ranges && data.ranges[range]) {
        return data.ranges[range];
    }
    if (fallbackData.ranges[range]) {
        return fallbackData.ranges[range];
    }
    return fallbackData.ranges[DEFAULT_RANGE];
}

function setRange(range, fromFilter = false) {
    const data = getDashboardData();
    const hasRange = Boolean(data?.ranges && data.ranges[range]) || Boolean(fallbackData.ranges[range]);
    if (!hasRange) {
        range = DEFAULT_RANGE;
    }

    if (currentRange === range && fromFilter) {
        return;
    }

    currentRange = range;
    renderKpis();
    renderRiskShipments();
    renderActivityFeed();
    updateShipmentChart();
    updateCashFlowChart();
    updateSupplierChart();
    updateChartDescriptions();

    if (!fromFilter) {
        const rangeGroup = document.querySelector('.filter-group[data-filter-group="range"]');
        if (rangeGroup) {
            rangeGroup.querySelectorAll('.filter-chip').forEach((chip) => {
                const isActive = chip.dataset.filterValue === range;
                chip.classList.toggle('active', isActive);
                chip.setAttribute('aria-pressed', String(isActive));
            });
        }
    }
}

function setupFilterBar() {
    document.querySelectorAll('.filter-group[data-filter-group]').forEach((group) => {
        const groupName = group.dataset.filterGroup;
        const chips = Array.from(group.querySelectorAll('.filter-chip'));
        chips.forEach((chip) => {
            chip.addEventListener('click', () => {
                if (chip.classList.contains('active')) return;
                chips.forEach((c) => {
                    c.classList.remove('active');
                    c.setAttribute('aria-pressed', 'false');
                });
                chip.classList.add('active');
                chip.setAttribute('aria-pressed', 'true');

                if (groupName === 'range') {
                    setRange(chip.dataset.filterValue, true);
                }
            });
        });
    });
}

function initializeParallax() {
    const parallaxElements = document.querySelectorAll('[data-depth]');
    if (!parallaxElements.length) {
        return () => {};
    }

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const parallaxState = { currentX: 0, currentY: 0, targetX: 0, targetY: 0, raf: null, listenersAttached: false };

    const shouldDisableParallax = () => prefersReducedMotion.matches || window.innerWidth < 768 || !parallaxElements.length;

    const stepParallax = () => {
        parallaxState.currentX += (parallaxState.targetX - parallaxState.currentX) * 0.08;
        parallaxState.currentY += (parallaxState.targetY - parallaxState.currentY) * 0.08;

        parallaxElements.forEach((el) => {
            const depth = parseFloat(el.dataset.depth || '0');
            const range = parseFloat(el.dataset.range || '40');
            const offsetX = -parallaxState.currentX * depth * range;
            const offsetY = -parallaxState.currentY * depth * range;
            el.style.setProperty('--parallax-x', `${offsetX}px`);
            el.style.setProperty('--parallax-y', `${offsetY}px`);
        });

        parallaxState.raf = requestAnimationFrame(stepParallax);
    };

    const handlePointerMove = (event) => {
        parallaxState.targetX = event.clientX / window.innerWidth - 0.5;
        parallaxState.targetY = event.clientY / window.innerHeight - 0.5;
    };

    const resetParallax = () => {
        parallaxState.targetX = 0;
        parallaxState.targetY = 0;
    };

    const attachListeners = () => {
        if (parallaxState.listenersAttached) return;
        window.addEventListener('pointermove', handlePointerMove);
        window.addEventListener('pointerup', resetParallax);
        window.addEventListener('pointercancel', resetParallax);
        document.addEventListener('mouseleave', resetParallax);
        window.addEventListener('blur', resetParallax);
        parallaxState.listenersAttached = true;
    };

    const detachListeners = () => {
        if (!parallaxState.listenersAttached) return;
        window.removeEventListener('pointermove', handlePointerMove);
        window.removeEventListener('pointerup', resetParallax);
        window.removeEventListener('pointercancel', resetParallax);
        document.removeEventListener('mouseleave', resetParallax);
        window.removeEventListener('blur', resetParallax);
        parallaxState.listenersAttached = false;
    };

    const initParallax = () => {
        if (shouldDisableParallax()) {
            detachListeners();
            parallaxState.currentX = 0;
            parallaxState.currentY = 0;
            parallaxState.targetX = 0;
            parallaxState.targetY = 0;
            parallaxElements.forEach((el) => {
                el.style.setProperty('--parallax-x', '0px');
                el.style.setProperty('--parallax-y', '0px');
            });
            if (parallaxState.raf) {
                cancelAnimationFrame(parallaxState.raf);
                parallaxState.raf = null;
            }
            return;
        }

        if (!parallaxState.raf) {
            parallaxState.raf = requestAnimationFrame(stepParallax);
        }
        attachListeners();
    };

    initParallax();
    if (typeof prefersReducedMotion.addEventListener === 'function') {
        prefersReducedMotion.addEventListener('change', initParallax);
    } else if (typeof prefersReducedMotion.addListener === 'function') {
        prefersReducedMotion.addListener(initParallax);
    }

    return initParallax;
}

function registerUserMenu() {
    const userMenu = document.querySelector('[data-user-menu]');
    if (!userMenu) return;

    const trigger = userMenu.querySelector('.user-trigger');
    const userOptions = Array.from(userMenu.querySelectorAll('.dropdown-panel a'));
    let userActiveIndex = -1;

    const setUserOpen = (open) => {
        userMenu.setAttribute('data-open', String(open));
        trigger.setAttribute('aria-expanded', String(open));
        if (open) {
            userActiveIndex = 0;
            userOptions[userActiveIndex]?.focus();
        } else {
            userActiveIndex = -1;
        }
    };

    trigger.addEventListener('click', () => {
        const isOpen = userMenu.getAttribute('data-open') === 'true';
        setUserOpen(!isOpen);
    });

    trigger.addEventListener('keydown', (event) => {
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            setUserOpen(true);
        } else if (event.key === 'Escape') {
            setUserOpen(false);
        }
    });

    userOptions.forEach((option, index) => {
        option.addEventListener('keydown', (event) => {
            if (event.key === 'ArrowDown') {
                event.preventDefault();
                userActiveIndex = (index + 1) % userOptions.length;
                userOptions[userActiveIndex].focus();
            } else if (event.key === 'ArrowUp') {
                event.preventDefault();
                userActiveIndex = (index - 1 + userOptions.length) % userOptions.length;
                userOptions[userActiveIndex].focus();
            } else if (event.key === 'Escape') {
                setUserOpen(false);
                trigger.focus();
            }
        });
    });

    document.addEventListener('click', (event) => {
        if (!userMenu.contains(event.target)) {
            setUserOpen(false);
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            setUserOpen(false);
        }
    });
}

function registerThemeToggle(root) {
    const savedTheme = localStorage.getItem(STORAGE_KEY);
    if (savedTheme === 'light' || savedTheme === 'dark') {
        root.setAttribute('data-theme', savedTheme);
    }

    const themeToggle = document.querySelector('[data-theme-toggle]');
    if (themeToggle) {
        themeToggle.addEventListener('click', () => {
            const current = root.getAttribute('data-theme') === 'light' ? 'light' : 'dark';
            const next = current === 'light' ? 'dark' : 'light';
            root.setAttribute('data-theme', next);
            localStorage.setItem(STORAGE_KEY, next);
        });
    }
}

function registerLangDropdown() {
    const langDropdown = document.querySelector('[data-lang-dropdown]');
    if (!langDropdown) return;

    const trigger = langDropdown.querySelector('[data-selected-lang]');
    const panel = langDropdown.querySelector('.lang-panel');
    const langOptions = Array.from(panel?.querySelectorAll('[data-lang-option]') ?? []);
    let langActiveIndex = -1;

    const savedLang = localStorage.getItem(LANG_STORAGE_KEY);
    if (trigger) {
        const initialLang = savedLang || 'tr';
        trigger.textContent = initialLang.toUpperCase();
        langOptions.forEach((opt, idx) => {
            opt.dataset.active = opt.getAttribute('data-lang-option') === initialLang ? 'true' : 'false';
            if (!savedLang && idx === 0) {
                opt.dataset.active = 'true';
            }
        });
    }

    const setLang = (value) => {
        if (!trigger) return;
        trigger.textContent = value.toUpperCase();
        localStorage.setItem(LANG_STORAGE_KEY, value);
        langDropdown.setAttribute('data-open', 'false');
        trigger.setAttribute('aria-expanded', 'false');
        langOptions.forEach((opt) => {
            opt.dataset.active = opt.getAttribute('data-lang-option') === value ? 'true' : 'false';
        });
    };

    trigger?.addEventListener('click', () => {
        const isOpen = langDropdown.getAttribute('data-open') === 'true';
        langDropdown.setAttribute('data-open', String(!isOpen));
        trigger.setAttribute('aria-expanded', String(!isOpen));
        if (!isOpen) {
            langActiveIndex = langOptions.findIndex((opt) => opt.dataset.active === 'true');
            if (langActiveIndex < 0) langActiveIndex = 0;
            langOptions[langActiveIndex]?.focus();
        }
    });

    trigger?.addEventListener('keydown', (event) => {
        if (event.key === 'ArrowDown') {
            event.preventDefault();
            langDropdown.setAttribute('data-open', 'true');
            trigger.setAttribute('aria-expanded', 'true');
            langActiveIndex = langOptions.findIndex((opt) => opt.dataset.active === 'true');
            if (langActiveIndex < 0) langActiveIndex = 0;
            langOptions[langActiveIndex]?.focus();
        } else if (event.key === 'Escape') {
            langDropdown.setAttribute('data-open', 'false');
            trigger.setAttribute('aria-expanded', 'false');
        }
    });

    langOptions.forEach((option, index) => {
        option.addEventListener('click', () => setLang(option.getAttribute('data-lang-option')));

        option.addEventListener('keydown', (event) => {
            if (event.key === 'ArrowDown') {
                event.preventDefault();
                langActiveIndex = (index + 1) % langOptions.length;
                langOptions[langActiveIndex].focus();
            } else if (event.key === 'ArrowUp') {
                event.preventDefault();
                langActiveIndex = (index - 1 + langOptions.length) % langOptions.length;
                langOptions[langActiveIndex].focus();
            } else if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                setLang(option.getAttribute('data-lang-option'));
                trigger?.focus();
            } else if (event.key === 'Escape') {
                langDropdown.setAttribute('data-open', 'false');
                trigger?.setAttribute('aria-expanded', 'false');
                trigger?.focus();
            }
        });
    });

    document.addEventListener('click', (event) => {
        if (!langDropdown.contains(event.target)) {
            langDropdown.setAttribute('data-open', 'false');
            trigger?.setAttribute('aria-expanded', 'false');
        }
    });

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape') {
            langDropdown.setAttribute('data-open', 'false');
            trigger?.setAttribute('aria-expanded', 'false');
        }
    });
}

function bootstrapDashboard() {
    renderStatusBar();
    renderKpis();
    renderRiskShipments();
    renderTasks();
    renderActivityFeed();
    updateChartDescriptions();

    shipmentChart = initShipmentStatusChart();
    cashFlowChart = initCashFlowChart();
    supplierChart = initSupplierChart();

    setupFilterBar();
    handleSidebarToggle();

    const parallaxInitializer = initializeParallax();
    window.addEventListener('resize', parallaxInitializer, { passive: true });

    registerUserMenu();
    registerThemeToggle(document.documentElement);
    registerLangDropdown();

    setRange(currentRange);
}

document.addEventListener('DOMContentLoaded', bootstrapDashboard);

function resolveSupplierData() {
    const data = getDashboardData();
    if (data?.suppliers && Array.isArray(data.suppliers.labels) && Array.isArray(data.suppliers.values)) {
        return data.suppliers;
    }
    return fallbackData.suppliers;
}

