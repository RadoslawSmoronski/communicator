import React, { Component } from 'react';

class MessageTile extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            this.props.yours ?
            <div className="messageTileRight">
            <div className="messageTileWrapperRight">
                <div className="messageCloud">
                    {this.props.mess}
                </div>
                <div className='messageTileTime'>
                    {this.props.time}
                </div>
            </div>
            </div>
        :
        <div className="messageTileLeft">
        <div className="messageTileWrapperLeft">
            <div className="messageCloud messageCloudLeft">
                {this.props.mess}
            </div>
            <div className='messageTileTime'>
                {this.props.time}
            </div>
        </div>
        </div>


        );
    }
}

export default MessageTile;