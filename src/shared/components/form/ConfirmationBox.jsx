import React, { useContext } from 'react'
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleInfo } from "@fortawesome/free-solid-svg-icons";
import { ChatUIContext } from '../../../features/messages/providers/ChatUIProvider';

const ConfirmationBox = ({ confirmFunc, text }) => {
    const { displayConfirmationBox, setDisplayConfimationBox } = useContext(ChatUIContext);

    const closePanel = () => {
        setDisplayConfimationBox(false);
    }

    return (
        <>
            {
                displayConfirmationBox &&
                <div className="offFocusBK" onClick={() => closePanel()}>
                    <div
                        className="confirmationBox"
                        onClick={(e) => e.stopPropagation()}
                    >
                        <div className="closeBtnBox confBox">
                            <div className="closeBtn confBox" onClick={() => closePanel()} />
                        </div>
                        <div className='confirmationBoxText'>
                            <FontAwesomeIcon icon={faCircleInfo} /> <br />{text}
                        </div>

                        <div className='editPanelInputWrapper rows'>
                            <button className='confirmationBoxBtn' onClick={() => confirmFunc()}>OK</button>
                            <button className='confirmationBoxBtn' onClick={() => closePanel()}>Cancel</button>
                        </div>
                    </div>
                </div>
            }
        </>

    )
}

export default ConfirmationBox