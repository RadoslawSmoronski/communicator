import React from "react";
import PopUp from "./components/popUp";
import axios from "./api/axios";

const LOGIN_URL = "/api/user/login";

class LoginPage extends React.Component{

    constructor(props) {
        super(props);
        this.state = {
            username: "",
            password: "",
            popUpMess: "",
            stateOfPopUp: false
        }

        this.handleChange = this.handleChange.bind(this);
        this.submitLogin = this.submitLogin.bind(this);
        this.closePopUpMess = this.closePopUpMess.bind(this);
    }

    handleChange = (event) => {
        this.closePopUpMess();

        const { name, value } = event.target;
        this.setState({
            [name]: value
        });
    };

    async submitLogin(event){
        event.preventDefault();

        //blank input
        if(this.state.username == "" || this.state.password == ""){
            this.showPopUpMess("Nazwa użytkownika lub hasło nie może być puste");
            return;
        }

        //fetch
        try{
            const data = await axios.post(LOGIN_URL,
                JSON.stringify({
                    userName: this.state.username,
                    password: this.state.password
                }),
                {
                    headers: { 'Content-Type': 'application/json' },
                    withCredentials: true
                }
            );

            //is ok
            if(data.succeeded){
                console.log(data);
                this.showPopUpMess(data.message);
            }

        } catch(err){
            this.showPopUpMess(err.toString());
        }

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
                <form className="loginPanel">
                    <label htmlFor="username">Username: </label>
                    <input value={this.state.username} onChange={this.handleChange} name="username" id="username" autoComplete="off" type="text" className="textInput"/><br/><br/>
                    <label htmlFor="password">Password: </label>
                    <input value={this.state.password} onChange={this.handleChange} name="password" id="password" type="password" className="textInput"/><br/>

                    <PopUp state={this.state.stateOfPopUp} mess={this.state.popUpMess} close={this.closePopUpMess}/>


                    <button className="btn" onClick={this.submitLogin}>Login</button><br/><br/>
                    <div>Nie masz konta? Zajerestruj się poniżej</div>
                    <a className="link" href="/register">Stwórz konto</a>
                </form>

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