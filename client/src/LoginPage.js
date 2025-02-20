import React from "react";
import { Link } from 'react-router-dom';
import PopUp from "./components/popUp";
import axios from "./api/axios";

import { AuthContext } from "./context/AuthProvider";
import { Navigate } from "react-router";

import APIs from "./context/ApiURL";

class LoginPage extends React.Component{
    static contextType = AuthContext;

    constructor(props) {
        super(props);
        this.state = {
            username: "",
            password: "",
            popUpMess: "",
            stateOfPopUp: false,
            directory: ""
        }

        this.handleChange = this.handleChange.bind(this);
        this.submitLogin = this.submitLogin.bind(this);
        this.closePopUpMess = this.closePopUpMess.bind(this);
    }

    navigate(dir){
        this.setState({directory: dir});
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
        const {accessToken} = this.context;

        try{
            const data = await axios.post(APIs.LOGIN_URL,
                JSON.stringify({
                    userName: this.state.username,
                    password: this.state.password
                }),
                {
                    withCredentials: true, //pass a http only cookie
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    }
                }
            );

            let res = data.data.resultData;
            console.log(data);
            //is ok
            if(data.status == 200){
                console.log(res);
                this.showPopUpMess(data.data.title);

                let userData = res.user;

                const { username, userID ,roles, accessToken, setAuth } = this.context;
                setAuth(userData.userName,userData.id ,["user"], userData.accessToken);

                // refreshToken
                sessionStorage.setItem('refreshToken', userData.refreshToken);

                // user info
                let userInfo = {
                    name: userData.userName,
                    userID: userData.id,
                    rules: roles,
                    currentChat: ''
                };
                sessionStorage.setItem('userInfo', JSON.stringify(userInfo));

                console.log(username, roles, accessToken );
                this.navigate("/message");
            }

        } catch(err){
            let mess = err.response?.data?.title;
            if(mess == undefined){
                mess = err.message;
            }

            this.showPopUpMess(mess);
        }

        //to remove
        // const { username, roles, accessToken, setAuth } = this.context;
        // setAuth(this.state.username, ["user"], "token");
        // console.log(username, roles, accessToken );
        // this.navigate("/message");
        //----
        this.setState({username: "", password: ""});
    }

    showPopUpMess(mess){
        this.setState({popUpMess: mess, stateOfPopUp: true});
    }

    closePopUpMess(){
        this.setState({popUpMess: '', stateOfPopUp: false});
    }

    componentDidMount(){
        // redirect to /message if user is logged in
        const { accessToken} = this.context;
        if(accessToken == null || accessToken == ''){
            const refreshToken = sessionStorage.getItem('refreshToken');
            if(refreshToken){
                this.navigate("/message");
            }
        }else{
            this.navigate("/message");
        }
    }

    render(){
        const { username, roles, accessToken } = this.context;

        console.log("Current context LOGIN:", username, roles, accessToken);
        
        return (
            this.state.directory ?
            <Navigate to={this.state.directory}/>
            : <div id="mainloginPage">
                <form className="loginPanel">
                    <label htmlFor="username">Username: </label>
                    <input value={this.state.username} onChange={this.handleChange} name="username" id="username" autoComplete="off" type="text" className="textInput"/><br/><br/>
                    <label htmlFor="password">Password: </label>
                    <input value={this.state.password} onChange={this.handleChange} name="password" id="password" type="password" className="textInput"/><br/>

                    <PopUp state={this.state.stateOfPopUp} mess={this.state.popUpMess} close={this.closePopUpMess}/>


                    <button className="btn" onClick={this.submitLogin}>Login</button><br/><br/>
                    <div>Don't have an account? Sign up below</div>
                    <Link to="/register" >Create an account</Link>
                    <Link to="/message" >MessagePage</Link>
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