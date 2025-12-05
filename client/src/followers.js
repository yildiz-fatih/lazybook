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

    // Load followers
    await loadFollowers(username)
}

async function loadFollowers(username)
{
    const response = await fetchWithAuth(`${API}/profiles/${username}/followers`)

    const listDiv = document.getElementById('followers-list')

    if (!response.ok)
    {
        listDiv.innerHTML = '<p>Failed to load followers</p>'
        return
    }

    const followers = await response.json()

    if (followers.length === 0)
    {
        listDiv.innerHTML = '<p>No followers yet</p>'
        return
    }

    // Build followers list
    let html = '<div>'
    for (const follower of followers)
    {
        html += `
            <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 10px; padding: 5px; border-bottom: 1px solid #eee;">
                ${getProfilePictureHtml(follower.profilePictureUrl, 40)}
                <a href="./profile.html?username=${follower.username}"><strong>${follower.username}</strong></a>
            </div>
        `
    }
    html += '</div>'

    listDiv.innerHTML = html
}

init()
