import React from 'react';

import {
    Route,
} from 'react-router-dom';

import { ROUTES } from "./routePaths";

import RequireAuth from '../../features/auth/context/RequireAuth';

import MessagePage from '../../pages/private/MessagePage'
import MessageWrapper from "../../pages/private/MessageWrapper"

const PrivateRoutes = () => {
    return (
        <>
            {/* protected routes */}
            <Route path="/">
                <Route element={<RequireAuth allowedRoles={['user']} />}>
                    <Route path={ROUTES.MESSAGE} element={<MessageWrapper />} />
                </Route>
            </Route>
        </>
    );
};

export default PrivateRoutes;