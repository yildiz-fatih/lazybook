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

// Pretty-print a single post
function renderPost(post) {
    const postContainer = document.createElement('div');

    const postHeader = document.createElement('div');

    const usernameLink = document.createElement('a');
    usernameLink.href = `./profile.html?user_id=${post.user_id}`;
    usernameLink.textContent = post.username;
    postHeader.appendChild(usernameLink);

    const timestampElement = document.createElement('small');
    const formattedDate = new Date(post.created_at).toLocaleString();
    timestampElement.textContent = ` ${formattedDate}`;
    postHeader.appendChild(timestampElement);

    const postContent = document.createElement('p');
    postContent.style.wordBreak = 'break-word';
    postContent.textContent = post.contents || '';
    postHeader.appendChild(postContent);

    postContainer.appendChild(postHeader);

    const separator = document.createElement('hr');
    postContainer.appendChild(separator);

    return postContainer;
}
