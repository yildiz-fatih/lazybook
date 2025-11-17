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
        html += `<div><a href="./profile.html?username=${follower.username}">${follower.username}</a></div>`
    }
    html += '</div>'

    listDiv.innerHTML = html
}

init()
