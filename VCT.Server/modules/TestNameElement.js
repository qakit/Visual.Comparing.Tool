import React from 'react'

export default React.createClass({
    render: function() {
        return (
            <div className="navbar-header">
                <a className="navbar-brand">{this.props.testName}</a>
            </div>
        );
    }
})