import { getToken, setupNavigation, fetchWithAuth, API, formatTime, getProfilePictureHtml } from './common.js'

function showGuest()
{
    document.getElementById('guest-view').style.display = ''
    document.getElementById('app-view').style.display = 'none'
}

function showApp()
{
    document.getElementById('guest-view').style.display = 'none'
    document.getElementById('app-view').style.display = ''
}

async function loadFeed()
{
    const response = await fetchWithAuth(`${API}/feeds/home`)
    const feed = document.getElementById('feed')

    if (!response.ok)
    {
        feed.innerHTML = '<p>Failed to load feed</p>'
        return
    }

    const posts = await response.json()

    if (posts.length === 0)
    {
        feed.innerHTML = '<p>No posts yet</p>'
        return
    }

    // Build feed HTML
    let html = '<div>'
    for (const post of posts)
    {
        const timestamp = formatTime(post.createdAt)
        html += `
            <div style="margin-bottom: 20px; border-bottom: 1px solid #ccc; padding-bottom: 10px; display: flex; gap: 15px;">
                <div>
                    ${getProfilePictureHtml(post.profilePictureUrl, 50)}
                </div>
                <div style="flex: 1;">
                    <div>
                        <a href="./profile.html?username=${post.username}"><strong>${post.username}</strong></a>
                        <span style="color: #666; font-size: 0.8em;"> (${formatTime(post.createdAt)})</span>
                    </div>
                    <div style="margin-top: 5px;">${post.text}</div>
                </div>
            </div>
        `
    }
    html += '</div>'

    feed.innerHTML = html
}

async function init()
{
    if (!getToken())
    {
        showGuest()
    } else
    {
        showApp()
        // Setup navigation
        await setupNavigation()
        // Load feed
        await loadFeed()
    }
}

init()
