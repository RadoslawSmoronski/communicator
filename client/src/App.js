import React from "react";
import {
    BrowserRouter as Router,
    Routes,
    Route,
    Navigate
  } from 'react-router-dom';
import LoginPage from "./LoginPage";
import RegisterPage from "./RegisterPage";
import DefaultPage from "./DefaultPage";

class App extends React.Component{
    render(){
        
        return (
            <Router>
                
                {/* <Link to="/">Login</Link>
                <Link to="/login">Login</Link> */}

                <Routes>
                    <Route path="/" element={<Navigate replace to="/login" />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    {/* 404 page */}
                    <Route path="*" element={<DefaultPage />} />
                </Routes>
            </Router>
        )
    }
}
export default App;