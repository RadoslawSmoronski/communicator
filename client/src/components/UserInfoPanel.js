import React, { Component } from 'react';
import { Link } from 'react-router-dom';

class UserInfoPanel extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className='userInfoPanel'>
                <div className='imageAndNameWrapperIP'>
                    <div className='friendTileIcon' 
                        style={{width: '60px', height: '60px'}}
                    />
                    <div className='friendTileUserName' style={{
                        width: '65%',
                        display: 'flex',
                        alignItems: 'center',
                        paddingLeft: '15px'
                    }}>
                        {this.props.username}
                    </div>
                </div>
                <div className='friendTile userInfoTile'>
                   Fullname: {this.props.fullname == null ? 'not given' : this.props.fullname}
                </div>
                <div className='friendTile userInfoTile'>
                    Email: {this.props.email == null ? 'not given' : this.props.email}
                </div>
                
                <Link to="/editprofile" style={{
                    textAlign:'center',
                    marginTop: '5px'
                }}>Edit the profile</Link>
            </div>
        );
    }
}

export default UserInfoPanel;