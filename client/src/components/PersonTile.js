import React, { Component } from 'react';

import { AuthContext } from '../context/AuthProvider';
import axios from '../api/axios';

import APIs from '../context/ApiURL';

class PersonTile extends Component {
    static contextType = AuthContext;

    constructor(props) {
        super(props);
        this.state = {
            sendBtnIsActive: true
        }

        this.sendInvitation = this.sendInvitation.bind(this);
    }

    async sendInvitation(){

        const { username, userID, accessToken} = this.context;
        const recipientId = this.props.userId;

        console.log("My id: " + userID + " yours id: " + recipientId);
        //fetch
        try{
            const data = await axios.post(APIs.SEND_INVITE_URL,
                JSON.stringify({
                    senderId: userID,
                    recipientId: recipientId
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

                // disable the button
                await this.setState({sendBtnIsActive: false});
            }

        } catch(err){
            console.log(err);
            if (err.response && err.response.status === 401) { // Unauthorized, token expired
                await this.props.refreshToken();
                // retry request
                await this.sendInvitation();
            }else if (err.response.status === 409) { // An invitation has already exist
                console.error("An invitation has already exist");
                // disable the button
                await this.setState({sendBtnIsActive: false});
            }else if(err.response.status === 400){
                console.error("Validation error");
            }
            else {
                console.error(err);
            }
        }
    }

    render() {
        return (
            <div className='friendTile' >
                <div className='friendTileIcon'/>
                <div className='friendTileWrapper'>
                    <div className='friendTileUserName'>{this.props.username}</div>
                    <div className='personBtnWrapper'>
                        {
                            this.state.sendBtnIsActive ? 
                            <div className='btnPerson' onClick={this.sendInvitation}>Add friend</div>
                            :
                            <div className='btnPerson btnDisabled'>invitation sent</div>
                        }
                        <div className='btnPerson' style={{backgroundColor: '#bf2d10'}}>Block</div>
                    </div>
                </div>
            </div>
        );
    }
}

export default PersonTile;