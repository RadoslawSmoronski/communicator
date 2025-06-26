import React from 'react';

const FriendTile = ({ messTimestamp, mess, username, author, selected, onClick, newMessageNotify }) => {
    const dateNow = new Date();
    const dateOfMessage = new Date(messTimestamp);
    const hours = dateOfMessage.getHours().toString().padStart(2, '0');
    const minutes = dateOfMessage.getMinutes().toString().padStart(2, '0');

    let dateOrTimeToDisplay = '';

    if (
        dateNow.getDate() !== dateOfMessage.getDate() ||
        dateNow.getMonth() !== dateOfMessage.getMonth() ||
        dateNow.getFullYear() !== dateOfMessage.getFullYear()
    ) {
        const day = dateOfMessage.getDate();
        const month = dateOfMessage.toLocaleString('en-US', { month: 'short' });
        dateOrTimeToDisplay = `${day} ${month}`;

        if (dateNow.getFullYear() !== dateOfMessage.getFullYear()) {
            const year = dateOfMessage.getFullYear();
            dateOrTimeToDisplay += ` ${year}`;
        }
    } else {
        dateOrTimeToDisplay = `${hours}:${minutes}`;
    }

    let truncatedMess = mess;
    if (truncatedMess?.length >= 20) {
        truncatedMess = truncatedMess.slice(0, 17) + '...';
    }

    return (
        <div className={selected ? 'friendTile selectedChat' : 'friendTile'} onClick={onClick}>
            <div className="friendTileIcon" />
            <div className="friendTileWrapper">
                <div className="friendTileUserName">{username}</div>
                <div className="friendTileMess">
                    {author} {truncatedMess} {dateOrTimeToDisplay}
                </div>
                {newMessageNotify && <div className="newMessageNotification" />}
            </div>
        </div>
    );
};

export default FriendTile;
