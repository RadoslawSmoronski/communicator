import React from 'react';

import {
    Route,
    Navigate
} from 'react-router-dom';

import { ROUTES } from "./routePaths";

import RedirectIfAuth from '../../features/auth/context/RedirectIfAuth';

import LoginPage from '../../pages/public/LoginPage';
import RegisterPage from '../../pages/public/RegisterPage';
import DefaultPage from '../../pages/public/DefaultPage';
import ConfirmAccountPage from  '../../pages/public/ConfirmAccountPage';
import LoggingHelpPage from '../../pages/public/LoggingHelpPage';
import ForgotPasswordPage from '../../pages/public/ForgotPasswordPage';
import ResetPasswordPage from '../../pages/public/ResetPasswordPage';


const PublicRoutes = () => {
    return (
        <>
            {/* public routes */}
            <Route path={ROUTES.HOME_REDIRECT} element={<Navigate replace to={ROUTES.LOGIN}/>} />
            <Route path={ROUTES.REGISTER} element={<RegisterPage />} />
            <Route path={ROUTES.CONFIRM} element={<ConfirmAccountPage />} />
            <Route path={ROUTES.HELP} element={<LoggingHelpPage />} />
            <Route path={ROUTES.FORGOT_PASSWORD} element={<ForgotPasswordPage />} />
            <Route path={ROUTES.RESET_PASSWORD} element={<ResetPasswordPage />} />
            <Route path={ROUTES.LOGIN} element={
                <RedirectIfAuth>
                    <LoginPage />
                </RedirectIfAuth>
            } />

            {/* 404 page */}
            <Route path={ROUTES.NOT_FOUND} element={<DefaultPage />} />

        </>
    );
};

export default PublicRoutes;