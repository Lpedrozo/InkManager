// Interceptor global para fetch
(function () {
    const originalFetch = window.fetch;

    window.fetch = async function (...args) {
        const token = localStorage.getItem('token');

        // Si hay token y no es una petición a rutas públicas
        if (token && args[1]) {
            args[1].headers = {
                ...args[1].headers,
                'Authorization': `Bearer ${token}`
            };
        } else if (token && !args[1]) {
            args[1] = {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            };
        }

        const response = await originalFetch.apply(this, args);

        // Si la respuesta es 401 (no autorizado), limpiar sesión y redirigir
        if (response.status === 401 && !window.location.pathname.includes('/login')) {
            localStorage.removeItem('token');
            localStorage.removeItem('session');
            window.location.href = '/login';
        }

        return response;
    };
})();