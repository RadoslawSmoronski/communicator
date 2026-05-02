import React, { useContext} from 'react';
import { Outlet, Navigate, useLocation } from 'react-router-dom';
import { ROUTES } from '../../../app/router/routePaths';

import { AuthContext } from '../../../app/providers/AuthProvider';
import { UserContext } from '../../../app/providers/UserProvider';

const RequireAuth = ({ allowedRoles }) => {
  const { accessToken, loading: authLoading } = useContext(AuthContext);
  const { user, loading: userLoading } = useContext(UserContext);
  const location = useLocation();

  if (authLoading || userLoading) return null; // waiting for data ...

  // 1. If user == null → is not logged
  // or error refresh token
  if (!user  || !accessToken) {
    return <Navigate to={ROUTES.LOGIN} state={{ from: location }} replace />;
  }

  // 2. Logged, but unauthorized
  if (!allowedRoles.includes(user.role)) {
    return <Navigate to={ROUTES.HOME_REDIRECT} state={{ from: location }} replace />;
  }

  // 3. OK
  return <Outlet />;
};

export default RequireAuth;