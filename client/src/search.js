import { API, fetchWithAuth, setupNavigation } from './common.js'

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
        html += `<div><a href="./profile.html?username=${user.username}">${user.username}</a></div>`
    }
    html += '</div>'

    resultsDiv.innerHTML = html
}

init()
