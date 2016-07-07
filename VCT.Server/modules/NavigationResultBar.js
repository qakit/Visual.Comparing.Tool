import React from 'react'

export default React.createClass({
    render: function() {
        const diffIconClass = this.props.showDiff ? "fa fa-eye-slash" : "fa fa-eye";
        const imageIndex = this.props.currentImageIndex + 1;
        const displayDiff = this.props.hasDiff ? { display: "inline" } : { display: "none" };
        const displayAcceptReject = this.props.showAcceptReject ? { display: "inline" } : { display: "none" };

        return (
            <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><a href="#" id="previousTest" onClick={this.props.clickEvent}><i  id="previousTest" className="fa fa-step-backward"></i></a></li>
                    <li><a href="#" id="previousFail" onClick={this.props.clickEvent}><i id="previousFail" className="fa fa-backward"></i></a></li>
                    <li><p className="navbar-text">{imageIndex} / {this.props.maxImages}</p></li>
                    <li><a href="#" id="nextFail" onClick={this.props.clickEvent}><i id="nextFail" className="fa fa-forward"></i></a></li>
                    <li><a href="#" id="nextTest" onClick={this.props.clickEvent}><i id="nextTest" className="fa fa-step-forward"></i></a></li>
                    <li style={displayDiff}><a href="#" id="showDiff" onClick={this.props.clickEvent}><i id="showDiff" className={diffIconClass}></i></a></li>
                    <li><p className="navbar-text">{this.props.testName}</p></li>
                </ul>
                <ul className="nav navbar-nav navbar-right">
                    <li style={displayAcceptReject}><a href="#" id="acceptFail" onClick={this.props.clickEvent}><i id="acceptFail" className="fa fa-check"></i></a></li>
                    <li style={displayAcceptReject}><a href="#" id="rejectFail" onClick={this.props.clickEvent}><i id="rejectFail" className="fa fa-ban"></i></a></li>
                </ul>
            </div>
        )
    }
});