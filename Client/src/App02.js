import React from "react";

class App02 extends React.Component{
    constructor(props) {
        super(props);
        this.state = {
            username: "",
            password: ""
        }

        this.handleChange = this.handleChange.bind(this);
        this.submitLogin = this.submitLogin.bind(this);
    }

    handleChange(event){
        let usernameVal = document.getElementById("username").value;
        let passwordVal = document.getElementById("password").value;
        this.setState({username: usernameVal, password: passwordVal});
    }

    async submitLogin(){
        console.log(this.state.username, this.state.password)
        //fetch
        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: this.state.username,
                password: this.state.password
            })
        };
        fetch('http://localhost:5205/api/account/login', requestOptions)
        .then(response => response.json())
        .then(data => console.log(data));

        this.setState({username: "", password: ""});
    }

    render(){
        
        return (
            <div id="formBody">
            <label htmlFor="username">Username: </label>
            <input name="username" id="username" type="text" value={this.state.username} onChange={this.handleChange}/><br/><br/>
            <label htmlFor="password">Password: </label>
            <input name="password" id="password" type="password" value={this.state.password} onChange={this.handleChange}/><br/>
            <button onClick={this.submitLogin}>Login</button>
            </div>
        );
    }
}
export default App02;