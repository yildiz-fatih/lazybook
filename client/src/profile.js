import { API, fetchWithAuth, getQueryParam, setupNavigation } from './common.js'

let currentProfileUsername = null
let profileData = null

async function init()
{
    // Setup navigation
    await setupNavigation()

    // Get username from URL
    currentProfileUsername = getQueryParam('username')
    if (!currentProfileUsername)
    {
        alert('No username specified')
        window.location.replace('./index.html')
        return
    }

    // Load profile
    await loadProfile()

    // Setup event listeners
    setupEventListeners()
}

async function loadProfile()
{
    const response = await fetchWithAuth(`${API}/profiles/${currentProfileUsername}`)

    if (!response.ok)
    {
        alert('Failed to load profile')
        window.location.replace('./index.html')
        return
    }

    profileData = await response.json()
    renderProfile()
}

function renderProfile()
{
    // Set username
    document.getElementById('profile-username').textContent = profileData.username

    // Set status
    document.getElementById('status-display').textContent = profileData.status || '(no status)'

    // Set follower/following counts
    document.getElementById('follower-count').textContent = profileData.followerCount
    document.getElementById('following-count').textContent = profileData.followingCount

    // Set follower/following links
    document.getElementById('followers-link').href = `./followers.html?username=${profileData.username}`
    document.getElementById('following-link').href = `./following.html?username=${profileData.username}`

    // Show/hide sections based on isSelf
    if (profileData.isSelf)
    {
        // Show status edit
        document.getElementById('status-edit').style.display = ''
        document.getElementById('status-input').value = profileData.status

        // Hide follow section
        document.getElementById('follow-section').style.display = 'none'
    } else
    {
        // Hide status edit
        document.getElementById('status-edit').style.display = 'none'

        // Show follow section
        document.getElementById('follow-section').style.display = ''

        // Set follow button text
        const followBtn = document.getElementById('follow-btn')
        followBtn.textContent = profileData.iFollow ? 'unfollow' : 'follow'

        // Show "follows you back" indicator
        const followsMe = document.getElementById('follows-me-indicator')
        if (profileData.followsMe)
        {
            followsMe.textContent = '(follows you back)'
        } else
        {
            followsMe.textContent = ''
        }
    }
}

function setupEventListeners()
{
    // Status save button
    const statusSaveBtn = document.getElementById('status-save-btn')
    if (statusSaveBtn)
    {
        statusSaveBtn.addEventListener('click', handleStatusSave)
    }

    // Follow/unfollow button
    const followBtn = document.getElementById('follow-btn')
    if (followBtn)
    {
        followBtn.addEventListener('click', handleFollowToggle)
    }
}

async function handleStatusSave()
{
    const newStatus = document.getElementById('status-input').value

    const response = await fetchWithAuth(`${API}/account`, {
        method: 'PUT',
        body: JSON.stringify({ status: newStatus })
    })

    if (!response.ok)
    {
        alert('Failed to update status')
        return
    }

    // Update display
    profileData.status = newStatus
    document.getElementById('status-display').textContent = newStatus || '(no status)'
    alert('Status updated!')
}

async function handleFollowToggle()
{
    const isFollowing = profileData.iFollow
    const method = isFollowing ? 'DELETE' : 'POST'

    const response = await fetchWithAuth(`${API}/profiles/${currentProfileUsername}/follow`, {
        method: method
    })

    if (!response.ok)
    {
        alert('Failed to update follow status')
        return
    }

    // Reload profile to get updated counts and status
    await loadProfile()
}

init()
