import React, { createContext, Component } from 'react';

export const AuthContext = createContext();

class AuthProvider extends Component {
    constructor(props) {
        super(props);
        this.state = {
          username: '',
          roles: [],
          accessToken: ''
        };
        this.setAuth = this.setAuth.bind(this);
      }

    setAuth(username, roles, accessToken){
        console.log(username, roles, accessToken)
        this.setState({username, roles, accessToken});
    }

    render() {
        return (
            <AuthContext.Provider value={{
                username: this.state.username, 
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