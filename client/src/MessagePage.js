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
    FIND_PEOPLE_URL : "/api/users/getUsersByText",
    FIND_FRIENDS_URL : "/api/friends/getFriends",
    REFRESH_TOKEN_URL: "/api/refreshAccessToken"
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
            messKey: 0,

            listOfUsers: [],
            findUsersStatus: "not typed",
            listOfFriends: [],
            findFriendsStatus: "not found"
        }
        this.handleChangeTxt = this.handleChangeTxt.bind(this);
        this.handleSwitchBtn = this.handleSwitchBtn.bind(this);
        this.addMessage = this.addMessage.bind(this);
        this.singOut = this.singOut.bind(this);
    }

    handleChangeTxt = async (event) => {

        const { name, value } = event.target;
        await this.setState({
            [name]: value
        });

        if(name == "searchBar"){
            if(!this.state.yourChatIsActive){
                this.searchPeople();
            }

        }
    };

    handleSwitchBtn = async (event) => {
        const {name}  = event.target;
        let flag = name == "yourChatsBtn" ? true : false;
        if(this.state.yourChatIsActive != flag){
            await this.setState({
                yourChatIsActive: flag,
                searchBar: '',
                findUsersStatus: 'not typed',
                listOfUsers: []
            });
        }
        
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
        sessionStorage.removeItem('userInfo');
    }

    async refreshAccessToken(){
        const {setAuth, username, roles,accessToken} = this.context;


    const refreshToken = sessionStorage.getItem('refreshToken');
    //fetch
    try{
        const data = await axios.post(APIs.REFRESH_TOKEN_URL,{
            refreshToken: refreshToken
        }, // Pass as a plain object
          {
            withCredentials: true, //pass a http only cookie
            headers: {
              Authorization: `Bearer ${accessToken}`,
              'Content-Type': 'application/json'
            }
          }
        );
    
            let res = data.data;
            //is ok
            if(data.status == 200){
                console.log("SUKCES: ", res.title);
                await setAuth(username, roles, res.resultData);
            }
    
        } catch(err){
            console.log("Error: Can't refresh token: ", err);
        }
      }

    

    async searchPeople(){
        const {username, accessToken} = this.context;

        if(this.state.searchBar == "") return;

        //fetch
        try{
            const data = await axios.get(`${APIs.FIND_PEOPLE_URL}/${this.state.searchBar}`,
                {
                    withCredentials: true,
                    headers: { 
                        Authorization: `Bearer ${accessToken}`,
                    },
                }
            );

            let res = data.data;
            //is ok
            if(data.status == 200){
                console.log(res.title);
                console.log(res.resultData);

                await this.setState({findUsersStatus: 'found',listOfUsers: res.resultData});

            }

        } catch(err){
            if (err.response && err.response.status === 401) { // Unauthorized, token expired
                await this.refreshAccessToken();
                // retry request
                await this.searchPeople();
            }else if(err.response.status === 400){
                await this.setState({findUsersStatus: 'not typed'});
            }
            else if(err.response.status === 404){
                await this.setState({findUsersStatus: 'not found'});
            }
            else {
                console.error(err);
            }
        }
    }

    async getFriends(){
        const {username, accessToken} = this.context;
        let userID;

        //fetch
        try{
            const data = await axios.get(`${APIs.FIND_FRIENDS_URL}/${userID}`,
                {
                    withCredentials: true,
                    headers: { 
                        Authorization: `Bearer ${accessToken}`,
                    },
                }
            );

            let res = data.data;
            //is ok
            if(data.status == 200){
                console.log(data.data.title);
                console.log(res.resultData);

                await this.setState({listOfFriends: res.resultData});

            }

        }catch(err){
            if (err.response && err.response.status === 401) { // Unauthorized, token expired
                await this.refreshAccessToken();
                // retry request
                await this.searchPeople();
            }else if(err.response.status === 400){
                await this.setState({findFriendsStatus: 'bad ID'});
            }
            else if(err.response.status === 404){
                await this.setState({findFriendsStatus: 'not found'});
            }
            else {
                console.error(err);
            }
        }

    }

    async filterFriends(){
        
    }


    componentDidMount(){
        const { setAuth,username, accessToken} = this.context;
        this.setState({username: username});

        if(!accessToken){
            this.refreshAccessToken();
        }


    }

    

    render() {


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
                    {
                    this.state.yourChatIsActive ?

                    (
                        this.state.listOfFriends.map(user => (
                            <FriendTile key={user.id} username={user.username} author={""} mess={""} />
                        ))
                    )

                    :
                    (
                        this.state.findUsersStatus == "not typed" ? 
                        (<div className=''>Please type 3 or more characters</div>)
                        :
                        (this.state.findUsersStatus == "not found" ?
                            (<div>There are no users named {this.state.searchBar} ...</div>)
                            :
                            (
                                this.state.listOfUsers.map(user => (
                                    <PersonTile key={user.id} username={user.userName} />
                                ))
                            )
                        )
                    )
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