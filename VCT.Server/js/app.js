var fakeProjectData = [{
	"Id": 0,
	"Name": "",
	"Suites": [{
		"DateStarted": "",
		"DateCompleted": "",
		"Passed": 0,
		"Failed": 0,
		"Id": 0,
		"Tests": [{
			"TestName": "",
			"Artifacts": [{
				"StableFile": {
					"Name": "",
					"Path": "",
					"RelativePath": ""
				},
				"TestingFile": {
					"Name": "",
					"Path": "",
					"RelativePath": ""
				},
				"DiffFile": {
					"Name": "",
					"Path": "",
					"RelativePath": ""
				}
			}]
		}]
	}]
}]

var TestNameElement = React.createClass({
    render: function() {
        return (
            <div className="navbar-header">
                <a className="navbar-brand">{this.props.testName}</a>
            </div>
        );
    }
})

var NavigationResultBar = React.createClass({
    render: function() {
        const diffIconClass = this.props.showDiff ? "fa fa-eye-slash" : "fa fa-eye";
        const imageIndex = this.props.currentImageIndex + 1;
        const displayDiff = this.props.hasDiff ? { display: "inline" } : { display: "none" };
        const displayAcceptReject = this.props.showAcceptRefect ? { display: "inline" } : { display: "none" };

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

var TestResultContainer = React.createClass({
    render: function() {
        return (
            <div className="flexChild rowParent">
                <div id="leftImage" onScroll={this.props.scrollEvent} className="flexChild">
                    <img src={this.props.left}/>
                </div>
                <div id="rightImage" onScroll={this.props.scrollEvent} className="flexChild">
                    <img src={this.props.right} />
                </div>
            </div>
        );
    }
});

var ResultsPreviewCotent = React.createClass({
    getCurrentImageName: function(artifact) {
        if (artifact.TestingFile.Name !== "") {
            return artifact.TestingFile.Name;
        }
        if (artifact.StableFile.Name !== "") {
            return artifact.StableFile.Name;
        }
        if (artifact.DiffFile.Name !== "") {
            return artifact.DiffFile.Name;
        }
        return "";
    },
    componentDidMount: function() {
        $(document.body).on('keydown', this.handleKeyDown);
        // this.loadDataFromServer();
    },
    componenWillUnmount: function() {
        $(document.body).off('keydown', this.handleKeyDown);
    },
    getInitialState: function() {
        var artifacts = this.props.data[0].Artifacts;
        
        var imageName = this.getCurrentImageName(artifacts[0]);
        var maxImages = artifacts.length;
        var maxTests = this.props.data.length;
        var hasDiff = artifacts[0].DiffFile.Name !== "";
        
        return ({
            //index of the test in current fails
            testIndex: 0,
            //image index in the current test
            imageIndex: 0,
            //max images count
            maxImages: maxImages,
            maxTests: maxTests,
            imageName: imageName,
            //show diff image instead of stable or not
            showDiff: false,
            hasDiff: hasDiff,
            testData: this.props.data
        });
    },
    handleChildClick: function(event) {
        var imageIndex = this.state.imageIndex;
        var showDiff = this.state.showDiff;
        var hasDiff = this.state.hasDiff;
        var testIndex = this.state.testIndex;
        var currentImageName = this.state.imageName;
        var maxImages = this.state.maxImages;
        var id;

        if (typeof event === "string") {
            id = event;
        } else {
            id = event.target.id;
        }

        if (id === "showDiff" && hasDiff) {
            showDiff = !this.state.showDiff;
        }
        if (id === "nextFail") {
            if (this.state.imageIndex === (this.state.maxImages - 1)) return;
            
            imageIndex = this.state.imageIndex + 1;
            showDiff = false;
        }
        if (id === "previousFail") {
            if (this.state.imageIndex === 0) return;
            imageIndex = this.state.imageIndex - 1,
                showDiff = false
        }
        if (id === "previousTest") {
            if (this.state.testIndex === 0) return;

            imageIndex = 0;
            showDiff = false;
            testIndex = this.state.testIndex - 1;

            maxImages = this.state.testData[testIndex].Artifacts.length;
        }
        if (id === "nextTest") {
            if (this.state.testIndex === (this.state.maxTests - 1)) return;

            testIndex = this.state.testIndex + 1;
            imageIndex = 0;
            showDiff = false;

            maxImages = this.state.testData[testIndex].Artifacts.length;
        }
        if (id === "acceptFail") {
            var testName = this.state.testData[this.state.testIndex].TestName;
            var projectName = this.props.projectName;
            if(testName === "") return;
            
            var url = 'tests//' + projectName + "//" + testName + '//stable';
            $.ajax({
                url: url,
                dataType: 'text',
                type: 'POST',
                cache: false,
                success: function(data) {
                    console.log(data);
                }.bind(this),
                error: function(xhr, status, err) {
                    console.error(url, status.err.toString());
                }.bind(this)
            });
            return;
        }

        currentImageName = this.getCurrentImageName(this.state.testData[testIndex].Artifacts[imageIndex]);
        hasDiff = this.state.testData[testIndex].Artifacts[imageIndex].DiffFile.Name !== "";

        this.setState({
            imageIndex: imageIndex,
            showDiff: showDiff,
            hasDiff: hasDiff,
            testIndex: testIndex,
            imageName: currentImageName,
            maxImages: maxImages
        })
    },
    handleScroll: function(e){
        var current = e.target;
        var $other = current.id === "leftImage" ? $("#rightImage") : $("#leftImage");
        $other.off('scroll');
        var other = $other[0];
        
        var verticalPercentage = current.scrollTop / (current.scrollHeight - current.offsetHeight);
        var horizontalPercentage = current.scrollLeft / (current.scrollWidth - current.offsetWidth);
        other.scrollLeft = horizontalPercentage * (other.scrollWidth - other.offsetWidth);;
        other.scrollTop = verticalPercentage * (other.scrollHeight - other.offsetHeight);
        
        setTimeout(function() { $other.on('scroll', this.handleScroll); }, 10);
    },
    handleKeyDown: function(event) {
        //rigth arrow
        if (event.keyCode === 39) {
            this.handleChildClick("nextFail");
            return;
        }
        //left arrow
        if (event.keyCode === 37) {
            this.handleChildClick("previousFail");
            return;
        }
        //up arrow
        if (event.keyCode === 38) {
            this.handleChildClick("previousTest");
            return;
        }
        //down arrow
        if (event.keyCode === 40) {
            this.handleChildClick("nextTest");
            return;
        }
        //space
        if (event.keyCode === 32) {
            this.handleChildClick("showDiff");
            return;
        }
    },
    render: function() {
        const {TestName: testName, Artifacts: artifacts} = this.state.testData[this.state.testIndex];
        
        const testingImage = this.state.showDiff ? artifacts[this.state.imageIndex].DiffFile : artifacts[this.state.imageIndex].TestingFile;
        const imageName = this.state.imageName;

        var testingImagePath;

        if (testingImage.RelativePath !== "") {
            testingImagePath = testingImage.RelativePath;
        } else {
            if (this.state.showDiff) {
                testingImagePath = "images\\nodiff.png";
            } else {
                testingImagePath = "images\\notesting.png"
            }
        }

        const stableImage = artifacts[this.state.imageIndex].StableFile;
        var stableImagePath
        if (stableImage.RelativePath !== "") {
            stableImagePath = stableImage.RelativePath;
        } else {
            stableImagePath = "images\\nostable.png";
        }

        return (
            <div className="flexChild columnParent"> 
                <nav className="navbar navbar-default">
                    <div className="container-fluid">
                        <NavigationResultBar 
                            clickEvent={this.handleChildClick}
                            showDiff = {this.state.showDiff}
                            testName = {testName}
                            currentImageIndex = {this.state.imageIndex}
                            maxImages = {this.state.maxImages}
                            hasDiff = {this.state.hasDiff}
                            showAcceptRefect = {this.props.showAcceptRefect}
                        />
                    </div>
                </nav>
                <TestResultContainer left={stableImagePath} right={testingImagePath} scrollEvent={this.handleScroll}/>
            </div>
        );
    }
});

var HistoryBar = React.createClass({
    render: function() {
        return (
            <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><p className="navbar-text"><i className="ion-clock"></i></p></li>
                    <li><p className="navbar-text">Test Results</p></li>
                </ul>
            </div>
        )
    }
});

var HistoryItem = React.createClass({
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
            <div className="col-xs-12 col-md-6" style={awesomeStyle} onClick={this.props.clickEventHandler}>
                <a href="#" className="row run" style={evenMoreAwesomeStyle}>
                    <div className="col-xs-6 info">
                        <p className="title">Some title with ID: {this.props.id}</p>
                        <p className="details">Started: {this.props.dateStarted}. Completed: {this.props.dateCompleted}</p>
                    </div>
                    <div className="col-xs-3 stat passed">
                        <p className="title">Passed</p>
                        <p className="amount">{this.props.passed}</p>
                    </div>
                    <div className="col-xs-3 stat failed">
                        <p className="title">Failed</p>
                        <p className="amount">{this.props.failed}</p>
                    </div>
                    
                </a>
            </div>
        )
    }
});

var ProjectItem = React.createClass({
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
            <div className="col-xs-12 col-md-6" style={awesomeStyle} onClick={this.props.clickEventHandler}>
                <a href="#" className="row run" style={evenMoreAwesomeStyle}>
                    <div className="col-xs-6 info">
                        <p className="title">Some title with ID: {this.props.id}</p>
                        <p className="details">Project: {this.props.name}</p>
                    </div>
                    <div className="col-xs-6 stat suites">
                        <p className="title">Suites</p>
                        <p className="amount">{this.props.suites}</p>
                    </div>
                </a>
            </div>
        )
    }
});

