import { API, fetchWithAuth, getQueryParam, setupNavigation, getProfilePictureHtml } from './common.js'

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
        html += `
            <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 10px; padding: 5px; border-bottom: 1px solid #eee;">
                ${getProfilePictureHtml(user.profilePictureUrl, 40)}
                <a href="./profile.html?username=${user.username}"><strong>${user.username}</strong></a>
            </div>
        `
    }
    html += '</div>'

    listDiv.innerHTML = html
}

init()
