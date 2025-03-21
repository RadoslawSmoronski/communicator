import React from 'react';
import { Outlet, Navigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';
import axios from '../api/axios';

import APIs from '../context/ApiURL';

class RequireAuth extends React.Component {
  static contextType = AuthContext; 

  constructor(props) {
    super(props);
    this.state = {
      loaded: false
    }
  }

  async componentDidMount(){
    const {accessToken, refreshAccessToken } = this.context;
    if(accessToken == ""){
      //no accessToken
      await refreshAccessToken();
      await this.setState({loaded: true});
    }else{
      //there is accessToken
      await this.setState({loaded: true});
    }
  }

  render() {

    if(this.state.loaded){
      const { allowedRoles, location } = this.props; // Pobieramy allowedRoles i location z props
      const { username, roles, accessToken, setAuth } = this.context; // Dostęp do auth z kontekstu
      console.log("RequireAuth: ", username, roles, accessToken );
      console.log(allowedRoles);
      //if (roles.some(role => allowedRoles.includes(role))) {
      if(accessToken){
          //authorized
          console.log("zajebiscie");
        return <Outlet />;}
      // } else if (username) {
      //   // unauthorized
      //   return <Navigate to="/unauthorized" state={{ from: location }} replace />;
      //}
      else {
        // not logged in
        return <Navigate to="/login" state={{ from: location }} replace />;
    }
    }

  }
}

export default RequireAuth;
