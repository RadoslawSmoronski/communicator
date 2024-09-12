import React, { Component } from 'react';

class PersonTile extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <div className='friendTile'>
                <div className='friendTileIcon'/>
                <div className='friendTileWrapper'>
                    <div className='friendTileUserName'>{this.props.username}</div>
                </div>
            </div>
        );
    }
}

export default PersonTile;