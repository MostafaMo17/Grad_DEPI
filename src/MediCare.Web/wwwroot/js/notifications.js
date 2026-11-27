// The bell counter, the toast and the live list.
// This file is loaded only for logged-in users (see _Layout.cshtml). It needs the SignalR client script.
//
// SECURITY: the browser never gets the user's API token.
// To connect to the hub it asks THIS website (/Notifications/HubToken) for a short 5-minute token.
(function () {
    "use strict";

    const bell = document.getElementById("mc-notification-bell");
    if (!bell) {
        return; // not logged in
    }

    const badge = document.getElementById("mc-notification-count");
    const toastContainer = document.getElementById("mc-toast-container");
    let unreadCount = 0;

    function goToLogin() {
        window.location.href = "/Account/Login?expired=true";
    }

    // The header tells the server this is a JavaScript request: it answers 401 with a status code instead of a redirect page.
    async function getJson(url) {
        const response = await fetch(url, { headers: { "X-Requested-With": "XMLHttpRequest" } });

        if (response.status === 401) {
            goToLogin();
            return null;
        }

        if (!response.ok) {
            return null;
        }

        return await response.json();
    }

    //  The bell 

    function showCount() {
        badge.textContent = unreadCount;
        badge.hidden = unreadCount <= 0;
    }

    async function loadUnreadCount() {
        const data = await getJson("/Notifications/UnreadCount");
        if (data) {
            unreadCount = data.count;
            showCount();
        }
    }

    //  Showing a new notification 

    function formatTime(utcText) {
        return new Date(utcText).toLocaleString("en-GB", { timeZone: "Africa/Cairo" });
    }

    function showToast(notification) {
        if (!toastContainer) {
            return;
        }

        const toast = document.createElement("div");
        toast.className = "mc-toast";

        const title = document.createElement("strong");
        title.textContent = notification.title;

        const message = document.createElement("span");
        message.textContent = " " + notification.message;

        toast.appendChild(title);
        toast.appendChild(message);
        toastContainer.appendChild(toast);

        setTimeout(function () { toast.remove(); }, 6000);
    }

    // If the notifications page is open, add the new notification at the top of the list.
    // (textContent is used so the text can never be treated as HTML.)
    function addToList(notification) {
        const list = document.getElementById("mc-notification-list");
        if (!list) {
            return;
        }

        const empty = document.getElementById("mc-notification-empty");
        if (empty) {
            empty.remove();
        }

        const item = document.createElement("li");
        item.className = "mc-notification mc-notification--unread";
        item.dataset.id = notification.notificationId;

        const title = document.createElement("strong");
        title.className = "mc-notification-title";
        title.textContent = notification.title;

        const message = document.createElement("span");
        message.className = "mc-notification-message";
        message.textContent = notification.message;

        const time = document.createElement("time");
        time.className = "mc-notification-time";
        time.dateTime = notification.createdAtUtc;
        time.textContent = formatTime(notification.createdAtUtc);

        item.appendChild(title);
        item.appendChild(message);
        item.appendChild(time);
        list.prepend(item);
    }

    //  The live connection 

    async function connectWithRetry(connection) {
        try {
            await connection.start();
            console.info("Notifications: connected.");
        } catch (error) {
            // The API may be starting or down: try again in 5 seconds.
            console.warn("Notifications: could not connect, retrying in 5 seconds.");
            setTimeout(function () { connectWithRetry(connection); }, 5000);
        }
    }

    function startConnection() {
        if (typeof signalR === "undefined") {
            console.warn("Notifications: the SignalR script did not load.");
            return;
        }

        const connection = new signalR.HubConnectionBuilder()
            .withUrl(document.body.dataset.hubUrl, {
                // Called on every connect and every reconnect, so an expired token is never reused.
                accessTokenFactory: async function () {
                    const data = await getJson("/Notifications/HubToken");
                    return data ? data.token : "";
                }
            })
            .withAutomaticReconnect()
            .build();

        // The server calls this method when a notification arrives for this user.
        connection.on("ReceiveNotification", function (notification) {
            unreadCount += 1;
            showCount();
            showToast(notification);
            addToList(notification);
        });

        connection.onreconnecting(function () {
            console.info("Notifications: connection lost, reconnecting...");
        });

        connection.onreconnected(function () {
            console.info("Notifications: reconnected.");
            loadUnreadCount(); // we may have missed notifications while disconnected
        });

        connectWithRetry(connection);
    }

    loadUnreadCount();
    startConnection();
})();