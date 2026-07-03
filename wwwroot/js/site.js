// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    const currentPath = window.location.pathname.toLowerCase();
    const drawerToggle = document.querySelector('[data-erp-drawer-toggle]');
    const drawerCloseButtons = document.querySelectorAll('[data-erp-drawer-close]');
    const sidebar = document.getElementById('erpSidebar');

    const setDrawerState = isOpen => {
        document.body.classList.toggle('sidebar-open', isOpen);
        drawerToggle?.setAttribute('aria-expanded', String(isOpen));
        sidebar?.setAttribute('aria-hidden', String(!isOpen && window.innerWidth < 1200));
        if (!isOpen) drawerToggle?.focus({ preventScroll: true });
    };

    drawerToggle?.addEventListener('click', () => setDrawerState(true));
    drawerCloseButtons.forEach(button => button.addEventListener('click', () => setDrawerState(false)));
    document.addEventListener('keydown', event => {
        if (event.key === 'Escape' && document.body.classList.contains('sidebar-open')) {
            setDrawerState(false);
        }
    });
    window.addEventListener('resize', () => {
        if (window.innerWidth >= 1200) setDrawerState(false);
    });

    sidebar?.querySelectorAll('a[href]:not([href="#"])').forEach(link => {
        link.addEventListener('click', () => {
            if (window.innerWidth < 1200) setDrawerState(false);
        });
    });

    document.querySelectorAll('.erp-sidebar a.nav-link, .erp-sidebar a.dropdown-item').forEach(link => {
        const href = link.getAttribute('href');
        if (!href || href === '#') return;
        const linkPath = new URL(link.href, window.location.origin).pathname.toLowerCase();
        const isActive = linkPath === '/' ? currentPath === '/' : currentPath === linkPath || currentPath.startsWith(`${linkPath}/`);
        if (isActive) {
            link.classList.add('active');
            link.closest('.dropdown')?.querySelector(':scope > .nav-link')?.classList.add('active');
        }
    });

    const search = document.getElementById('erpGlobalSearch');
    search?.addEventListener('input', event => {
        const query = event.target.value.trim().toLocaleLowerCase('tr-TR');
        document.querySelectorAll('main table tbody tr').forEach(row => {
            row.classList.toggle('d-none', query.length > 0 && !row.textContent.toLocaleLowerCase('tr-TR').includes(query));
        });
    });

    document.addEventListener('keydown', event => {
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k' && search) {
            event.preventDefault();
            search.focus();
        }
    });

    document.querySelectorAll('main table tbody').forEach(body => {
        if (body.rows.length !== 0 || body.querySelector('.erp-empty-state')) return;
        const columns = body.closest('table')?.querySelectorAll('thead th').length || 1;
        body.insertAdjacentHTML('beforeend', `<tr class="erp-empty-state"><td colspan="${columns}"><i class="bi bi-inbox"></i><strong>Kayıt bulunamadı</strong><span>Bu görünüm için henüz gösterilecek veri yok.</span></td></tr>`);
    });

    document.querySelectorAll('main table').forEach(table => {
        if (table.closest('.table-responsive') || table.closest('.invoice-container')) return;
        const wrapper = document.createElement('div');
        wrapper.className = 'table-responsive';
        table.parentNode.insertBefore(wrapper, table);
        wrapper.appendChild(table);
    });

    document.querySelectorAll('main h3, main h4, main h5').forEach(heading => {
        if (heading.closest('.summary-card, .dashboard-card, .finance-card, .warehouse-card, .accounting-card, .erp-kpi-card, .modal, .alert, .accordion-header, .invoice-container')) return;
        const isSectionHeading = heading.parentElement === document.querySelector('main')
            || heading.parentElement?.classList.contains('admin-box')
            || heading.nextElementSibling?.matches('.table-responsive, .custom-table, .card, form');
        if (isSectionHeading) heading.classList.add('erp-section-heading');
    });

    document.querySelectorAll('.summary-card, .finance-card, .dashboard-card, .warehouse-card, .accounting-card, .erp-kpi-card')
        .forEach(card => {
            card.classList.add('erp-kpi-surface');
            card.querySelector('h2, h3, .value, strong')?.classList.add('erp-kpi-value');
            card.querySelector('h5, h6, small, .card-body > div:first-child')?.classList.add('erp-kpi-title');
        });

    document.querySelectorAll('.card-header').forEach(header => {
        header.classList.add('erp-card-header');
        const title = header.querySelector('strong, .fw-semibold, h3, h4, h5');
        title?.classList.add('erp-card-title');
        header.querySelector('small, .text-muted')?.classList.add('erp-card-subtitle');
        if (header.closest('.card')?.querySelector('table')) header.classList.add('erp-table-title');
    });

    document.querySelectorAll('.card').forEach(card => {
        if (card.querySelector('canvas, .chart-wrap, .finance-chart')) card.classList.add('erp-chart-card');
    });

    const createChartEmptyState = (container, variant = 'axis') => {
        if (container.querySelector('.erp-chart-empty')) return;
        const empty = document.createElement('div');
        empty.className = `erp-chart-empty erp-chart-empty-${variant}`;
        empty.innerHTML = '<i class="bi bi-bar-chart-line" aria-hidden="true"></i><strong>Henüz gösterilecek veri bulunmuyor</strong><span>Veri oluştuğunda grafik bu alanda otomatik olarak görüntülenecektir.</span>';
        container.appendChild(empty);
    };

    document.querySelectorAll('.chart-wrap, .finance-chart, .erp-chart-card .card-body').forEach(container => {
        const canvas = container.querySelector('canvas');

        if (!canvas) {
            container.replaceChildren();
            createChartEmptyState(container, 'axis');
            return;
        }

        const chart = window.Chart?.getChart?.(canvas);
        if (!chart) return;

        const values = chart.data.datasets
            .flatMap(dataset => Array.isArray(dataset.data) ? dataset.data : [])
            .map(value => Number(value));
        const hasData = values.some(value => Number.isFinite(value) && value !== 0);
        if (hasData) return;

        const chartType = chart.config.type;
        const isCircular = chartType === 'doughnut' || chartType === 'pie' || chartType === 'polarArea';
        container.classList.add('erp-chart-empty-container');
        if (isCircular) canvas.classList.add('d-none');
        else canvas.classList.add('erp-chart-skeleton-canvas');
        createChartEmptyState(container, isCircular ? 'donut' : 'overlay');
    });

    document.querySelectorAll('.erp-chart-card > canvas').forEach(canvas => {
        const container = canvas.parentElement;
        const chart = window.Chart?.getChart?.(canvas);
        if (!container || !chart) return;

        const values = chart.data.datasets
            .flatMap(dataset => Array.isArray(dataset.data) ? dataset.data : [])
            .map(value => Number(value));
        if (values.some(value => Number.isFinite(value) && value !== 0)) return;

        const chartType = chart.config.type;
        const isCircular = chartType === 'doughnut' || chartType === 'pie' || chartType === 'polarArea';
        container.classList.add('erp-chart-empty-container');
        if (isCircular) canvas.classList.add('d-none');
        else canvas.classList.add('erp-chart-skeleton-canvas');
        createChartEmptyState(container, isCircular ? 'donut' : 'overlay');
    });

    document.querySelectorAll('form').forEach(form => {
        form.addEventListener('submit', () => {
            if (!form.checkValidity()) return;
            const button = form.querySelector('button[type="submit"], button:not([type])');
            if (!button || button.classList.contains('js-confirm')) return;
            button.classList.add('erp-loading');
            button.setAttribute('aria-busy', 'true');
        });
    });

    document.querySelectorAll('.btn').forEach(button => {
        if (button.querySelector('i, svg')) return;
        const text = button.textContent.trim().toLocaleLowerCase('tr-TR');
        let icon = 'arrow-right';
        if (/ekle|oluştur|tanımla|yeni|kaydet|ata|stok girişi/.test(text)) icon = text.includes('kaydet') ? 'check2' : 'plus-lg';
        else if (/aktifleş/.test(text)) icon = 'play-circle';
        else if (/devre dışı|pasifleştir/.test(text)) icon = 'pause-circle';
        else if (/kaldır|iptal/.test(text)) icon = 'x-circle';
        else if (/sil/.test(text)) icon = 'trash3';
        else if (/düzenle|güncelle/.test(text)) icon = 'pencil';
        else if (/görüntüle|detay|aç/.test(text)) icon = 'eye';
        else if (/indir|excel|csv|word/.test(text)) icon = 'download';
        else if (/ara|filtre|göster/.test(text)) icon = 'search';
        else if (/geri|dön/.test(text)) icon = 'arrow-left';
        else if (/onay/.test(text)) icon = 'check2-circle';
        else if (/reddet/.test(text)) icon = 'x-circle';
        button.insertAdjacentHTML('afterbegin', `<i class="bi bi-${icon}" aria-hidden="true"></i>`);

        const actionCell = button.closest('table td:last-child');
        if (actionCell && button.classList.contains('btn-sm')) {
            button.setAttribute('aria-label', button.textContent.trim());
            button.setAttribute('title', button.textContent.trim());
            if (button.classList.contains('erp-icon-only')) {
                button.classList.add('erp-icon-action');
                Array.from(button.childNodes)
                    .filter(node => node.nodeType === Node.TEXT_NODE)
                    .forEach(node => node.remove());
            }
        }
    });
});
