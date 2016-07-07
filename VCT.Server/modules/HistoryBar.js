import React from 'react'

export default React.createClass({
    render: function() {
        return (
            <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><p className="navbar-text icon"><i className="ion-clock"></i></p></li>
                    <li><p className="navbar-text title">Test Results</p></li>
                </ul>
            </div>
        )
    }
});