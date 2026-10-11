// Ширина селектора дежурной м/с: по самому длинному имени в списке,
// но не шире свободного места в строке служебных полей (.report-meta).
(function () {
    function measureWidth(sel) {
        const probe = document.createElement('select');
        probe.className = sel.className;
        probe.style.cssText = 'position:fixed;left:-10000px;top:0;width:auto;visibility:hidden;';
        const opt = document.createElement('option');
        probe.appendChild(opt);
        document.body.appendChild(probe);

        let needed = 0;
        for (const o of sel.options) {
            opt.textContent = o.textContent;
            needed = Math.max(needed, probe.offsetWidth);
        }
        probe.remove();
        return needed;
    }

    function freeSpace(sel) {
        const container = sel.closest('.report-meta');
        if (!container) return Infinity;

        const cs = getComputedStyle(container);
        const cRect = container.getBoundingClientRect();
        const item = sel.closest('.report-meta-item');
        const itemRect = (item || sel).getBoundingClientRect();

        const contentRight = cRect.right - parseFloat(cs.paddingRight || '0');
        const contentLeft = cRect.left + parseFloat(cs.paddingLeft || '0');
        let free = contentRight - itemRect.left;
        if (free < 0) free = contentRight - contentLeft;
        return free;
    }

    function apply(selectId) {
        const sel = document.getElementById(selectId);
        if (!sel || !sel.options || sel.options.length === 0) return;

        const needed = measureWidth(sel);
        const free = freeSpace(sel);
        sel.style.width = Math.max(0, Math.min(needed, free)) + 'px';
    }

    window.dutyNurseWidth = apply;

    window.addEventListener('resize', function () {
        const sel = document.getElementById('dutyNurse');
        if (!sel) return;
        sel.style.width = '';
        apply('dutyNurse');
    });
})();
