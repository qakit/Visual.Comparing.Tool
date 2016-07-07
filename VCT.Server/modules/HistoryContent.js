import React from 'react'
import HistoryItem from './HistoryItem'
import HistoryBar from './HistoryBar'

var fakeHistoryData = [{"DateStarted":"","DateCompleted":"","Passed":0,"Failed":0,"Id":0,"Name":"","Tests":null}]

export default React.createClass({
    getInitialState: function(){
        var fakeData = fakeHistoryData;
        return({
            data: fakeData,
            projectId: ""
        })
    },
    componentDidMount: function(){
        this.loadDataFromServer(); 
    },
    loadDataFromServer: function() {
        var projectId = this.props.params.projectId;
        var url = "api/" + projectId + "/suites";

        $.ajax({
            url: url,
            dataType: 'json',
            cache: false,
            success: function(data) {
                this.setState({
                    data: data,
                    projectId: projectId
                })
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });
    },
    render: function(){
        var projId = this.state.projectId;

        return (
            <div>
                <nav className="navbar navbar-default history">
                    <div className="container-fluid">
                        <HistoryBar />
                    </div>
                </nav>
                <div className="row history-items">
                    <div className="col-xs-12">
                        <table className="table table-striped">
                            <thead>
                                <tr>
                                    <th>Status</th>
                                    <th>Run ID</th>
                                    <th>Start Time</th>
                                    <th>End Time</th>
                                    <th>Passed Tests</th>
                                    <th>Failed Tests</th>
                                </tr>            
                            </thead>
                            <tbody>
                                {this.state.data.map(function(d){
                                    return <HistoryItem 
                                            key={d.Id} 
                                            id={d.Id}
                                            passed={d.Passed}
                                            failed={d.Failed}
                                            dateCompleted={d.DateCompleted}
                                            dateStarted={d.DateStarted}
                                            suiteId={d.Name}
                                            projectId={projId}
                                    />
                                })}
                            </tbody>
                        </table>
                    </div>
                </div> 
            </div>
        );
    }
})