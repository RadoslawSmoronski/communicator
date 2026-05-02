import React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCircleNotch } from '@fortawesome/free-solid-svg-icons';

const Spinner = ({ size = '2em', containerPadding = '20px' }) => {
    return (
        <div className="spinner-container"
            style={{ padding: containerPadding }}>
            <FontAwesomeIcon
                icon={faCircleNotch}
                className="spinner"
                style={{ fontSize: size }}
            />
        </div >
    );
};

export default Spinner;