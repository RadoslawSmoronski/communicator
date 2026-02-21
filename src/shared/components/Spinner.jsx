import React from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faCircleNotch } from '@fortawesome/free-solid-svg-icons';

const Spinner = ({ size = '2x', color = '#3498db' }) => {
    return (
        <div className="spinner-container">
            <FontAwesomeIcon
                icon={faCircleNotch}
                className="spinner"
                style={{ color: color }}
                size={size}
            />
        </div>
    );
};

export default Spinner;