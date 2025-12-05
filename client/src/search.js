import { API, fetchWithAuth, setupNavigation, getProfilePictureHtml } from './common.js'

async function init()
{
    // Setup navigation
    await setupNavigation()

    // Setup search input listener
    const searchInput = document.getElementById('search-input')
    searchInput.addEventListener('input', handleSearch)
}

async function handleSearch(event)
{
    const query = event.target.value.trim()
    const resultsDiv = document.getElementById('search-results')

    if (!query)
    {
        resultsDiv.innerHTML = ''
        return
    }

    // Show loading
    resultsDiv.innerHTML = '<p>searching...</p>'

    // Fetch search results
    const response = await fetchWithAuth(`${API}/profiles/search?username=${encodeURIComponent(query)}`)

    if (!response.ok)
    {
        resultsDiv.innerHTML = '<p>Search failed</p>'
        return
    }

    const users = await response.json()

    // Display results
    if (users.length === 0)
    {
        resultsDiv.innerHTML = '<p>No users found</p>'
        return
    }

    // Build results HTML
    let html = '<div>'
    for (const user of users)
    {
        html += `
            <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 10px; padding: 5px; border-bottom: 1px solid #eee;">
                ${getProfilePictureHtml(user.profilePictureUrl, 40)}
                <a href="./profile.html?username=${user.username}"><strong>${user.username}</strong></a>
            </div>
        `
    }
    html += '</div>'

    resultsDiv.innerHTML = html
}

init()
