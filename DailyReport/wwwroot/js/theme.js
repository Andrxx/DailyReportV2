window.theme = (function () {
    const KEY = 'dr-theme';

    function current() {
        return document.documentElement.classList.contains('dark');
    }

    function set(dark) {
        document.documentElement.classList.toggle('dark', dark);
        try { localStorage.setItem(KEY, dark ? 'dark' : 'light'); } catch (e) { }
    }

    function applySaved() {
        let dark;
        try {
            const saved = localStorage.getItem(KEY);
            dark = saved ? saved === 'dark' : window.matchMedia('(prefers-color-scheme: dark)').matches;
        } catch (e) {
            dark = false;
        }
        set(dark);
    }

    applySaved();

    return { current, set };
})();