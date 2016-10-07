import React from 'react'

export default React.createClass({
    render: function() {
        const diffIconClass = this.props.showDiff ? "fa fa-eye-slash" : "fa fa-eye";
        const imageIndex = this.props.currentImageIndex + 1;
        const displayDiff = this.props.hasDiff ? { display: "inline" } : { display: "none" };
        const displayAcceptReject = this.props.showAcceptReject ? { display: "inline" } : { display: "none" };
        
        return (
            <div className="navbar navbar-default history" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><a href="#" id="back"><i  id="previousTest" className="fa fa-arrow-left"></i></a></li>
                    <li><p className="navbar-text">{this.props.testName}</p></li>
                </ul>
                <ul className="nav navbar-nav navbar-right">
                    <li><a href="#" id="toggleList"><i className="fa fa-list"></i></a></li>
                </ul>
            </div>
        )
    }
});