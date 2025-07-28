import React, { useState, useEffect, useContext } from "react";
import { Outlet, Link, useLocation } from 'react-router-dom';
import { AuthContext } from '../../context/AuthProvider';

import EditAvatar from "./EditAvatar";
import EditUsername from "./EditUsername";
import ValidatedInput from "../form/ValidatedInput";


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

                <div className="editProfileTitle">Password</div>
                <div className="editProfileElement">
                    <div className="editPanelInputWrapper">
                        <ValidatedInput
                            htmlName="oldpassword"
                            labelText="Old password"
                            formData={null}
                            regexStatus={null}
                            formFocus={null}
                            handleChange={null}
                            handleFocusOn={null}
                            inputType="password"
                            validationText={<></>
                            }
                        />
                        <ValidatedInput
                            htmlName="newPassword"
                            labelText="New password"
                            formData={null}
                            regexStatus={null}
                            formFocus={null}
                            handleChange={null}
                            handleFocusOn={null}
                            inputType="password"
                            validationText={<></>
                            }
                        />
                        <ValidatedInput
                            htmlName="repeatPassword"
                            labelText="Repeat password"
                            formData={null}
                            regexStatus={null}
                            formFocus={null}
                            handleChange={null}
                            handleFocusOn={null}
                            inputType="password"
                            validationText={<></>
                            }
                        />

                    </div>
                    <div className="editProfileBtnWrapper">
                        <button className='editPanelBtn default'>Save</button>
                    </div>
                </div>
            </div>
        </div>
    )
}

export default EditProfilePanel