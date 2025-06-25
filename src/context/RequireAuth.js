import React, { useContext, useEffect, useState } from 'react';
import { Outlet, Navigate, useLocation } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';

const RequireAuth = ({ allowedRoles }) => {
  const { login, role, userId, loading } = useContext(AuthContext);
  const location = useLocation();

  if (loading) return null; // wczytanie Session storage...

  // sprawdzanie rule:
  if (allowedRoles.includes(role)) {
    return <Outlet />;
  } else if (userId) {
    // unauthorized
    return <Navigate to="/home" state={{ from: location }} replace />;
  } else {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
};

export default RequireAuth;