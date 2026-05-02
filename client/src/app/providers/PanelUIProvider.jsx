import React, { createContext, useState } from "react";

export const PanelUIContext = createContext();

const PanelUIProvider = ({ children }) => {
    const [display, setDisplay] = useState({
        invitationList: false,
        userInfoPanel: false,
        editProfilePanel: false
    });

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

    const hidePanels = () => {
        setDisplay({
            invitationList: false,
            userInfoPanel: false,
            editProfilePanel: false,
        })
    }

    return (
        <PanelUIContext.Provider value={{
            display,
            togglePanel,
            hidePanels
        }}>
            {children}
        </PanelUIContext.Provider>
    )
}

export default PanelUIProvider;