import React from 'react';

import { BrowserRouter, Routes } from "react-router-dom";
import AuthProvider from "../providers/AuthProvider";
import PublicRoutes from "./PublicRoutes";
import PrivateRoutes from "./PrivateRoutes";

const Router = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          {PublicRoutes()}
          {PrivateRoutes()}
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};

export default Router;
