import React, { Component } from 'react';
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCheck, faTimes, faInfoCircle } from "@fortawesome/free-solid-svg-icons";

class ValidationBox extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        return (
            <>
            {
                !this.props.regex && this.props.value && this.props.focus && (
                    <div className="validationBox">
                        <FontAwesomeIcon icon={faInfoCircle}/><br/>
                        {this.props.text}
                    </div>
                )
            }
            </>
        );
    }
}

export default ValidationBox;