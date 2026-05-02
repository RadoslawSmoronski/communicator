import React, { useState, useEffect, useContext } from "react";

import EditAvatar from "./edit/EditAvatar";
import EditUsername from "./edit/EditUsername";
import EditPassword from "./edit/EditPassword";


const EditProfilePanel = ({ display, togglePanel }) => {
    return (
        <>
            {
                display &&
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
            }
        </>
    )
}

export default EditProfilePanel