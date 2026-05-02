import React from "react";
import { Link } from 'react-router-dom';

const DefaultPage = () => {

    return (
        <div id="mainFormPage">
            <div className="loginPanel panel404">
                <h1 className="errorFont">404</h1>
                <div>Page not found</div>
                <Link to="/login" className="btn2 btn404">Back to login page</Link>
            </div>

        </div>
    )

}
export default DefaultPage;