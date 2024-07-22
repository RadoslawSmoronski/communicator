import React from "react";

class Note extends React.Component{
    render(){
        return(
            <div className="note">
                <h4>{this.props.title}</h4>
                {this.props.text}
            </div>
        );
    }

}
export default Note;