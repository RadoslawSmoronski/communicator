import React, { Component } from 'react';

import { AuthContext } from '../context/AuthProvider';
import axios from '../api/axios';

import APIs from '../context/ApiURL';

class InvitationTile extends Component {
    constructor(props) {
        super(props);

        this.acceptInvitation = this.acceptInvitation.bind(this);
        this.rejectInvitation = this.rejectInvitation.bind(this);
    }
    acceptInvitation = () =>{
        this.props.invitationAction('accept', this.props.id);
    }
    rejectInvitation = () =>{
        this.props.invitationAction('reject', this.props.id);
    }

    render() {
        return (
            <div className='friendTile invitationTile' >
                <div className='friendTileIcon'/>
                <div className='friendTileWrapper invitationWrapper'>
                    <div className='friendTileUserName invitationTileText'>
                        <span style={{color: '#bf7210'}}> {this.props.username}</span> sent you an invitation
                    </div>
                    <div className='personBtnWrapper'>
                        <div className='btnPerson btnInvitation' onClick={this.acceptInvitation} >Accept</div>
                        <div className='btnPerson btnInvitation' onClick={this.rejectInvitation} style={{backgroundColor: '#bf2d10'}}>Reject</div>
                    </div>
                </div>
            </div>
        );
    }
}

export default InvitationTile;