import React, { Component } from 'react';

class FriendTile extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className='friendTile'>
                <div className='friendTileIcon'/>
                <div className='friendTileWrapper'>
                    <div className='friendTileUserName'>{this.props.username}</div>
                    <div className='friendTileMess'>{this.props.author} : {this.props.mess}</div>
                </div>
            </div>
        );
    }
}

export default FriendTile;