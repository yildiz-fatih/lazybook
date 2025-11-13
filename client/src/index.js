import { getToken, removeToken } from './common.js'

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

document.getElementById('logout-button').addEventListener('click', () =>
{
    removeToken()
    showGuest()
})

function init()
{
    if (!getToken())
    {
        showGuest()
    } else
    {
        showApp()
    }
}

init()
