import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../../app/providers/AuthProvider';

import EditAvatar from "./EditAvatar";
import EditUsername from "./EditUsername";
import EditPassword from "./EditPassword";


const EditProfilePanel = ({ togglePanel }) => {
    return (
        <div className="offFocusBK" onClick={() => togglePanel('editProfilePanel')}>
            <div
                className="editProfilePanel"
                onClick={(e) => e.stopPropagation()}
            >
                <div className="closeBtnBox">
                    <div className="closeBtn" onClick={() => togglePanel('editProfilePanel')} />
                </div>

                <EditAvatar />
                <EditUsername />
                <EditPassword />
            </div>
        </div>
    )
}

export default EditProfilePanel