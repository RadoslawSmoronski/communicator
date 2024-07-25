import React from "react";

class LoginPage extends React.Component{

    constructor(props) {
        super(props);
        this.state = {
            username: "",
            password: "",
            popUpMess: "Złe hasło",
            stateOfPopUp: false
        }

        this.handleChange = this.handleChange.bind(this);
        this.submitLogin = this.submitLogin.bind(this);
        this.closePopUpMess = this.closePopUpMess.bind(this);
    }

    handleChange = (event) => {
        const { name, value } = event.target;
        this.setState({
            [name]: value
        });
    };

    submitLogin(){
        console.log(this.state.username, this.state.password)

        //blank input
        if(this.state.username == "" || this.state.password == ""){
            this.showPopUpMess("Nazwa użytkownika lub hasło nie może być puste");
            return;
        }

        //fetch
        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: this.state.username,
                password: this.state.password
            })
        };



        fetch('http://localhost:5205/api/login', requestOptions)
        .then(async response => {
            const isJson = response.headers.get('content-type')?.includes('application/json');
            const data = isJson && await response.json();

            // check for error response
            if (!response.ok) {
                // get error message from body or default to response status
                const error = (data && data.message) || response.status;
                return Promise.reject(error);
            }

            //is ok
            console.log(data);
        })
        .catch(error => {
            this.showPopUpMess("Błąd logowania!", error.toString());
        });

        this.showPopUpMess("wiadomosc");

        this.setState({username: "", password: ""});
    }

    showPopUpMess(mess){
        this.setState({popUpMess: mess, stateOfPopUp: true});
    }

    closePopUpMess(){
        this.setState({popUpMess: '', stateOfPopUp: false});
    }

    render(){
        
        return (
            <>
                <div className="loginPanel">
                    <label htmlFor="username">Username: </label>
                    <input value={this.state.username} onChange={this.handleChange} name="username" id="username" autoComplete="off" type="text" className="textInput"/><br/><br/>
                    <label htmlFor="password">Password: </label>
                    <input value={this.state.password} onChange={this.handleChange} name="password" id="password" type="password" className="textInput"/><br/>
                    <table className="popUp" style={{visibility: this.state.stateOfPopUp ? 'visible' : 'hidden' }}>
                        <tr>
                            <th>{this.state.popUpMess}</th>
                            <th className="closeBtnBox"><div className="closeBtn" onClick={this.closePopUpMess}/></th>
                        </tr>

                    </table>
                    <button className="btn" onClick={this.submitLogin}>Login</button><br/><br/>
                    <div>Nie masz konta? Zajerestruj się poniżej</div>
                    <a className="link" href="/register">Stwórz konto</a>
                </div>
            </>
        );
    }

}
export default LoginPage;