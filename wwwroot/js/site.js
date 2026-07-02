// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener('DOMContentLoaded', () => {
    const currentPath = window.location.pathname.toLowerCase();

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
        if (/ekle|oluştur|tanımla|yeni|kaydet|ata/.test(text)) icon = text.includes('kaydet') ? 'check2' : 'plus-lg';
        else if (/sil|pasifleştir|iptal/.test(text)) icon = 'trash3';
        else if (/düzenle|güncelle/.test(text)) icon = 'pencil';
        else if (/görüntüle|detay|aç/.test(text)) icon = 'eye';
        else if (/indir|excel|csv|word/.test(text)) icon = 'download';
        else if (/ara|filtre|göster/.test(text)) icon = 'search';
        else if (/geri|dön/.test(text)) icon = 'arrow-left';
        else if (/onay/.test(text)) icon = 'check2-circle';
        else if (/reddet/.test(text)) icon = 'x-circle';
        button.insertAdjacentHTML('afterbegin', `<i class="bi bi-${icon}" aria-hidden="true"></i>`);
    });
});
