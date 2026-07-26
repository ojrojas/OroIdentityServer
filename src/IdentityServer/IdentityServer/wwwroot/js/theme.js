// Applied synchronously in <head> so the stored theme paints on the first frame.
(function () {
    try {
        var stored = localStorage.getItem('theme');
        if (stored === 'dark' || stored === 'light') {
            document.documentElement.setAttribute('data-theme', stored);
        }
    } catch (e) { /* private mode - fall back to the OS preference */ }
})();

window.oroTheme = {
    current: function () {
        var explicit = document.documentElement.getAttribute('data-theme');
        if (explicit) return explicit;
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    },

    set: function (theme) {
        document.documentElement.setAttribute('data-theme', theme);
        try { localStorage.setItem('theme', theme); } catch (e) { /* ignore */ }
        return theme;
    },

    toggle: function () {
        return window.oroTheme.set(window.oroTheme.current() === 'dark' ? 'light' : 'dark');
    }
};
