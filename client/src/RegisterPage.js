import React from "react";
import { faCheck, faTimes, faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

import PopUp from "./components/popUp";
import ValidationBox from "./components/ValidationBox";
import axios from "./api/axios";

const REGISTER_URL = "/api/user/register";

//REGEX
const USER_REGEX = /^[a-zA-Z][a-zA-Z0-9-_#]{4,24}$/;
// Has 5 - 24 characters in length
// Has to start with English letter
// Can cointain English letter, digits and -_#
const PASS_REGEX = /^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{8,64}$/;
// Has 8 - 64 characters in length
// At least one uppercase English letter 
// At least one lowercase English letter
// At least one digit and special character

class RegisterPage extends React.Component{

    constructor(props) {
        super(props);
        this.state = {
            username: "",
            password: "",
            password2: "",

            popUpMess: "Złe hasło",
            stateOfPopUp: false,

            userRegex: false,
            passRegex: false,
            pass2Regex: false,

            usernameFocus: false,
            passwordFocus: false,
            password2Focus: false,
        }

        this.handleChange = this.handleChange.bind(this);
        this.handleFocusOn = this.handleFocusOn.bind(this);
        this.submitRegister = this.submitRegister.bind(this);
        this.closePopUpMess = this.closePopUpMess.bind(this);
    }

    handleChange = (event) => {
        this.closePopUpMess();

        const { name, value } = event.target;
        this.setState({
            [name]: value
        });

        let validation;
        if(name == "username"){
            validation = USER_REGEX.test(value);
            this.setState({userRegex : validation});
        }else if(name == "password"){
            validation = PASS_REGEX.test(value);
            this.setState({passRegex : validation,
                pass2Regex: this.state.password2 == value
            });
        }else if(name == "password2"){
            validation = this.state.password == value;
            this.setState({pass2Regex : validation});
        }

    };

    handleFocusOn = (event) => {
        this.setState({
            usernameFocus: false,
            passwordFocus: false,
            password2Focus: false
        });

        const { name, value } = event.target;
        this.setState({
            [name+"Focus"]: true
        });
    }

    async submitRegister(event){
        event.preventDefault();
        

        //blank input
        if(this.state.username == "" || this.state.password == "" || this.state.password2 == ""){
            this.showPopUpMess("Nazwa użytkownika lub hasło nie może być puste");
            return;
        }

        //critiria user or password
        if(!this.state.userRegex || !this.state.passRegex){
            this.showPopUpMess("Nazwa użytkownika lub hasło nie spełniają kryteriów");
            return;
        }

        //different passwords
        if(!this.state.pass2Regex){
            this.showPopUpMess("Podane hasła są różne!");
            this.setState({ password: "", password2: ""});
            return;
        }

        //fetch
        try{
            const data = await axios.post(REGISTER_URL,
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
           <div id="mainregisterPage">
           <form className="loginPanel">

                    <label htmlFor="username" className={this.state.username ? this.state.userRegex ? "correctValidation" : "wrongValidation" : ""}>Username: </label>
                    <input value={this.state.username} onChange={this.handleChange} onFocus={this.handleFocusOn} name="username" id="username" autoComplete="off" type="text" className="textInput"/><br/>

                    <ValidationBox
                        regex={this.state.userRegex} value={this.state.username} focus={this.state.usernameFocus}
                        text={<>
                            Has 5 - 24 characters in length<br/>
                            Has to start with English letter<br/>
                            Can cointain English letter, digits and -_#
                            </>
                        } 
                    />

                    <label htmlFor="password" className={this.state.password ? this.state.passRegex ? "correctValidation" : "wrongValidation" : ""}>Password: </label>
                    <input value={this.state.password} onChange={this.handleChange} onFocus={this.handleFocusOn} name="password" id="password" type="password" className="textInput"/><br/>

                    <ValidationBox
                        regex={this.state.passRegex} value={this.state.password} focus={this.state.passwordFocus}
                        text={<>
                            Has 8 - 64 characters in length<br/>
                            At least one uppercase English letter <br/>
                            At least one lowercase English letter<br/>
                            At least one digit and special character
                            </>
                        } 
                    />


                    <label htmlFor="password2" className={this.state.password2 ? this.state.pass2Regex ? "correctValidation" : "wrongValidation" : ""}>Repeat password: </label>
                    <input value={this.state.password2} onChange={this.handleChange} onFocus={this.handleFocusOn} name="password2" id="password2" type="password" className="textInput"/><br/>
                    
                    <ValidationBox
                        regex={this.state.pass2Regex} value={this.state.password2} focus={this.state.password2Focus}
                        text={<>
                            Passwords have to match<br/>
                            </>
                        } 
                    />

                    <PopUp state={this.state.stateOfPopUp} mess={this.state.popUpMess} close={this.closePopUpMess}/>


                    <button className="btn" onClick={this.submitRegister}>Register</button><br/><br/>
                    <div>Masz już konto? Zaloguj się poniżej</div>
                    <a className="link" href="/login">Zaloguj się</a>
            </form>
            <div id="logo"/>

           </div> 
        )}
}
export default RegisterPage;