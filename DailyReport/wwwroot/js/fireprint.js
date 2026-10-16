// Печать пожарной сводки по частям: «personnel» — только список контактов,
// «table» — только таблица итоговых данных.
//
// Нужная секция прячется INLINE-СТИЛЕМ (el.style.display = 'none'), а не только
// классом на <body>: inline-стиль не зависит от кэша site.css, от специфичности и
// от media-query, поэтому разделение печати работает даже если браузер отдал
// устаревший CSS под прежним ?v=. Класс на <body> и инъектируемый <style>
// оставлены как второй уровень защиты.
(function () {
    var STYLE_ID = 'fire-print-style';
    var hidden = [];

    function ensureStyle() {
        if (document.getElementById(STYLE_ID)) {
            return;
        }
        var style = document.createElement('style');
        style.id = STYLE_ID;
        style.textContent =
            '@media print {' +
            'body.print-personnel .sum-scroll{display:none !important;}' +
            'body.print-table .nurse-shift{display:none !important;}' +
            '.nurse-print-skip{display:none !important;}' +
            '}';
        document.head.appendChild(style);
    }

    function hide(el) {
        if (!el) {
            return;
        }
        hidden.push({ el: el, prev: el.style.display });
        el.style.display = 'none';
    }

    function restore() {
        for (var i = 0; i < hidden.length; i++) {
            hidden[i].el.style.display = hidden[i].prev;
        }
        hidden = [];
    }

    window.firePrint = function (mode) {
        var body = document.body;
        ensureStyle();
        restore();
        body.classList.remove('print-personnel', 'print-table');

        if (mode === 'personnel') {
            body.classList.add('print-personnel');
            hide(document.querySelector('.sum-scroll'));
            document.querySelectorAll('.nurse-print-skip').forEach(function (el) {
                hide(el);
            });
        } else {
            body.classList.add('print-table');
            hide(document.querySelector('.nurse-shift'));
        }

        function cleanup() {
            restore();
            window.removeEventListener('afterprint', cleanup);
        }

        // Класс и inline-стили снимаются по afterprint (в Chrome window.print()
        // блокирующий, поэтому режим держится всё время открытого диалога печати).
        window.addEventListener('afterprint', cleanup);
        window.print();
    };
})();
