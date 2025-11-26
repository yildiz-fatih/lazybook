import { API, fetchWithAuth, getQueryParam, setupNavigation, formatTime, getToken } from './common.js'
import * as signalR from '@microsoft/signalr'

let otherUsername = null
let currentUsername = null
let connection = null

async function init()
{
    // Setup navigation
    await setupNavigation()

    // Get other user's username from URL
    otherUsername = getQueryParam('username')
    if (!otherUsername)
    {
        alert('No username specified')
        window.location.replace('./index.html')
        return
    }

    // Get current user's username
    const accountResponse = await fetchWithAuth(`${API}/account`)
    if (!accountResponse.ok)
    {
        alert('Failed to load account')
        return
    }
    const accountData = await accountResponse.json()
    currentUsername = accountData.username

    // Set page title and back link
    document.getElementById('other-username').textContent = otherUsername
    document.getElementById('back-link').href = `./profile.html?username=${otherUsername}`

    // Load conversation history
    await loadMessages()

    // Setup SignalR
    await setupSignalR()

    // Setup send button
    document.getElementById('send-btn').addEventListener('click', sendMessage)
    document.getElementById('message-input').addEventListener('keypress', (e) =>
    {
        if (e.key === 'Enter')
        {
            sendMessage()
        }
    })
}

async function loadMessages()
{
    const response = await fetchWithAuth(`${API}/messages/${otherUsername}`)
    const messagesDiv = document.getElementById('messages')

    if (!response.ok)
    {
        messagesDiv.innerHTML = '<p>Failed to load messages</p>'
        return
    }

    const messages = await response.json()

    // Display messages
    displayMessages(messages)
}

function displayMessages(messages)
{
    const messagesDiv = document.getElementById('messages')
    let html = ''

    for (const msg of messages)
    {
        const isMine = msg.senderUsername === currentUsername
        const time = formatTime(msg.createdAt)

        html += `
            <div style="margin-bottom: 10px; text-align: ${isMine ? 'right' : 'left'};">
                <div style="display: inline-block; max-width: 70%; background: ${isMine ? 'var(--pico-primary)' : 'var(--pico-secondary-background)'}; color: ${isMine ? 'var(--pico-primary-inverse)' : 'var(--pico-color)'}; padding: 10px; border-radius: 8px;">
                    <div style="font-weight: bold; font-size: 12px; opacity: 0.9;">${msg.senderUsername}</div>
                    <div style="margin: 4px 0;">${msg.text}</div>
                    <div style="font-size: 10px; opacity: 0.7;">${time}</div>
                </div>
            </div>
        `
    }

    messagesDiv.innerHTML = html

    // Scroll to bottom
    messagesDiv.scrollTop = messagesDiv.scrollHeight
}

async function setupSignalR()
{
    const token = getToken()

    connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5174/chat', {
            accessTokenFactory: () => token
        })
        .build()

    // Listen for incoming messages
    connection.on('ReceiveMessage', (message) =>
    {
        // Only show messages from/to the person we're chatting with
        if (message.senderUsername === otherUsername || message.recipientUsername === otherUsername)
        {
            appendMessage(message)
        }
    })

    // Listen for errors
    connection.on('Error', (error) =>
    {
        alert('Error: ' + error)
    })

    // Start connection
    try
    {
        await connection.start()
    } catch (err)
    {
        alert('Failed to connect to chat')
    }
}

function appendMessage(message)
{
    const messagesDiv = document.getElementById('messages')
    const isMine = message.senderUsername === currentUsername
    const time = formatTime(message.createdAt)

    const messageHtml = `
        <div style="margin-bottom: 10px; text-align: ${isMine ? 'right' : 'left'};">
            <div style="display: inline-block; max-width: 70%; background: ${isMine ? 'var(--pico-primary)' : 'var(--pico-secondary-background)'}; color: ${isMine ? 'var(--pico-primary-inverse)' : 'var(--pico-color)'}; padding: 10px; border-radius: 8px;">
                <div style="font-weight: bold; font-size: 12px; opacity: 0.9;">${message.senderUsername}</div>
                <div style="margin: 4px 0;">${message.text}</div>
                <div style="font-size: 10px; opacity: 0.7;">${time}</div>
            </div>
        </div>
    `

    messagesDiv.innerHTML += messageHtml

    // Scroll to bottom
    messagesDiv.scrollTop = messagesDiv.scrollHeight
}

async function sendMessage()
{
    const input = document.getElementById('message-input')
    const text = input.value.trim()

    if (!text)
    {
        return
    }

    try
    {
        await connection.invoke('SendMessage', {
            recipientUsername: otherUsername,
            text: text
        })

        // Clear input
        input.value = ''
    } catch (err)
    {
        console.error('Failed to send message:', err)
        alert('Failed to send message')
    }
}

init()