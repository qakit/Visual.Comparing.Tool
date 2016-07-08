import React from 'react'

export default React.createClass({
    handleChildClick: function(){
        window.history.back();
    },
    render: function() {
        return (
            <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><a href="#" id="back" onClick={this.handleChildClick}><i id="back" className="fa fa-arrow-left"></i></a></li>
                    <li><p className="navbar-text icon"><i className="ion-clock"></i></p></li>
                    <li><p className="navbar-text title">Test Results</p></li>
                </ul>
            </div>
        )
    }
});