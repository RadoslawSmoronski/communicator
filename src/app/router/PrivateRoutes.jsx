import React from 'react';

import {
    Route,
} from 'react-router-dom';

import { ROUTES } from "./routePaths";

import RequireAuth from '../../features/auth/context/RequireAuth';

import Layout from '../../components/Layout';
import MessagePage from '../../pages/private/MessagePage'


const PrivateRoutes = () => {
    return (
        <>
            {/* protected routes */}
            <Route path="/" element={<Layout />}>
                <Route element={<RequireAuth allowedRoles={['user']} />}>
                    <Route path={ROUTES.MESSAGE} element={<MessagePage />} />
                </Route>
            </Route>
        </>
    );
};

export default PrivateRoutes;