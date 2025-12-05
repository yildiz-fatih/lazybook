export const BASE_URL = "http://localhost:5174";
export const API = "http://localhost:5174/api";

// Token helpers
export function getToken() { return localStorage.getItem("access_token"); }
export function setToken(t) { localStorage.setItem("access_token", t); }
export function removeToken() { localStorage.removeItem("access_token"); }

// Get URL query parameter
export function getQueryParam(name)
{
    const params = new URLSearchParams(window.location.search)
    return params.get(name)
}

// Format ISO time string to "HH:MM"
export function formatTime(isoString)
{
    const date = new Date(isoString)
    return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })
}

// Fetch with auth header
export async function fetchWithAuth(url, options = {})
{
    const token = getToken()
    if (!token)
    {
        window.location.replace('./login.html')
        return
    }

    const headers = {
        'Authorization': `Bearer ${token}`,
        ...(options.body instanceof FormData ? {} : { 'Content-Type': 'application/json' }),
        ...options.headers
    }

    const response = await fetch(url, { ...options, headers })

    if (response.status === 401)
    {
        removeToken()
        window.location.replace('./login.html')
    }

    return response
}

// Setup navigation bar (called from each page's JS)
export async function setupNavigation()
{
    const response = await fetchWithAuth(`${API}/account`)
    if (!response.ok) return

    const { username } = await response.json()

    // Update profile link
    const profileLink = document.getElementById('profile-link')
    if (profileLink)
    {
        profileLink.href = `./profile.html?username=${username}`
    }

    // Setup logout
    const logoutBtn = document.getElementById('logout-btn')
    if (logoutBtn)
    {
        logoutBtn.addEventListener('click', () =>
        {
            removeToken()
            window.location.replace('./index.html')
        })
    }
}

export function getProfilePictureHtml(profilePictureUrl, size = 40)
{
    // If we have a URL, show the image
    if (profilePictureUrl)
    {
        return `<img src="${BASE_URL}${profilePictureUrl}" class="profile-picture" style="width: ${size}px; height: ${size}px;">`
    }
    // Otherwise, show a placeholder
    else
    {
        return `<div class="profile-picture placeholder" style="width: ${size}px; height: ${size}px;"></div>`
    }
}
