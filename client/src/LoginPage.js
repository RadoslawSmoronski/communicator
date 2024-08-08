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
                userName: this.state.username,
                password: this.state.password
            })
        };



        fetch('http://localhost:5205/api/user/login', requestOptions)
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
            if(data.succeeded){
                console.log(data);
                this.showPopUpMess(data.message);
            }
        })
        .catch(error => {
            this.showPopUpMess(error.toString());
        });

        

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
            <div id="mainloginPage">
                <div className="loginPanel">
                    <label htmlFor="username">Username: </label>
                    <input value={this.state.username} onChange={this.handleChange} name="username" id="username" autoComplete="off" type="text" className="textInput"/><br/><br/>
                    <label htmlFor="password">Password: </label>
                    <input value={this.state.password} onChange={this.handleChange} name="password" id="password" type="password" className="textInput"/><br/>
                    <table className="popUp" style={{display: this.state.stateOfPopUp ? '' : 'none' }}>
                        <tr>
                            <th>{this.state.popUpMess}</th>
                            <th className="closeBtnBox"><div className="closeBtn" onClick={this.closePopUpMess}/></th>
                        </tr>

                    </table>
                    <button className="btn" onClick={this.submitLogin}>Login</button><br/><br/>
                    <div>Nie masz konta? Zajerestruj się poniżej</div>
                    <a className="link" href="/register">Stwórz konto</a>
                </div>

                <div className="welcomeBlock">
                    <div className="logoAndText">
                    <div id="logo"/>
                    <div className="bottomText"><div className="highlightText">Lorem ipsum dolor sit amet.</div> consectetur adipiscing elit. Nam egestas arcu quis ex vehicula facilisis. Sed maximus nunc vitae tincidunt porttitor. Phasellus congue imperdiet vestibulum. Phasellus a diam iaculis urna condimentum dictum vel in nulla. Proin velit velit, aliquam sed consectetur vitae, porttitor eget turpis. Cras sollicitudin eros eget libero tempor, quis posuere justo laoreet. </div>
                    </div>
                </div>

                <div id="welcomeBlockSmall">
                    <div id="logo"/>
                </div>
            </div>
        );
    }

}
export default LoginPage;