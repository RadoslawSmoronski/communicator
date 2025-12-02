import React, { useContext } from 'react';
import { Navigate } from 'react-router-dom';
import { ROUTES } from '../../../app/router/routePaths';

import { AuthContext } from '../../../app/providers/AuthProvider';
import { UserContext } from '../../../app/providers/UserProvider';

const RedirectIfAuth = ({ children }) => {
  const { accessToken, loading: authLoading } = useContext(AuthContext);
  const { user, loading: userLoading } = useContext(UserContext);

  if (authLoading || userLoading) return null;

  return (user && accessToken) ? <Navigate to={ROUTES.MESSAGE} replace /> : children;
};

export default RedirectIfAuth;
