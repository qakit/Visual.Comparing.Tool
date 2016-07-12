import React from 'react'
import ProjectItem from './ProjectItem'

export default React.createClass({
    render: function(){
        var clickHanlded = this.props.clickEventHandler;
        return (
            <div>
                <nav className="navbar navbar-default">
                    <div className="container-fluid">
                        <div className="navbar navbar-default history" id="bs-example-navbar-collapse-1">
                            <ul className="nav navbar-nav">
                                <li><p className="navbar-text icon"><i className="ion-clock"></i></p></li>
                                <li><p className="navbar-text title">Projects</p></li>
                            </ul>
                        </div>
                    </div>
                </nav>
                <div className="container-fluid">
                    <div className="row">
                       {this.props.data.map(function(d){
                           return <ProjectItem 
                                key={d.Id} 
                                id={d.Id}
                                name={d.Name}
                                suites={d.SuitesCount}
                                clickEventHandler = {clickHanlded(d.Id, d.Name)}
                           />
                       })}
                    </div>
                </div> 
            </div>
        );
    }
})