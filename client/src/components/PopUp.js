import React from 'react';

class PopUp extends React.Component {
    constructor(props) {
        super(props);

    }


    render() {
        return (
            <>
            {this.props.state && (
                <table className="popUp" >
                        <tbody>
                            <tr>
                                <th>{this.props.mess}</th>
                                <th className="closeBtnBox">
                                    <div className="closeBtn" onClick={this.props.close} />
                                </th>
                            </tr>
                        </tbody>
                </table>
            )}
            </>
        );
    }
}

export default PopUp;