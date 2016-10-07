import React from 'react'
import { Link } from 'react-router'

export default React.createClass({
    render: function() {
        const awesomeStyle = {
            height: '80px',
            marginTop: '15px',  
        };
        const evenMoreAwesomeStyle = {
            display: 'block',
            width: '100%',
            height: '100%',
            color: 'rgba(0,0,0,0.87)',
        };
        
        return (
                <div className="col-xs-12 col-md-6" style={awesomeStyle}>
                    <Link to={`/${this.props.name}`} className="row run" style={evenMoreAwesomeStyle}>
                        <div className="col-xs-6 info">
                            <p className="title">Project: {this.props.name}</p>
                            <p className="details">Project description goes here.</p>
                        </div>
                        <div className="col-xs-6 stat suites">
                            <p className="title">Suites</p>
                            <p className="amount">{this.props.suites}</p>
                        </div>
                    </Link>
                </div>
        )
    }
})