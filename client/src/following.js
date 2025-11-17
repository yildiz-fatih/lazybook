import { API, fetchWithAuth, getQueryParam, setupNavigation } from './common.js'

async function init()
{
    // Setup navigation
    await setupNavigation()

    // Get username from URL
    const username = getQueryParam('username')
    if (!username)
    {
        alert('No username specified')
        window.location.replace('./index.html')
        return
    }

    // Set username in title
    document.getElementById('username-display').textContent = username

    // Set back link
    document.getElementById('back-link').href = `./profile.html?username=${username}`
    // Load following
    await loadFollowing(username)
}

async function loadFollowing(username)
{
    const response = await fetchWithAuth(`${API}/profiles/${username}/following`)

    const listDiv = document.getElementById('following-list')

    if (!response.ok)
    {
        listDiv.innerHTML = '<p>Failed to load following</p>'
        return
    }

    const following = await response.json()

    if (following.length === 0)
    {
        listDiv.innerHTML = '<p>Not following anyone yet</p>'
        return
    }

    // Build following list
    let html = '<div>'
    for (const user of following)
    {
        html += `<div><a href="./profile.html?username=${user.username}">${user.username}</a></div>`
    }
    html += '</div>'

    listDiv.innerHTML = html
}

init()
