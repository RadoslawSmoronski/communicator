import React, { Component } from 'react';
import { Link } from 'react-router-dom';

import { AuthContext } from "./context/AuthProvider";
import axios from "./api/axios";

import { FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import { faCircleInfo, faMagnifyingGlass, faMessage, faPaperPlane, faPhone } from "@fortawesome/free-solid-svg-icons";

import FriendTile from './components/FriendTile';
import MessageTile from './components/MessageTile';
import PersonTile from './components/PersonTile';

const APIs = {
    FIND_PEOPLE_URL : "/api/user/searchTest",
    REFRESH_TOKEN: "api/refreshAccessToken"
}

class MessagePage extends Component {
    static contextType = AuthContext;

    constructor(props) {
        super(props);
        this.state = {
            username: 'Bartek',
            searchBar: '',
            activeFriend: 'Rado',
            messageInput: '',

            yourChatIsActive: true,

            messages: [],
            messKey: 0
        }
        this.handleChangeTxt = this.handleChangeTxt.bind(this);
        this.handleSwitchBtn = this.handleSwitchBtn.bind(this);
        this.addMessage = this.addMessage.bind(this);
        this.singOut = this.singOut.bind(this);
    }

    handleChangeTxt = (event) => {

        const { name, value } = event.target;
        this.setState({
            [name]: value
        });

        if(name == "searchBar"){
            this.searchPeople();
        }
    };

    handleSwitchBtn = (event) => {
        const {name}  = event.target;
        let flag = name == "yourChatsBtn" ? true : false;
        this.setState({yourChatIsActive: flag});
    }

    addMessage() {
        // temporary for testing 
        const newMessage = {
            text: this.state.messageInput, 
            yours: true,                    
            time: new Date().toLocaleTimeString()  
        };

        this.setState((prevState) => ({
            messages: [newMessage,...prevState.messages],
            messageInput: '',  
        }));
    }

    singOut(){
        const { setAuth} = this.context;
        setAuth('', [], '');
        sessionStorage.removeItem('refreshToken');
    }

    async refreshAccessToken(){
        const {setAuth, username, roles,accessToken} = this.context;

        const refreshToken = sessionStorage.getItem('refreshToken');
        //fetch
        try{
            const data = await axios.post(APIs.REFRESH_TOKEN,
                JSON.stringify({
                    refreshToken  
                }),
                {
                    withCredentials: true //can be problematic
                }
            );

            let res = data.data;
            //is ok
            if(res.succeeded){
                console.log(res);
                setAuth(username, roles, res.accessToken);
                //new refreshToken
                sessionStorage.setItem('refreshToken', res.refreshToken);
            }

        } catch(err){
            console.log("Error: Can't refresh token: ", err);
        }
    }

    async searchPeople(){
        const {username, accessToken} = this.context;

        //fetch
        try{
            const data = await axios.get(APIs.FIND_PEOPLE_URL,
                {
                    params:{
                        input: this.state.searchBar
                    },
                    headers: { 
                        //Authorization: `Bearer ${accessToken}`,
                        'Content-Type': 'application/json'
                    },
                    //withCredentials: true //can be problematic
                }
            );

            let res = data.data;
            //is ok
            if(res.succeeded){
                console.log(res);
                
            }

        } catch(err){
            // if (err.response && err.response.status === 401) { // Unauthorized, token expired
            //     await this.refreshAccessToken();
            //     // retry request
            //     await this.searchPeople();
            //   } else {
            //     console.error(err);
            //   }
        }
    }

    componentDidMount(){
        const { setAuth,username, accessToken} = this.context;
        this.setState({username: username});

        if(!accessToken){
            this.refreshAccessToken();
        }
    }

    

    render() {
        let ziomki = [];
        for(let i=0; i < 20; i++){
            ziomki.push(<FriendTile key={i} username={"Ziomek "+i.toString()} author="You" mess="Joł"/>)
        }
        let randomy = [];
        for(let i=0; i < 20; i++){
            randomy.push(<PersonTile key={i} username={"Random "+i.toString()}/>);
        }

        const { username, roles, accessToken } = this.context;

        console.log("Current context MESSAGE PAGE:", username, roles, accessToken);

        return (
            <div id='mainMessagePage'>
                <div id='menuBar'>
                    <div id='logoInMenu'/>
                    <div id='profileBox'>
                        <button className='btn2' onClick={this.singOut}>Sing out</button>
                        {this.state.username}
                        <div className='profileIcon'/>
                    </div>
                    
                </div>
                <div id='searchBar'>
                    <div className="inputWrapper">
                        <FontAwesomeIcon icon={faMagnifyingGlass} className='inputIcon'/>
                        <input className='textInput2' placeholder=' Search for people' value={this.state.searchBar}
                        onChange={this.handleChangeTxt} name='searchBar' type="text" autoComplete='off'/>
                    </div>
                    <div id='buttonsSearchBar'>
                        <button name='yourChatsBtn' className={this.state.yourChatIsActive ? 'btn2 btn2Active' : 'btn2'} 
                        onClick={this.handleSwitchBtn}>Your chats</button>
                        <button name='findFriendsBtn' className={!this.state.yourChatIsActive ? 'btn2 btn2Active' : 'btn2'}
                        onClick={this.handleSwitchBtn}>Find friends</button>
                    </div>
                </div>
                <div id='friendsList'>
                    {this.state.yourChatIsActive ?
                    <>
                    <FriendTile username="Bartosz" author="Bartosz" mess="Hand dziak"/>
                    <FriendTile username="Rado" author="Rado" mess="Na na na NA"/>
                    {ziomki}
                    </>:
                     randomy
                    }
                </div>
                <div id='friendBar'>
                    <div className='friendBarIconBox'>
                        <div className='profileIcon'/>
                    </div>
                    <div className='friendBarUserName'>
                        {this.state.activeFriend}
                    </div>
                    <div className='friendBarRightBox'>
                        <FontAwesomeIcon icon={faPhone} className='friendBarIcon'/>
                        <FontAwesomeIcon icon={faCircleInfo} className='friendBarIcon'/>
                    </div>
                </div>
                <div id='messageBox'>
                    {/* Renderowanie wiadomości z tablicy messages */}
                    {this.state.messages.map((message, index) => (
                        <MessageTile key={index} mess={message.text} yours={message.yours} time={message.time} />
                    ))}

                    <MessageTile mess=" eu commodo lectus, ac viverra est. Sed eleifend massa a dignissim varius. Mauris id diam nec metus aliquam dapibus sit amet at odio. Praesent q" yours={false} time="16:40"/>
                    <MessageTile mess="j suscipit metus convalli" yours={true} time="15:34"/>
                    <MessageTile mess=" sapien euismod aliquam. Nulla" yours={true} time="15:33"/>
                    <MessageTile mess="Lorem ipsum dolor sit amet, consectetur adipiscing elit. Pellentesque et leo quis arcu maximus mattis. Nullam ac libero enim. Sed ut orci mi. Curabitur sollicitudin urna velit, sed porta nulla porta nec. Morbi volutpat pharetra orci vehicula ultricies. Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. Maecenas non mi vel sem aliquet laoreet. Vivamus sodales nisl a lectus accumsan, eu tincidunt felis ullamcorper. Praesent molestie non purus in finibus. Suspendisse hendrerit varius co" yours={false} time="14:33"/>
                
                </div>
                <div id='sendMessageBox'>
                <input className='textInput2 sendMessageInput' placeholder='Type a message...' value={this.state.messageInput}
                onChange={this.handleChangeTxt} name='messageInput' type="text" autoComplete='off'/>
                <FontAwesomeIcon icon={faPaperPlane} className='friendBarIcon' onClick={this.addMessage}/>
                </div>
            </div>
        );
    }
}

export default MessagePage;