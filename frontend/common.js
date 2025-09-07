const API = "http://localhost:8000";

// token helpers
function getToken() { return localStorage.getItem("lazy_token"); }
function setToken(t) { localStorage.setItem("lazy_token", t); }
function removeToken() { localStorage.removeItem("lazy_token"); }

// Wrapper around fetch() with the Bearer token
async function authFetch(url, options = {}) {
    if (!options.headers) {
        options.headers = {};
    }

    const token = getToken();
    if (token) {
        options.headers['Authorization'] = 'Bearer ' + token;
    }

    const response = await fetch(url, options);
    if (response.status === 401) {
        removeToken();
        location.replace('./login.html');
    }
    return response;
}
