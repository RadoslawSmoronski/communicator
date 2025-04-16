import React, { Component } from 'react';

class FriendTile extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        let dateNow = new Date();
        let dateOfMessage = new Date(this.props.messTimestamp);
        let hours = dateOfMessage.getHours().toString().padStart(2, '0');
        let minutes = dateOfMessage.getMinutes().toString().padStart(2, '0');

        let day = '';
        let month = '';
        let year = '';

        let dateOrTimeToDisplay = '';

        if(dateNow.getDate() != dateOfMessage.getDate() ||
            dateNow.getMonth() != dateOfMessage.getMonth() || 
            dateNow.getFullYear() != dateOfMessage.getFullYear()){

            day = dateOfMessage.getDate();
            month = dateOfMessage.toLocaleString('en-US', { month: 'short' });

            dateOrTimeToDisplay = day + ' ' + month;

            if(dateNow.getFullYear() != dateOfMessage.getFullYear()){
                year = dateOfMessage.getFullYear();
                dateOrTimeToDisplay += ' ' + year;
            }
        }else{
            dateOrTimeToDisplay = hours + ':' + minutes;
        }

        let mess = this.props.mess;

        if(mess?.length >= 20){
            mess = mess.slice(0,17) + "...";
        }

        console.log(`Nowa wiad dla ${this.props.username}: ${this.props.newMessageNotify}`)

        return (
            <div className={this.props.selected ? 'friendTile selectedChat' : 'friendTile'} onClick={this.props.onClick} >
                <div className='friendTileIcon'/>
                <div className='friendTileWrapper'>
                    <div className='friendTileUserName'>{this.props.username}</div>
                    <div className='friendTileMess'>{this.props.author} {mess} {dateOrTimeToDisplay}</div>
                    {
                        this.props.newMessageNotify &&
                        <div className='newMessageNotification'/>
                    }
                    
                </div>
            </div>
        );
    }
}

export default FriendTile;