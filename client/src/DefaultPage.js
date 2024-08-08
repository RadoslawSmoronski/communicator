import React from "react";

class DefaultPage extends React.Component{
    render(){
        
        return (<>
        <div className="loginPanel panel404">
            <h1 className="errorFont">Bad Page 404</h1>
            <h1>Oj... Wygląda na to, że nie ma takiej strony</h1><br/><br/>
            <div>Powrót do logowania</div>
            <a className="link" href="/login">Zaloguj się</a>
            </div>
                </>
        )
    }
}
export default DefaultPage;