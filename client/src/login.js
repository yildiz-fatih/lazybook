import { API, setToken } from './common.js'

document.getElementById('login-form').addEventListener('submit', async (e) =>
{
    e.preventDefault()

    const username = document.getElementById('username-input').value
    const password = document.getElementById('password-input').value
    const response = await fetch(`${API}/auth/login`, {
        method: 'POST',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify({ username, password })
    })

    if (!response.ok)
    {
        alert('login failed')
        return
    }
    else
    {
        const { access_token } = await response.json()
        setToken(access_token)
        window.location.replace(`./index.html`)
    }
})
