import { API, fetchWithAuth, getQueryParam, setupNavigation, formatTime, getProfilePictureHtml } from './common.js'

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

    // Load posts
    await loadPosts()

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

async function loadPosts()
{
    const response = await fetchWithAuth(`${API}/profiles/${currentProfileUsername}/posts`)
    const postsList = document.getElementById('posts-list')

    if (!response.ok)
    {
        postsList.innerHTML = '<p>Failed to load posts</p>'
        return
    }

    const posts = await response.json()

    if (posts.length === 0)
    {
        postsList.innerHTML = '<p>No posts yet</p>'
        return
    }

    // Build posts HTML
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

    postsList.innerHTML = html
}

function renderProfile()
{
    // Set profile picture
    document.getElementById('profile-picture').innerHTML = getProfilePictureHtml(profileData.profilePictureUrl, 80)

    // Set username
    document.getElementById('profile-username').textContent = profileData.username

    // Set follower/following counts
    document.getElementById('follower-count').textContent = profileData.followerCount
    document.getElementById('following-count').textContent = profileData.followingCount

    // Set follower/following links
    document.getElementById('followers-link').href = `./followers.html?username=${profileData.username}`
    document.getElementById('following-link').href = `./following.html?username=${profileData.username}`

    // Get elements for status editing
    const statusText = document.getElementById('status-text')
    const statusForm = document.getElementById('status-form')
    const statusInput = document.getElementById('status-input')

    // Show/hide sections based on isSelf
    if (profileData.isSelf)
    {
        // Show picture upload
        document.getElementById('picture-upload-section').style.display = 'block';

        // Show create post
        document.getElementById('create-post').style.display = ''

        // Show status form, hide the text
        statusForm.style.display = 'block'
        statusInput.value = profileData.status || '(no status)'
        statusText.style.display = 'none'

        // Hide follow section
        document.getElementById('follow-section').style.display = 'none'
    } else
    {
        // Hide picture upload
        document.getElementById('picture-upload-section').style.display = 'none';

        // Hide create post
        document.getElementById('create-post').style.display = 'none'

        // Hide status form, show the text
        statusForm.style.display = 'none'
        statusText.style.display = 'block'
        statusText.textContent = profileData.status || '(no status)'

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

        // Set message link
        document.getElementById('message-link').href = `./chat.html?username=${profileData.username}`
    }
}

function setupEventListeners()
{
    // Post button
    const postBtn = document.getElementById('post-btn')
    if (postBtn)
    {
        postBtn.addEventListener('click', handleCreatePost)
    }

    // Status save button
    const statusForm = document.getElementById('status-form')
    if (statusForm)
    {
        statusForm.addEventListener('submit', async (e) =>
        {
            e.preventDefault()
            const input = document.getElementById('status-input')

            const response = await fetchWithAuth(`${API}/account`, {
                method: 'PUT',
                body: JSON.stringify({ status: input.value })
            })

            if (response.ok)
            {
                profileData.status = input.value
                alert('Status updated')
            } else
            {
                alert('Failed to save status')
            }
        })
    }

    // Follow/unfollow button
    const followBtn = document.getElementById('follow-btn')
    if (followBtn)
    {
        followBtn.addEventListener('click', handleFollowToggle)
    }

    // Picture Upload Logic
    const pictureBtn = document.getElementById('picture-btn')
    const pictureInput = document.getElementById('picture-input')

    if (pictureBtn && pictureInput)
    {
        // Trigger file input on button click
        pictureBtn.addEventListener('click', () => pictureInput.click())

        // Handle file selection
        pictureInput.addEventListener('change', async (e) =>
        {
            const file = e.target.files[0]
            if (!file) return

            const formData = new FormData()
            formData.append('Image', file)

            pictureBtn.disabled = true

            try
            {
                const response = await fetchWithAuth(`${API}/account/picture`, {
                    method: 'POST',
                    body: formData
                })

                if (response.ok)
                {
                    await loadProfile()
                } else
                {
                    alert('Upload failed');
                }
            } catch (err)
            {
                console.error(err)
                alert('Error uploading image')
            }
        })
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

async function handleCreatePost()
{
    const postInput = document.getElementById('post-input')
    const text = postInput.value.trim()

    if (!text)
    {
        alert('Post cannot be empty')
        return
    }

    const response = await fetchWithAuth(`${API}/posts`, {
        method: 'POST',
        body: JSON.stringify({ text: text })
    })

    if (!response.ok)
    {
        alert('Failed to create post')
        return
    }

    // Clear input
    postInput.value = ''

    // Reload posts to show the new one
    await loadPosts()
}

init()
