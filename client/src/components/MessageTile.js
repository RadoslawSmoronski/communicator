import React, { Component } from 'react';

class MessageTile extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        let dateNow = new Date();
        let dateOfMessage = new Date(this.props.time);
        let hours = dateOfMessage.getHours();
        let minutes = dateOfMessage.getMinutes();
        let day = '';
        let month = '';
        let year = '';

        if(dateNow.getDate() != dateOfMessage.getDate() ||
            dateNow.getMonth() != dateOfMessage.getMonth() || 
            dateNow.getFullYear() != dateOfMessage.getFullYear()){

            day = dateOfMessage.getDate();
            month = dateOfMessage.toLocaleString('en-US', { month: 'short' });

            if(dateNow.getFullYear() != dateOfMessage.getFullYear()){
                year = dateOfMessage.getFullYear();
            }
        }
        

        return (
            this.props.yours ?
            <div className="messageTileRight">
            <div className="messageTileWrapperRight">
                <div className="messageCloud">
                    {this.props.mess}
                </div>
                <div className='messageTileTime'>
                    {`${day} ${month} ${year} ${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`}
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
                {`${day} ${month} ${year} ${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`}
            </div>
        </div>
        </div>


        );
    }
}

export default MessageTile;