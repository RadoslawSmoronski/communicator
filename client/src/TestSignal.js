import React, { Component } from 'react';

import * as signalR from "@microsoft/signalr";

const SIGNAL_URL = "/testsignal";
const SEND_MESSAGE = "sendMessage";
const GET_MESSAGE = "getNewMessage";

class TestSignal extends Component {
    constructor(props) {
        super(props);
        this.state = {
            connection: "",
            username: "",
            message: "",
            allMessages: [
                {user: 'dupa', message: 'joł'},
                {user: 'dupa213', message: 'joł2'},
                {user: 'dupa3', message: 'joł33'}
            ]
        }
        this.handleChange = this.handleChange.bind(this);
        this.sendMessage = this.sendMessage.bind(this);
    }

    handleChange = (event) => {

        const { name, value } = event.target;
        this.setState({
            [name]: value
        });

    };

    sendMessage = async (event) =>{
        event.preventDefault();
        console.log(this.state);

        if (this.state.connection && this.state.message.trim() !== "") {
            // send my message
            connection.invoke(SEND_MESSAGE, this.state.username, this.state.message)
                .catch(err => console.error("Error sending message: ", err));
            this.setState({ message: "" });
        }

    }

    componentDidMount(){
        // new connection
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5205" + SIGNAL_URL)
            .withAutomaticReconnect()
            .build();

        // get connection
        connection.start()
            .then(() => console.log("Connected to SignalR"))
            .catch(err => console.error("Connection failed: ", err));

        // listening for new messages
        connection.on(GET_MESSAGE, (user, message) => {
            this.setState(prevState => ({
                messages: [...prevState.messages, { user, message }]
            }));
        });

        this.setState({ connection });
    }

    render() {
        return (
            <div>
                test signal
                <div className='testMessages'>
                    {this.state.allMessages.map((msg, index) => (
                            <p key={index}><b>{msg.user}:</b> {msg.message}</p>
                    ))}
                </div>
                <form>
                    <label htmlFor="username">Username: </label>
                    <input value={this.state.username} onChange={this.handleChange} name='username' id='usernameTest' type="text"/><br/>
                    <label htmlFor="message">Message: </label>
                    <textarea value={this.state.message} onChange={this.handleChange} name='message' id='usernameTest' /><br/>
                    <button type='submit' onClick={this.sendMessage}>Send message</button>
                </form>
            </div>
        );
    }
}

export default TestSignal;