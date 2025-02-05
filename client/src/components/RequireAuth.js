import React from 'react';
import { Outlet, Navigate } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';
import axios from '../api/axios';

const REFRESH_TOKEN_URL = "/api/user/refreshAccessToken"

class RequireAuth extends React.Component {
  static contextType = AuthContext; 

  constructor(props) {
    super(props);
    this.state = {
      loaded: false
    }
  }

  async refreshAccessToken(){
    const {setAuth, username, roles,accessToken} = this.context;


    const refreshToken = sessionStorage.getItem('refreshToken');
    const userInfo = JSON.parse(sessionStorage.getItem('userInfo'));

    //fetch
    try{
        const data = await axios.post(REFRESH_TOKEN_URL, {
              refreshToken: refreshToken
          }, // Pass as a plain object
          {
            withCredentials: true, //pass a http only cookie
            headers: {
              Authorization: `Bearer ${accessToken}`,
              'Content-Type': 'application/json'
            }
          }
        );

        let res = data.data;
        //is ok
        if(data.status == 200){
            console.log("SUKCES: ", res.title);
            await setAuth(userInfo.name, userInfo.rules, res.resultData);
        }

    } catch(err){
        console.log("Error: Can't refresh token: ", err);
    }
  }

  async componentDidMount(){
    const {accessToken } = this.context;
    if(accessToken == ""){
      //no accessToken
      await this.refreshAccessToken();
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
