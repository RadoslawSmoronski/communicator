import React from "react";
import {
    BrowserRouter as Router,
    Routes,
    Route,
    Navigate
  } from 'react-router-dom';
import AuthProvider from "./context/AuthProvider";
import LoginPage from "./LoginPage";
import RegisterPage from "./RegisterPage";
import DefaultPage from "./DefaultPage";
import MessagePage from "./MessagePage";
import RequireAuth from "./components/RequireAuth";


class App extends React.Component{
    render(){
        console.log("renderuje app");
        return (
            <AuthProvider>
            <Router>

                <Routes>
                    {/* public routes */}
                    <Route path="/" element={<Navigate replace to="/login" />} />
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/register" element={<RegisterPage />} />
                    {/* <Route path="/message" element={<MessagePage/>}/> */}
                    
                    {/* protected routes */}
                    <Route element={<RequireAuth allowedRoles={['user']}/>}>
                        <Route path="/message" element={<MessagePage/>}/>
                    </Route>


                    {/* 404 page */}
                    <Route path="*" element={<DefaultPage />} />



                </Routes>
            </Router>
            </AuthProvider>
        )
    }
}
export default App;