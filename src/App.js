import React from "react";
import {
    BrowserRouter as Router,
    Routes,
    Route,
    Navigate
} from 'react-router-dom';

import RequireAuth from "./context/RequireAuth";
import AuthProvider from "./context/AuthProvider";
import RedirectIfAuth from "./context/RedirectIfAuth";
import Layout from "./components/Layout";

import LoginPage from "./pages/public/LoginPage";
import RegisterPage from "./pages/public/RegisterPage";
import DefaultPage from "./pages/public/DefaultPage";
import MessagePage from "./pages/private/MessagePage";

const App = () => {
    console.log("renderuje app");

    return (
        <AuthProvider>
            <Router>
                <Routes>
                    {/* public routes */}
                    <Route path="/" element={<Navigate replace to="/login" />} />
                    <Route path="/register" element={<RegisterPage />} />
                    <Route path="/login" element={
                        <RedirectIfAuth>
                            <LoginPage />
                        </RedirectIfAuth>
                    } />

                    {/* protected routes */}
                    <Route path="/" element={<Layout />}>
                        <Route element={<RequireAuth allowedRoles={['user']} />}>
                            <Route path="/message" element={<MessagePage />} />
                            {/* <Route path="/editprofile" element={<EditProfile />} /> */}
                        </Route>
                    </Route>

                    {/* 404 page */}
                    <Route path="*" element={<DefaultPage />} />

                </Routes>
            </Router>
        </AuthProvider>
    );
}

export default App;