import React, { Component } from 'react';
import { Link } from 'react-router-dom';

import { AuthContext } from "./context/AuthProvider";

import PopUp from "./components/popUp";
import ValidationBox from "./components/ValidationBox";
import axios from "./api/axios";

import APIs from "./context/ApiURL";

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
const EMAIL_REGEX = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

class EditProfile extends Component {
    static contextType = AuthContext;
    
    constructor(props) {
        super(props);
        this.state = {
            login: "",
            username: "",
            oldPassword: "",
            newPassword: "",
            newPassword2: "",
            email: "",

            popUpMess: "",
            stateOfPopUp: false,

            loginRegex: false,
            usernameRegex: false,
            passRegex: false,
            pass2Regex: false,
            emailRegex: false,

            loginFocus: false,
            usernameFocus: false,
            oldPasswordFocus: false,
            newPasswordFocus: false,
            newPassword2Focus: false,
            emailFocus: false
        }

        this.handleChange = this.handleChange.bind(this);
        this.handleFocusOn = this.handleFocusOn.bind(this);
        this.closePopUpMess = this.closePopUpMess.bind(this);
    }

    handleChange = async (event) => {
        this.closePopUpMess();

        const { name, value } = event.target;
        this.setState({
            [name]: value
        });

        let validation;
        if(name == "login"){
            validation = USER_REGEX.test(value);
            this.setState({loginRegex : validation});
        }else if(name == "username"){
            this.setState({
                usernameRegex: value.length  >= 3
            });
        }else if(name == "newPassword"){
            validation = PASS_REGEX.test(value);
             this.setState({
                passRegex : validation,
                pass2Regex: this.state.newPassword2 == value
            });
        }else if(name == "newPassword2"){
            validation = this.state.password == value;
            this.setState({pass2Regex : validation});
        }

    };

    handleFocusOn = (event) => {
        this.setState({
            loginFocus: false,
            usernameFocus: false,
            oldPasswordFocus: false,
            newPasswordFocus: false,
            newPassword2Focus: false,
            emailFocus: false
        });

        const { name, value } = event.target;
        this.setState({
            [name+"Focus"]: true
        });
    }

    showPopUpMess(mess){
        this.setState({popUpMess: mess, stateOfPopUp: true});
    }

    closePopUpMess(){
        this.setState({popUpMess: '', stateOfPopUp: false});
    }

    render() {
        return (
            <div id="mainEditProfilePage">
                <div id='menuBar'>
                <div id='logoInMenu'/>
                <div id='profileBox'>
                    <Link to='/message' className='btn2 goBackBtn'>Go back</Link>
                </div>
                </div>
                <div className="editPanelWrapper">
                    <form className='editPanel'>

                        {/* LOGIN */}
                        <label htmlFor="login" className={this.state.login ? this.state.loginRegex ? "correctValidation" : "wrongValidation" : ""}>Login: </label>
                        <input value={this.state.login} onChange={this.handleChange} onFocus={this.handleFocusOn} name="login" id="login" autoComplete="off" type="text" className="textInput"/><br/>

                        <ValidationBox
                            regex={this.state.loginRegex} value={this.state.login} focus={this.state.loginFocus}
                            text={<>
                                Has 5 - 24 characters in length<br/>
                                Has to start with English letter<br/>
                                Can cointain English letter, digits and -_#
                                </>
                            } 
                        />
                        {/* USERNAME */}
                        <label htmlFor="username" className={this.state.username ? this.state.usernameRegex ? "correctValidation" : "wrongValidation" : ""}>Username: </label>
                        <input value={this.state.username} onChange={this.handleChange} onFocus={this.handleFocusOn} name="username" id="username" autoComplete="off" type="text" className="textInput"/><br/>
                        
                        <ValidationBox
                            regex={this.state.usernameRegex} value={this.state.username} focus={this.state.usernameFocus}
                            text={<>
                                Has 3 + characters in length<br/>
                                </>
                            } 
                        />
                        <br/><br/><br/>
                        {/* PASSWORDS */}

                        <label htmlFor="oldPassword" >Old password: </label>
                        <input value={this.state.oldPassword} onChange={this.handleChange} onFocus={this.handleFocusOn} name="oldPassword" id="oldPassword" type="password" className="textInput"/><br/>

                        <label htmlFor="newPassword" className={this.state.newPassword ? this.state.passRegex ? "correctValidation" : "wrongValidation" : ""}>New password: </label>
                        <input value={this.state.newPassword} onChange={this.handleChange} onFocus={this.handleFocusOn} name="newPassword" id="newPassword" type="password" className="textInput"/><br/>

                        <ValidationBox
                            regex={this.state.passRegex} value={this.state.newPassword} focus={this.state.newPasswordFocus}
                            text={<>
                                Has 8 - 64 characters in length<br/>
                                At least one uppercase English letter <br/>
                                At least one lowercase English letter<br/>
                                At least one digit and special character
                                </>
                            } 
                        />


                        <label htmlFor="newPassword2" className={this.state.newPassword2 ? this.state.pass2Regex ? "correctValidation" : "wrongValidation" : ""}>Repeat password: </label>
                        <input value={this.state.newPassword2} onChange={this.handleChange} onFocus={this.handleFocusOn} name="newPassword2" id="newPassword2" type="password" className="textInput"/><br/>
                        
                        <ValidationBox
                            regex={this.state.pass2Regex} value={this.state.newPassword2} focus={this.state.newPassword2Focus}
                            text={<>
                                Passwords have to match<br/>
                                </>
                            } 
                        />

                    </form>

                    <PopUp state={this.state.stateOfPopUp} mess={this.state.popUpMess} close={this.closePopUpMess}/>
                </div>
            </div>
        );
    }
}

export default EditProfile;