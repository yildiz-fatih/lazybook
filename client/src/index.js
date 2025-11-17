import { getToken, setupNavigation } from './common.js'

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
    }
}

init()
