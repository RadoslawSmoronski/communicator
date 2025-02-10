import React, { Component } from 'react';
import { Link } from 'react-router-dom';

import { AuthContext } from "./context/AuthProvider";
import axios from "./api/axios";

import { FontAwesomeIcon} from "@fortawesome/react-fontawesome";
import { faCircleInfo, faMagnifyingGlass, faMessage, faPaperPlane, faPhone, faBell } from "@fortawesome/free-solid-svg-icons";

import FriendTile from './components/FriendTile';
import MessageTile from './components/MessageTile';
import PersonTile from './components/PersonTile';
import InvitationTile from './components/InvitationTile';
import UserInfoPanel from './components/UserInfoPanel';

import APIs from './context/ApiURL';

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
            invitationListIsDisplayed: false,
            userInfoPanelIsDiplayed: false,

            messages: [],
            messKey: 0,

            listOfUsers: [],
            findUsersStatus: "not typed",
            listOfFriends: [],
            listOfFriends_filtered: [],
            findFriendsStatus: "not found",
            listOfInvitations: []
        }
        this.handleChangeTxt = this.handleChangeTxt.bind(this);
        this.handleSwitchBtn = this.handleSwitchBtn.bind(this);
        this.addMessage = this.addMessage.bind(this);
        this.singOut = this.singOut.bind(this);
        this.displayInvationList = this.displayInvationList.bind(this);
        this.displayUserInfoPanel = this.displayUserInfoPanel.bind(this);

        this.invitationActions = this.invitationActions.bind(this);
    }

    handleChangeTxt = async (event) => {

        const { name, value } = event.target;
        await this.setState({
            [name]: value
        });

        if(name == "searchBar"){
            if(!this.state.yourChatIsActive){
                // Find friends
                this.searchPeople();
            }else{
                // Your chats
                let v_listOfFriends_filtered = this.state.listOfFriends.filter(user =>
                    user.userName.toLowerCase().includes(this.state.searchBar.toLowerCase())
                );
                await this.setState({listOfFriends_filtered: v_listOfFriends_filtered});
                
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
                listOfUsers: [],
                listOfFriends_filtered: this.state.listOfFriends
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
        setAuth('',null ,[], '');
        sessionStorage.removeItem('refreshToken');
        sessionStorage.removeItem('userInfo');
    }

    async refreshAccessToken(){
        const {setAuth, username, userID ,roles,accessToken} = this.context;


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
                await setAuth(username, userID ,roles, res.resultData);
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

    displayInvationList(){
        this.setState({
            invitationListIsDisplayed: !this.state.invitationListIsDisplayed,
            userInfoPanelIsDiplayed: false
        });
    }

    displayUserInfoPanel(){
        this.setState({
            invitationListIsDisplayed: false,
            userInfoPanelIsDiplayed: !this.state.userInfoPanelIsDiplayed
        });
    }

    async getInvitations(){
        const {userID ,accessToken} = this.context;

        //fetch
        try{
            const data = await axios.get(`${APIs.GET_INVITATIONS_URL}/${userID}`,
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

                await this.setState({listOfInvitations: res.resultData});

            }

        }catch(err){
            if (err.response && err.response.status === 401) { // Unauthorized, token expired
                await this.refreshAccessToken();
                // retry request
                await this.getInvitations();
            }else if(err.response.status === 400){
                console.error("bad host id");
            }
            else if(err.response.status === 404){
                // no new invitations
            }
            else {
                console.error(err);
            }
        }
    }

    async invitationActions(action, recipientID){
        const {userID ,accessToken} = this.context;

        const API_URL = action == "accept" ? APIs.ACCEPT_INVITE_URL : APIs.DECELINE_INVITE_URL;
        
        //fetch
        try{
            const data = await axios.post(API_URL,
                JSON.stringify({
                    senderId: recipientID,
                    recipientId: userID
                }),
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
            console.log(data);


            if(data.status == 200){
                console.log(res.title);
                console.log(res.traceId);

                // delete invitation
                await this.setState(prevState => ({
                    listOfInvitations: prevState.listOfInvitations.filter(invitation => invitation.id !== recipientID)
                }));
            }

        } catch(err){
            console.log(err);
            if (err.response && err.response.status === 401) { // Unauthorized, token expired
                await this.refreshAccessToken();
                // retry request
                await this.invitationActions(action, recipientID);
            }else if(err.response.status === 404){
                console.log(recipientID);
                console.error("Invitation or RecipientUser doesn't exist");
            }
            else {
                console.error(err);
            }
        }
    }

    async getFriends(){
        const {username, userID, accessToken} = this.context;

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

                await this.setState({
                    listOfFriends: res.resultData,
                    listOfFriends_filtered: res.resultData
                });

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

        this.getInvitations();
        this.getFriends();
    }

    

    render() {


        const { username, roles, accessToken } = this.context;

        console.log("Current context MESSAGE PAGE:", username, roles, accessToken);

        return (
            <div id='mainMessagePage'>
                {/* INVITATION LIST */}
                {
                    this.state.invitationListIsDisplayed ?
                    <div id='invitationsList'>
                        {
                            this.state.listOfInvitations.length > 0 ?
                            this.state.listOfInvitations.map(user => (
                                <InvitationTile key={user.id} id={user.id} username={user.userName} invitationAction={this.invitationActions} />
                            ))
                            :
                            <div className='infoText'>No new invitations</div>
                        }
                    </div>
                    :
                    <></>
                }
                {/* USER INFO PANEL */}
                {
                    this.state.userInfoPanelIsDiplayed ? 
                    <UserInfoPanel
                        username={this.state.username} fullname={null} email={null}/>
                    :
                    <></>
                }
                

                {/* MENU BAR */}
                <div id='menuBar'>
                    <div id='logoInMenu'/>
                    <div id='profileBox'>
                        <div className='bellWrapper' onClick={this.displayInvationList}>
                            <FontAwesomeIcon icon={faBell} className='friendBarIcon' />
                            {
                                this.state.listOfInvitations.length > 0 ?
                                <div className='notificationBadge'>{this.state.listOfInvitations.length}</div>
                                :
                                <></>
                            }
                        </div>
                        
                        <button className='btn2' onClick={this.singOut}>Sing out</button>

                        <div className='profileInfoWrapper' onClick={this.displayUserInfoPanel}>
                            {this.state.username}
                            <div className='profileIcon'/>
                        </div>
                    </div>
                    
                </div>

                {/* SEARCH BAR */}
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

                {/* FRIEND OR PEOPLE LIST */}
                <div id='friendsList'>
                    {
                    this.state.yourChatIsActive ?

                    (
                        this.state.listOfFriends.length > 0 ?
                            this.state.listOfFriends_filtered.length > 0 ?
                                this.state.listOfFriends_filtered.map(user => (
                                    <FriendTile key={user.id} username={user.userName} author={"You"} mess={"hello my friend!"} />
                                ))
                            :
                            <div className='infoText'>There are no friends named {this.state.searchBar} ...</div>
                        :
                        <div className='infoText'>You have zero friends</div>
                    )

                    :
                    (
                        this.state.findUsersStatus == "not typed" ? 
                        (<div className='infoText'>Please type 3 or more characters</div>)
                        :
                        (this.state.findUsersStatus == "not found" ?
                            (<div className='infoText'>There are no users named {this.state.searchBar} ...</div>)
                            :
                            (
                                this.state.listOfUsers.map(user => (
                                    <PersonTile key={user.id} username={user.userName} userId={user.id} refreshToken={this.refreshAccessToken}/>
                                ))
                            )
                        )
                    )
                    }
                </div>

                {/* FRIEND BAR */}
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

                {/* MESSAGE BOX */}
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

                {/* SEND MESSAGE BOX */}
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