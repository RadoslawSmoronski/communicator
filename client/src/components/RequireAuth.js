import React from 'react';
import { Outlet, Navigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';


class RequireAuth extends React.Component {
  static contextType = AuthContext; 

  render() {
    const { allowedRoles, location } = this.props; // Pobieramy allowedRoles i location z props
    const { username, roles, accessToken, setAuth } = this.context; // Dostęp do auth z kontekstu
    console.log(username, roles, accessToken );
    console.log(allowedRoles);
    if (roles.some(role => allowedRoles.includes(role))) {
        //authorized
        console.log("zajebiscie");
      return <Outlet />;
    } else if (username) {
      // unauthorized
      return <Navigate to="/unauthorized" state={{ from: location }} replace />;
    } else {
      // not logged in
      return <Navigate to="/login" state={{ from: location }} replace />;
    }
  }
}

export default RequireAuth;
