import React from 'react';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck } from "@fortawesome/free-solid-svg-icons";

const MessageTile = ({ time, mess, yours, isLastReadByFriend }) => {
    const dateNow = new Date();
    const dateOfMessage = new Date(time);
    const hours = dateOfMessage.getHours();
    const minutes = dateOfMessage.getMinutes();

    let day = '';
    let month = '';
    let year = '';

    if (
        dateNow.getDate() !== dateOfMessage.getDate() ||
        dateNow.getMonth() !== dateOfMessage.getMonth() ||
        dateNow.getFullYear() !== dateOfMessage.getFullYear()
    ) {
        day = dateOfMessage.getDate();
        month = dateOfMessage.toLocaleString('en-US', { month: 'short' });

        if (dateNow.getFullYear() !== dateOfMessage.getFullYear()) {
            year = dateOfMessage.getFullYear();
        }
    }

    const formattedTime = `${day} ${month} ${year} ${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}`;

    return yours ? (
        <div className="messageTileRight">
            <div className="messageTileWrapperRight">
                <div className="messageCloud">{mess}</div>
                <div className="messageTileTime">{formattedTime}</div>
            </div>
            {isLastReadByFriend &&
                <FontAwesomeIcon icon={faCheck} className='messageCloudCheckMark' />
            }

        </div>
    ) : (
        <div className="messageTileLeft">
            <div className="messageTileWrapperLeft">
                <div className="messageCloud messageCloudLeft">{mess}</div>
                <div className="messageTileTime">{formattedTime}</div>
            </div>
            {isLastReadByFriend &&
                <FontAwesomeIcon icon={faCheck} className='messageCloudCheckMark' />
            }
        </div>
    );
};

export default MessageTile;
