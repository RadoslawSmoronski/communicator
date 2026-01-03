import React from 'react'

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleInfo } from "@fortawesome/free-solid-svg-icons";

const FeedbackText = ({ successText, failText}) => {
    return (
        <>
            {
                successText ?
                    <div className="editProfileBtnInfo success">
                        < FontAwesomeIcon icon={faCircleInfo} /> {successText}
                    </div >
                    :
                    failText &&
                    <div className="editProfileBtnInfo fail">
                        <FontAwesomeIcon icon={faCircleInfo} /> {failText}
                    </div>
            }
        </>
    )
}

export default FeedbackText