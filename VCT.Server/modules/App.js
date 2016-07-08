import React from 'react'
import ProjectsContent from './Projects'
import HistoryContent from './HistoryContent'

var fakeProjectData = [{"Id":1,"Name":"","SuitesCount":0}]

export default React.createClass({
    getInitialState: function(){
        var defaultUrl = "/api/projects";
        var fakeData = fakeProjectData;
        return ({
            url: defaultUrl,
            data: fakeData,
            content: <ProjectsContent data={fakeData} clickEventHandler = {this.handleProjectClick}/>,
        });
    },
    componentDidMount: function(){
        this.loadDataFromServer(); 
    },
    loadDataFromServer: function() {
        $.ajax({
            url: this.state.url,
            dataType: 'json',
            cache: false,
            success: function(data) {
                this.setState({
                    data: data,
                    content: <ProjectsContent data={data} clickEventHandler = {this.handleProjectClick}/>
            });
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });
    },
    handleProjectClick: function (id, projectName) {
       const _this = this;
       
       return function(event){
              var testData = _this.state.data[id - 1].Suites;
              _this.setState({
                    data: testData,
                    content: <HistoryContent data={testData} projectName={projectName} clickEventHandler = {_this.handleHistoryClick}/>
              });
        }
    },
    render: function(){
        return(
            this.state.content
        )
    }
})