const cookieUtils = {
    set: (name, obj, days = 7) => {
        const expires = new Date();
        expires.setTime(expires.getTime() + days * 24 * 60 * 60 * 1000);

        const value = encodeURIComponent(JSON.stringify(obj));

        document.cookie = `${name}=${value}; expires=${expires.toUTCString()}; path=/`;
    },

    get: (name) => {
        const cookies = document.cookie.split(';');
        for (let c of cookies) {
            const [cookieName, cookieValue] = c.trim().split('=');
            if (cookieName === name) {
                try {
                    return JSON.parse(decodeURIComponent(cookieValue));
                } catch (e) {
                    console.warn("Cannot get cookie:", e);
                    return null;
                }
            }
        }
        return null;
    }

}

export default cookieUtils;