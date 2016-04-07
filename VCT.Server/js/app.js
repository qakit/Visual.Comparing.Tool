 var fakeData = [{
	"TestName": "",
	"Artifacts": [{
		"DiffImages": [{
			"Name": "",
			"Path": "",
			"RelativePath": ""
		}],
		"TestingImages": [{
			"Name": "",
			"Path": "",
			"RelativePath": ""
		}],
		"StableImages": [{
			"Name": "",
			"Path": "",
			"RelativePath": ""
		}]
	}]
}];
            
var TestNameElement = React.createClass({
    render: function(){
        return (
            <div className="navbar-header">
                  <a className="navbar-brand">{this.props.testName}</a>
            </div>
        );
    }
})

var NavigationResultBar = React.createClass({
    render: function(){
        const diffIconClass = this.props.showDiff ? "fa fa-eye-slash" : "fa fa-eye";
        
        return(
            <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><a href="#" id="previousTest" onClick={this.props.clickEvent}><i  id="previousTest" className="fa fa-step-backward"></i></a></li>
                    <li><a href="#" id="previousFail" onClick={this.props.clickEvent}><i id="previousFail" className="fa fa-backward"></i></a></li>
                    <li><a href="#" id="nextFail" onClick={this.props.clickEvent}><i id="nextFail" className="fa fa-forward"></i></a></li>
                    <li><a href="#" id="nextTest" onClick={this.props.clickEvent}><i id="nextTest" className="fa fa-step-forward"></i></a></li>
                    <li><a href="#" id="showDiff" onClick={this.props.clickEvent}><i id="showDiff" className={diffIconClass}></i></a></li>
                    <li><a><i className="glyphicon glyphicon-tag">{this.props.testName}</i></a></li>
                </ul>
                <ul className="nav navbar-nav navbar-right">
                    <li><a href="#" id="acceptFail" onClick={this.props.clickEvent}><i id="rejectFail" className="fa fa-check"></i></a></li>
                    <li><a href="#" id="rejectFail" onClick={this.props.clickEvent}><i id="rejectFail" className="fa fa-ban"></i></a></li>
                </ul>
            </div>
        )
    }
});

var TestResultContainer = React.createClass({
    render: function(){
        return (
            <div className="flexChild rowParent">
                <div className="flexChild">
                    <img src={this.props.left}/>
                </div>
                <div className="flexChild">
                    <img src={this.props.right} />
                </div>
            </div>
        );
    }
});

var PageContent = React.createClass({
    loadCommentsFromServer: function() {
        $.ajax({
            url: this.props.url,
            dataType: 'json',
            cache: false,
            success: function(data) {
                var maxTests = data.length;
                
                var testingImages = data[0].Artifacts[0].TestingImages;
                var initialImageName = testingImages[0].Name;
                this.setState({
                    testData: data,
                    maxTests: maxTests,
                    maxImages: testingImages.length,
                    imageName: initialImageName 
            });
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });
    },
    componentDidMount: function(){
        console.log('getting data');
       this.loadCommentsFromServer(); 
    },
    getInitialState: function(){
        return ({
            //index of the test in current fails
            testIndex: 0,
            //image index in the current test
            imageIndex: 0,
            //max images count
            maxImages: 0,
            maxTests: 0,
            imageName: "",
            //show diff image instead of stable or not
            showDiff: false,
            testData: fakeData
        });
    },
    handleChildClick: function(event){
        if(event.target.id === "showDiff"){
            this.setState({showDiff: !this.state.showDiff});
            return;
        }
        if(event.target.id === "nextFail"){
            if(this.state.imageIndex === (this.state.maxImages - 1)) return;
            //todo handle end of the fails (our of range);            
            this.setState({
                imageIndex: this.state.imageIndex + 1,
                showDiff: false
            });
            return;
        }
        if(event.target.id === "previousFail"){
            if(this.state.imageIndex === 0) return;
            
            this.setState({
                imageIndex: this.state.imageIndex - 1,
                showDiff: false
            });
            return;
        }
        if(event.target.id === "previousTest"){
            if(this.state.testIndex === 0) return;
            
            this.setState({
                testIndex: this.state.testIndex - 1,
                imageIndex: 0,
                showDiff: false
            });
            return;
        }
         if(event.target.id === "nextTest"){
            if(this.state.testIndex === (this.state.maxTests - 1)) return;
            console.log('next test');
            this.setState({
                testIndex: this.state.testIndex + 1,
                imageIndex: 0,
                showDiff: false
            });
            return;
        }
    },
    render: function(){
        console.log('rendering');
        const {TestName: testName, Artifacts: artifacts} = this.state.testData[this.state.testIndex];
        const collection = this.state.showDiff ? artifacts[0].DiffImages : artifacts[0].TestingImages;
        
        const testingImage = collection[this.state.imageIndex];
        
        const stableImage = artifacts[0].StableImages[this.state.imageIndex];

        return(
            <div className="flexChild columnParent"> 
                <nav className="navbar navbar-default">
                    <div className="container-fluid">
                        <NavigationResultBar 
                            clickEvent={this.handleChildClick}
                            showDiff = {this.state.showDiff}
                            testName = {testName}
                        />
                    </div>
                </nav>
                <TestResultContainer left={stableImage.RelativePath} right={testingImage.RelativePath}/>
            </div>
        );
    }
});

ReactDOM.render(
    <PageContent url="http://localhost:9111/tests/fails"/>,
    document.getElementById('root')
)

