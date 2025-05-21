// notification.js - Handles real-time notifications using SignalR

// Initialize the connection to the SignalR hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect()
    .build();

// Start the connection
function startConnection() {
    connection.start()
        .then(() => {
            console.log("Connected to NotificationHub");
        })
        .catch(err => {
            console.error("Error connecting to NotificationHub:", err);
            // Try to reconnect after 5 seconds
            setTimeout(startConnection, 5000);
        });
}

// Handle connection closed event
connection.onclose(async () => {
    console.log("Connection to NotificationHub closed");
    await startConnection();
});

// Handle receiving notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("Received notification:", notification);
    
    // Update the notification badge count
    updateNotificationBadge();
    
    // Show a toast notification
    showToastNotification(notification);
    
    // If the notifications dropdown is open, refresh its content
    if ($("#notificationsDropdown").attr("aria-expanded") === "true") {
        loadNotifications();
    }
});

// Function to show a toast notification
function showToastNotification(notification) {
    // Create a new toast element
    const toastId = `toast-${Date.now()}`;
    const toast = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="5000">
            <div class="toast-header">
                <strong class="me-auto">Notification</strong>
                <small class="text-muted">just now</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                <a href="${notification.link}" class="text-decoration-none text-dark">
                    ${notification.content}
                </a>
            </div>
        </div>
    `;
    
    // Append the toast to the toast container (create if it doesn't exist)
    if ($("#toast-container").length === 0) {
        $("body").append('<div id="toast-container" class="toast-container position-fixed bottom-0 end-0 p-3"></div>');
    }
    
    $("#toast-container").append(toast);
    
    // Show the toast
    const toastElement = new bootstrap.Toast(document.getElementById(toastId));
    toastElement.show();
}

// Function to update the notification badge count
function updateNotificationBadge() {
    $.get('/Notification/GetUnreadCount', function(data) {
        if (data.count > 0) {
            $('.notification-badge').text(data.count).show();
        } else {
            $('.notification-badge').hide();
        }
    });
}

// Function to load notifications into the dropdown
function loadNotifications() {
    $.get('/Notification/GetUserNotifications', function(data) {
        $('#notificationsContainer').html(data);
    });
}

// Initialize when the document is ready
$(document).ready(function() {
    // Start the SignalR connection
    startConnection();
    
    // Load notifications on dropdown open
    $('#notificationsDropdown').on('show.bs.dropdown', function() {
        loadNotifications();
    });
    
    // Check for unread notifications on page load
    updateNotificationBadge();
    
    // Set up periodic checking for new notifications (every 30 seconds)
    setInterval(updateNotificationBadge, 30000);
});

// Handle marking notifications as read
$(document).on('click', '.mark-as-read', function(e) {
    e.preventDefault();
    const notificationId = $(this).data('id');
    
    $.post('/Notification/MarkAsRead', { id: notificationId, __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() }, function(data) {
        if (data.success) {
            // Update the notification badge count
            updateNotificationBadge();
            // Refresh the notifications dropdown content
            loadNotifications();
        }
    });
});

// Handle marking all notifications as read
$(document).on('submit', '#mark-all-read-form', function(e) {
    e.preventDefault();
    
    $.post('/Notification/MarkAllAsRead', $(this).serialize(), function(data) {
        if (data.success) {
            // Update the notification badge count
            updateNotificationBadge();
            // Refresh the notifications dropdown content
            loadNotifications();
        }
    });
}); 