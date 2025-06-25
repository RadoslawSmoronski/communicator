import React from "react";
import { Link } from 'react-router-dom';

class DefaultPage extends React.Component{
    render(){
        
        return (
        <div id="mainregisterPage" style={{justifyContent: 'right'}}>
            <div className="loginPanel panel404">
                <h1 className="errorFont">404</h1>
                <div>Page not found</div>
                <Link to="/login" className="btn2 btn404">Back to login page</Link>
            </div>

        </div>
        )
    }
}
export default DefaultPage;