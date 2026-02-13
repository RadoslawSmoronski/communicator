import React, { useContext } from 'react'
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleInfo } from "@fortawesome/free-solid-svg-icons";
import { ChatUIContext } from '../../../features/messages/providers/ChatUIProvider';

const ConfirmationBox = ({ confirmFunc, text }) => {
    const { displayConfirmationBox, hideConfirmationBox } = useContext(ChatUIContext);


    return (
        <>
            {
                displayConfirmationBox &&
                <div className="offFocusBK" onClick={() => hideConfirmationBox()}>
                    <div
                        className="confirmationBox"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <div className="closeBtnBox confBox">
                            <div className="closeBtn confBox" onClick={() => hideConfirmationBox()} />
                        </div>
                        <div className='confirmationBoxText'>
                            <FontAwesomeIcon icon={faCircleInfo} /> <br />{text}
                        </div>

                        <div className='editPanelInputWrapper rows'>
                            <button className='confirmationBoxBtn' onClick={() => confirmFunc()}>OK</button>
                            <button className='confirmationBoxBtn' onClick={() => hideConfirmationBox()}>Cancel</button>
                        </div>
                    </div>
                </div>
            }
        </>

    )
}

export default ConfirmationBox