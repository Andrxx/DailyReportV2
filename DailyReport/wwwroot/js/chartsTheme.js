window.chartsTheme = (function () {
    let observer = null;
    let dotNetRef = null;

    function notify() {
        if (!dotNetRef) return;
        dotNetRef.invokeMethodAsync('OnThemeChanged', document.documentElement.classList.contains('dark'));
    }

    function watch(ref) {
        dotNetRef = ref;
        if (observer) observer.disconnect();
        observer = new MutationObserver(notify);
        observer.observe(document.documentElement, { attributes: true, attributeFilter: ['class'] });
    }

    function dispose() {
        if (observer) {
            observer.disconnect();
            observer = null;
        }
        dotNetRef = null;
    }

    return {
        watch,
        dispose,
        current: () => document.documentElement.classList.contains('dark')
    };
})();
