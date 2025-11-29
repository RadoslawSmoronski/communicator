import React, { useState, useEffect, useContext, useRef } from "react";
import { Link, useLocation } from 'react-router-dom';
import axios from "../../api/axios";

import APIs from "../../api/ApiURL";
import useRawQueryParam from "../../shared/hooks/useRawQueryParam";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleXmark, faCircleCheck, faChampagneGlasses } from "@fortawesome/free-solid-svg-icons";

const ConfirmAccountPage = () => {
    const location = useLocation();
    // const queryParams = new URLSearchParams(location.search);
    // const userId = queryParams.get("userId");
    // const token = queryParams.get("token");

    const [feedbackMess, setFeedbackMess] = useState(<>Wait ...</>);
    const [isSuccess, setIsSuccess] = useState(null);

    const userId = useRawQueryParam("userId");
    const token = useRawQueryParam("token");

    const errorMark = <FontAwesomeIcon icon={faCircleXmark} />;

    const confirmAccount = async () => {

        if (userId == null || token == null) {
            setIsSuccess(false);
            setFeedbackMess(<>{errorMark} The address contains incorrect data.</>);
            return;
        }

        try {
            const response = await axios.post(APIs.CONFIRM_ACCOUNT,
                JSON.stringify({
                    userId: userId,
                    confirmationToken: token
                }),
                {
                    headers: {
                        'Content-Type': 'application/json'
                    }
                }
            );

            if (response.status === 200) {
                setIsSuccess(true);
                setFeedbackMess(<><FontAwesomeIcon icon={faCircleCheck} /> Your account has been successfully activated</>);
            }

        } catch (err) {
            setIsSuccess(false);
            if (err.response?.status === 400) {
                setFeedbackMess(<>{errorMark} The address contains incorrect data.</>);
            } else {
                console.error(err);
                setFeedbackMess(<>{errorMark} An error occurred while activating the account.</>);
            }
        }
    };


    useEffect(() => {
        console.log("userId: " + userId);
        console.log("token: " + token)

        confirmAccount();
    }, []);


    return (
        <div id="mainregisterPage" style={{ justifyContent: 'right' }}>
            <div id="logo" className="confirmAccount" />
            <div className="confirmAccountBody">

                {isSuccess &&
                    <>
                        <FontAwesomeIcon icon={faChampagneGlasses} className="confirmAccount icon" />
                        <div className="confirmAccount thanks">THANKS FOR JOINING</div>
                        <div className="confirmAccount mainText">Your registration is complete.</div>
                    </>
                }
                <div className={isSuccess != null ? (isSuccess ? "successText" : "errorText") : ""}>{feedbackMess}</div>

                <Link to="/login" className="btn2 btn404">Back to login page</Link>


            </div>
        </div>
    )

}
export default ConfirmAccountPage;