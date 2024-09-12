import React from "react";
import { Link } from 'react-router-dom';

class DefaultPage extends React.Component{
    render(){
        
        return (<>
        <div className="loginPanel panel404">
            <h1 className="errorFont">Bad Page 404</h1>
            <h1>Oj... Wygląda na to, że nie ma takiej strony</h1><br/><br/>
            <div>Powrót do logowania</div>
            <Link to="/login">Zaloguj się</Link>
            </div>
                </>
        )
    }
}
export default DefaultPage;