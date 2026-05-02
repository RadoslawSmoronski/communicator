import React from 'react';

import { BrowserRouter, Routes } from "react-router-dom";
import AuthProvider from "../providers/AuthProvider";
import UserProvider from '../providers/UserProvider';
import PublicRoutes from "./PublicRoutes";
import PrivateRoutes from "./PrivateRoutes";

const Router = () => {
  return (
    <BrowserRouter>
      <AuthProvider>
        <UserProvider>
            <Routes>
              {PublicRoutes()}
              {PrivateRoutes()}
            </Routes>
        </UserProvider>
    </AuthProvider>
    </BrowserRouter>
  );
};

export default Router;
