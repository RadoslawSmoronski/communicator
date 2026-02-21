import React from "react";
import { Link } from 'react-router-dom';
import Spinner from "../../shared/components/Spinner";

import { ROUTES } from "../../app/router/routePaths";
import useRawQueryParam from "../../shared/hooks/useRawQueryParam";
import { useConfirmAccount } from "../../features/auth/hooks/useConfirmAccount";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCircleXmark, faCircleCheck, faChampagneGlasses } from "@fortawesome/free-solid-svg-icons";

const ConfirmAccountPage = () => {
    const userId = useRawQueryParam("userId");
    const token = useRawQueryParam("token");

    const { loading, success, message } = useConfirmAccount(userId, token);

    const icon = success
        ? <FontAwesomeIcon icon={faCircleCheck} />
        : <FontAwesomeIcon icon={faCircleXmark} />;

    return (
        <div id="mainFormPage">
            <div id="logo" className="confirmAccount" />
            <div className="confirmAccountBody">

                {success &&
                    <>
                        <FontAwesomeIcon icon={faChampagneGlasses} className="confirmAccount icon" />
                        <div className="confirmAccount thanks">THANKS FOR JOINING</div>
                        <div className="confirmAccount mainText">Your registration is complete.</div>
                    </>
                }
                <div className={success != null ? (success ? "successText" : "errorText") : ""}>
                    {loading
                        ? <Spinner size="3em" containerPadding="2px" />
                        : <>{icon} {message}</>
                    }
                </div>

                <Link to={ROUTES.LOGIN} className="btn2 btn404">Back to login page</Link>


            </div>
        </div>
    )

}
export default ConfirmAccountPage;