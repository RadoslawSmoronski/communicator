async function login() {
    const username = document.getElementById("username").value;
    const password = document.getElementById("password").value;
    let connection;
    const response = await fetch("https://localhost:7003/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password })
    });

    if (response.ok) {
        const data = await response.json();
        localStorage.setItem("jwt", data.token);  // Store JWT in local storage
        alert("Login successful!");
        window.location.href = "/Home/Chat"; // Redirect to the chat page
    } else {
        alert("Login failed. Please check your credentials.");
    }
}
async function sendMessage() {
    const message = document.getElementById("messageInput").value;
    const user = "You"; // Replace with actual username if available

    try {
        await connection.invoke("SendMessage", user, message);
        document.getElementById("messageInput").value = ""; // Clear the input after sending
    } catch (err) {
        console.error("Error sending message:", err);
    }
}

//document.getElementById("sendButton").addEventListener("click", sendMessage);

async function startConnection() {
    const token = localStorage.getItem("jwt");

    if (!token || token.split('.').length !== 3) {
        alert("Token not Found!");
        return;
    }

     connection = new signalR.HubConnectionBuilder()
         .withUrl("https://localhost:5205/chatHub", {
            accessTokenFactory: () => {
                return token; 
            }
        })
        .configureLogging(signalR.LogLevel.Information)
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveMessage", (user, message) => {
        const msg = document.createElement("div");
        msg.textContent = `${user}: ${message}`;
        document.getElementById("messagesList").appendChild(msg);
    });

    try {
        await connection.start();
        console.log("SignalR Connected");
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        document.getElementById("messagesList").appendChild("Unable to connect to server");
        setTimeout(startConnection, 5000); 
    }
}

document.addEventListener("DOMContentLoaded", function () {
    if (window.location.pathname.toLowerCase() === "/home/chat") {
        console.log("Token", localStorage.getItem("jwt"));
        startConnection();

        const sendButton = document.getElementById("sendButton");
        if (sendButton) {
            sendButton.addEventListener("click", sendMessage);
        }
    }
});
});
