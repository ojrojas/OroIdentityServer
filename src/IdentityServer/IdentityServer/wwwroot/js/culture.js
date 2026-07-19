window.blazorCulture = {
    get: function () {
        var match = document.cookie.match(/(?:^|; )\.AspNetCore\.Culture=([^;]*)/);
        if (!match) return null;

        var value = decodeURIComponent(match[1]);
        var cultureMatch = value.match(/c=([^|]+)/);
        return cultureMatch ? cultureMatch[1] : null;
    }
};
