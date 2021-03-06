import React from 'react'
import { Link, browserHistory } from 'react-router'

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
        
        var suiteClass = this.props.failed === 0 ? "ion-checkmark-circled passed" : "ion-close-circled failed"
        var statusStyle = {width: '40px', padding: 0, textAlign: 'center'};
        var projectId = this.props.projectId;
        var suiteId = this.props.suiteId;
        var failed = this.props.failed;

        var handleHistoryItemClick = function(){
            //no need to show anything in case if there wasn't any fail
            if(failed === 0) return;
            var url = "/" + projectId + "/" + suiteId;
            browserHistory.push(url);
        }

        var handleClick = this.props.handleRemoveClick;
        var handleRemoveSuiteClick = function(e){
            e.stopPropagation();
            handleClick(projectId, suiteId);
        }

        return (
            <tr onClick={handleHistoryItemClick}>
                <td style={statusStyle}><i className={suiteClass}/></td>
                <td>{this.props.id}</td>
                <td>{this.props.dateStarted}</td>
                <td>{this.props.dateCompleted}</td>
                <td className="stat passed">{this.props.passed}</td>
                <td className="stat failed">{this.props.failed}</td>
                <td onClick={handleRemoveSuiteClick} style={statusStyle}><i className="ion-ios-trash"/></td>
            </tr>
        )
    }
});