var HistoryContent = React.createClass({
    render: function(){
        var clickHanlded = this.props.clickEventHandler;
        var projectName = this.props.projectName;
        return (
            <div>
                <nav className="navbar navbar-default history">
                    <div className="container-fluid">
                        <HistoryBar />
                    </div>
                </nav>
                <div className="container-fluid">
                    <div className="row">
                       {this.props.data.map(function(d){
                           return <HistoryItem 
                                key={d.Id} 
                                id={d.Id}
                                passed={d.Passed}
                                failed={d.Failed}
                                dateCompleted={d.DateCompleted}
                                dateStarted={d.DateStarted}
                                clickEventHandler = {clickHanlded(d.Id, projectName)}
                           />
                       })}
                    </div>
                </div> 
            </div>
        );
    }
})

var ProjectsContent = React.createClass({
    render: function(){
        var clickHanlded = this.props.clickEventHandler;
        
        return (
            <div>
                <nav className="navbar navbar-default history">
                    <div className="container-fluid">
                        <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                            <ul className="nav navbar-nav">
                                <li><p className="navbar-text"><i className="ion-clock"></i></p></li>
                                <li><p className="navbar-text">Projects</p></li>
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
                                suites={d.Suites.length}
                                clickEventHandler = {clickHanlded(d.Id, d.Name)}
                           />
                       })}
                    </div>
                </div> 
            </div>
        );
    }
})

var Page = React.createClass({
    getInitialState: function(){
        var defaultUrl = "/tests/history";
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
    handleHistoryClick: function(id, projectName){
        const _this = this;
        return function(event){
              var testData = _this.state.data[id].Tests;
              var showAcceptRefect = id === 0 ? true : false;
              _this.setState({
                    data: testData,
                    content: <ResultsPreviewCotent data={testData} showAcceptRefect = {showAcceptRefect} projectName={projectName}/>
              });
        }
    },
    handleProjectClick: function (id, projectName) {
       const _this = this;
       return function(event){
              var testData = _this.state.data[id].Suites;
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
});

ReactDOM.render(
    <Page/>,
    document.getElementById('root')
)