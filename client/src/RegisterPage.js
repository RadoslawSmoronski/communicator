import React from "react";

class RegisterPage extends React.Component{

    constructor(props) {
        super(props);
        this.state = {
            username: "",
            password: "",
            password2: "",
            popUpMess: "Złe hasło",
            stateOfPopUp: false
        }

        this.handleChange = this.handleChange.bind(this);
        this.submitRegister = this.submitRegister.bind(this);
        this.closePopUpMess = this.closePopUpMess.bind(this);
    }

    handleChange = (event) => {
        const { name, value } = event.target;
        this.setState({
            [name]: value
        });
    };

    submitRegister(){
        console.log(this.state.username, this.state.password, this.state.password2);

        //blank input
        if(this.state.username == "" || this.state.password == "" || this.state.password2 == ""){
            this.showPopUpMess("Nazwa użytkownika lub hasło nie może być puste");
            return;
        }

        //different passwords
        if(this.state.password != this.state.password2){
            this.showPopUpMess("Podane hasła są różne!");
            this.setState({ password: "", password2: ""});
            return;
        }

        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                username: this.state.username,
                password: this.state.password
            })
        };


        fetch('http://localhost:5205/api/register', requestOptions)
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
            this.showPopUpMess("Błąd rejestracji!", error.toString());
        });


        this.setState({username: "", password: "", password2: ""});
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
                    <label htmlFor="password">Repeat password: </label>
                    <input value={this.state.password2} onChange={this.handleChange} name="password2" id="password2" type="password" className="textInput"/><br/>
                    <table className="popUp" style={{visibility: this.state.stateOfPopUp ? 'visible' : 'hidden' }}>
                        <tr>
                            <th>{this.state.popUpMess}</th>
                            <th className="closeBtnBox"><div className="closeBtn" onClick={this.closePopUpMess}/></th>
                        </tr>

                    </table>
                    <button className="btn" onClick={this.submitRegister}>Register</button><br/><br/>
                    <div>Masz już konto? Zaloguj się poniżej</div>
                    <a className="link" href="/login">Zaloguj się</a>
                </div>
           </> 
        )}
}
export default RegisterPage;