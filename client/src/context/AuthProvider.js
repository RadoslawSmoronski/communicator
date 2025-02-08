import React, { createContext, Component } from 'react';

export const AuthContext = createContext();

class AuthProvider extends Component {
    constructor(props) {
        super(props);
        this.state = {
          username: '',
          userID: null,
          roles: [],
          accessToken: ''
        };
        this.setAuth = this.setAuth.bind(this);
      }

    setAuth(username,userID ,roles, accessToken){
        console.log(username,userID ,roles, accessToken)
        this.setState({username,userID ,roles, accessToken});
    }

    render() {
        return (
            <AuthContext.Provider value={{
                username: this.state.username,
                userID: this.state.userID, 
                roles: this.state.roles,
                accessToken: this.state.accessToken,
                setAuth: this.setAuth
            }}>
                {this.props.children}
            </AuthContext.Provider>
        );
    }
}

export default AuthProvider;