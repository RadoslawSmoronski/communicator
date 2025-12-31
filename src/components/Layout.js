import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../app/providers/AuthProvider';
import { UserContext } from "../app/providers/UserProvider";

import { tokenService } from "../features/auth/services/tokenService";
import { userDataService } from "../features/auth/services/userDataService";

import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faBell } from "@fortawesome/free-solid-svg-icons";

import axios from "../api/axios";
import APIs from "../api/ApiURL";
import eventBus from "../shared/utils/eventBus";

import InvitationTile from './tiles/InvitationTile';
import UserInfoPanel from '../features/profile/UserInfoPanel';
import EditProfilePanel from "../features/profile/EditProfilePanel"
import Avatar from "../shared/components/Avatar";

const Layout = () => {
    const { accessToken, refreshAccessToken } = useContext(AuthContext);
    const { user, removeUser } = useContext(UserContext);

    const location = useLocation();
    const currentPath = location.pathname;

    const [friend, setFriend] = useState({
        invitations: []
    });

    const [display, setDisplay] = useState({
        invitationList: false,
        userInfoPanel: false,
        editProfilePanel: false
    });

    // Invitation list
    // Displays / hides given panel
    const togglePanel = (panelName) => {
        setDisplay(prev => {
            const panelsState = {
                invitationList: false,
                userInfoPanel: false,
                editProfilePanel: false,
            };

            panelsState[panelName] = !prev[panelName];
            return panelsState;
        });
    };


    // Invitation list
    // Fetches all invitations
    const getInvitations = async () => {
        try {
            const data = await axios.get(APIs.GET_INVITATIONS(user?.userID), {
                withCredentials: true,
                headers: { Authorization: `Bearer ${accessToken}` },
            });

            if (data.status === 200) {
                setFriend(prev => ({
                    ...prev,
                    invitations: data.data,
                }));
            }
        } catch (err) {
            if (err.response?.status === 401) {
                // await refreshAccessToken();
                // await getInvitations();
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
    const invitationActions = async (action, invitationId, senderId) => {
        // const API_URL = action === "accept" ? APIs.ACCEPT_INVITE(invitationId) : APIs.DECELINE_INVITE(invitationId);

        try {
            const data = action === "accept" ? await axios.post(
                APIs.ACCEPT_INVITE(invitationId),
                {},
                {
                    withCredentials: true,
                    headers: {
                        Authorization: `Bearer ${accessToken}`,
                        "Content-Type": "application/json",
                    },
                }
            ) :
                await axios.delete(
                    APIs.DECELINE_INVITE(invitationId),
                    {
                        withCredentials: true,
                        headers: {
                            Authorization: `Bearer ${accessToken}`,
                            "Content-Type": "application/json",
                        },
                    }
                );
            ;

            if (data.status === 200 || data.status === 201) {
                // delete invitation
                setFriend(prev => ({
                    ...prev,
                    invitations: prev.invitations.filter(inv => inv.friendInvitationId !== invitationId),
                }));

                // emit the 'refreshFriends'
                eventBus.emit('refreshFriends');
            }
        } catch (err) {
            if (err.response?.status === 401) {
                // await refreshAccessToken();
                // await invitationActions(action, invitationId, senderId);
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
        removeUser();
    };

    useEffect(() => {
        const refreshToken = sessionStorage.getItem('refreshToken');
        if (!refreshToken) { // do not refresh token if user is logged out
            return;
        }

        if (!accessToken) {
            // refreshAccessToken();
        } else {
            if (currentPath == "/message") {
                getInvitations();
            }
        }

    }, [accessToken, user?.username, currentPath]);

    return (
        <div id='mainMessagePage'>
            {/* INVITATION LIST */}
            {display.invitationList && (
                <div id='invitationsList'>
                    {friend.invitations && friend.invitations.length > 0 ? (
                        friend.invitations.map(inv => (
                            <InvitationTile
                                key={inv.friendInvitationId}
                                invitationId={inv.friendInvitationId}
                                senderId={inv.senderId}
                                username={inv.senderUsername}
                                avatarUrl={inv.senderAvatarUrl}
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
                <UserInfoPanel avatarUrl={user?.avatarUrl} username={user?.username} fullname={null} email={user?.email} togglePanel={togglePanel} />
            )}

            {/* EDIT PROFILE PANEL */}
            {display.editProfilePanel && (
                <EditProfilePanel togglePanel={togglePanel} />
            )
            }

            {/* MENU BAR */}
            <div id='menuBar'>
                <div id='logoInMenu' />
                <div id='profileBox'>
                    {
                        currentPath === "/message" &&
                        <div className='bellWrapper' onClick={() => togglePanel('invitationList')}>
                            <FontAwesomeIcon icon={faBell} className='friendBarIcon' />
                            {friend.invitations && friend.invitations.length > 0 && (
                                <div className='notificationBadge'>{friend.invitations.length}</div>
                            )}
                        </div>
                    }

                    <button className='btn2' onClick={signOut}>Sign out</button>
                    <div className='profileInfoWrapper' onClick={() => togglePanel('userInfoPanel')}>
                        {user?.username}
                        <Avatar url={user?.avatarUrl} />
                    </div>
                </div>
            </div>

            <Outlet />
        </div>
    );
};

export default Layout;