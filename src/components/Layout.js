import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../context/AuthProvider';

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBell } from "@fortawesome/free-solid-svg-icons";

import axios from "../api/axios";
import APIs from "../api/ApiURL";

import InvitationTile from './tiles/InvitationTile';
import UserInfoPanel from './UserInfoPanel';

const Layout = () => {
    const { username, userId, accessToken, refreshAccessToken, setAuth } = useContext(AuthContext);
    const location = useLocation();
    const currentPath = location.pathname;

    const [friend, setFriend] = useState({
        invitations: []
    });

    const [display, setDisplay] = useState({
        invitationList: false,
        userInfoPanel: false,
    });

    // Invitation list
    // Displays / hides invitation list
    const displayInvitationList = () => {
        setDisplay(prev => ({
            ...prev,
            invitationList: !prev.invitationList,
            userInfoPanel: false,
        }));
    };

    // User info panel
    // Displays / hides user info panel
    const displayUserInfoPanel = () => {
        setDisplay(prev => ({
            ...prev,
            invitationList: false,
            userInfoPanel: !prev.userInfoPanel,
        }));
    };

    // Invitation list
    // Fetches all invitations
    const getInvitations = async () => {
        try {
            const data = await axios.get(`${APIs.GET_INVITATIONS}/${userId}`, {
                withCredentials: true,
                headers: { Authorization: `Bearer ${accessToken}` },
            });

            if (data.status === 200) {
                setFriend(prev => ({
                    ...prev,
                    invitations: data.data.resultData,
                }));
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await getInvitations();
            } else if (err.response?.status === 400) {
                console.error("bad host id");
            } else if (err.response?.status === 404) {
                // no new invitations
                setFriend(prev => ({ ...prev, invitations: [] }));
            } else {
                console.error(err);
            }
        }
    };

    // Invitation list
    // Handles actions for accepting or decelining invitation
    const invitationActions = async (action, recipientID) => {
        const API_URL = action === "accept" ? APIs.ACCEPT_INVITE : APIs.DECELINE_INVITE;

        try {
            const data = await axios.post(
                API_URL,
                JSON.stringify({
                    senderId: recipientID,
                    recipientId: userId,
                }),
                {
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        "Content-Type": "application/json",
                    },
                }
            );

            if (data.status === 200) {
                // delete invitation
                setFriend(prev => ({
                    ...prev,
                    invitations: prev.invitations.filter(inv => inv.id !== recipientID),
                }));

                await getFriends();
            }
        } catch (err) {
            if (err.response?.status === 401) {
                await refreshAccessToken();
                await invitationActions(action, recipientID);
            } else if (err.response?.status === 404) {
                console.error("Invitation or RecipientUser doesn't exist");
            } else {
                console.error(err);
            }
        }
    };

    // Menu bar
    // Sings out user - reset states, session Storage
    const signOut = () => {
        setAuth('', '', '', '');
        sessionStorage.removeItem('refreshToken');
        sessionStorage.removeItem('userInfo');
    };

    useEffect(() => {
        if (!accessToken) {
            refreshAccessToken();
        }

        if (currentPath == "/message") {
            getInvitations();
        }

    }, [accessToken, username, currentPath]);

    return (
        <div id='mainMessagePage'>
            {/* INVITATION LIST */}
            {display.invitationList && (
                <div id='invitationsList'>
                    {friend.invitations && friend.invitations.length > 0 ? (
                        friend.invitations.map(user => (
                            <InvitationTile
                                key={user.id}
                                id={user.id}
                                username={user.userName}
                                invitationAction={invitationActions}
                            />
                        ))
                    ) : (
                        <div className='infoText'>No new invitations</div>
                    )}
                </div>
            )}

            {/* USER INFO PANEL */}
            {display.userInfoPanel && (
                <UserInfoPanel username={username} fullname={null} email={null} />
            )}

            {/* MENU BAR */}
            <div id='menuBar'>
                <div id='logoInMenu' />
                <div id='profileBox'>
                    {
                        currentPath === "/message" &&
                        <div className='bellWrapper' onClick={displayInvitationList}>
                            <FontAwesomeIcon icon={faBell} className='friendBarIcon' />
                            {friend.invitations && friend.invitations.length > 0 && (
                                <div className='notificationBadge'>{friend.invitations.length}</div>
                            )}
                        </div>
                    }

                    <button className='btn2' onClick={signOut}>Sign out</button>
                    <div className='profileInfoWrapper' onClick={displayUserInfoPanel}>
                        {username}
                        <div className='profileIcon' />
                    </div>
                </div>
            </div>

            <Outlet />
        </div>
    );
};

export default Layout;