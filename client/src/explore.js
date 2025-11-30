import { API, fetchWithAuth, setupNavigation, formatTime } from './common.js'

async function init()
{
    // Setup navigation
    await setupNavigation()

    // Load explore feed
    await loadExploreFeed()
}

async function loadExploreFeed()
{
    const feedDiv = document.getElementById('explore-feed')

    const response = await fetchWithAuth(`${API}/feeds/explore`)

    if (!response.ok)
    {
        feedDiv.innerHTML = '<p>Failed to load explore feed</p>'
        return
    }

    const posts = await response.json()

    if (posts.length === 0)
    {
        feedDiv.innerHTML = '<p>No posts found</p>'
        return
    }

    // Build feed HTML
    let html = '<div>'
    for (const post of posts)
    {
        const timestamp = formatTime(post.createdAt)
        html += `
            <div style="margin-bottom: 20px; border-bottom: 1px solid #ccc; padding-bottom: 10px;">
                <div>
                    <a href="./profile.html?username=${post.username}"><strong>${post.username}</strong></a>
                    <span style="color: #666;"> (${timestamp})</span>
                </div>
                <div>${post.text}</div>
            </div>
        `
    }
    html += '</div>'

    feedDiv.innerHTML = html
}

init()