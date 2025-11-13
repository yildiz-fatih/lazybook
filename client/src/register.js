import { API } from './common.js'

document.getElementById('register-form').addEventListener('submit', async (e) =>
{
    e.preventDefault()

    const username = document.getElementById('username-input').value.trim()
    const password = document.getElementById('password-input').value
    const confirm = document.getElementById('confirm-input').value

    if (password !== confirm)
    {
        alert("passwords do not match")
        return
    }

    const response = await fetch(`${API}/auth/register`, {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify({ username: username, password: password })
    })

    if (!response.ok)
    {
        alert('registration failed')
    }
    else
    {
        alert('registered')
        window.location.replace(`./login.html`)
        return
    }
})
