import React, { useContext } from 'react';
import { Navigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';

const RedirectIfAuth = ({ children }) => {
  const { userId, loading } = useContext(AuthContext);

  if (loading) return null;

  return userId ? <Navigate to="/message" replace /> : children;
};

export default RedirectIfAuth;
