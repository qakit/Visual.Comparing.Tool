 var allData = [{
	TestName: "Test1",
	Artifacts: {
		DiffImages: [{
			Name: "Capture",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\DiffFiles\\Test1\\Capture.PNG",
			RelativePath: ".\\images\\DiffFiles\\Test1\\Capture.PNG"
		},
		{
			Name: "Capture1",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\DiffFiles\\Test1\\Capture1.PNG",
			RelativePath: ".\\images\\DiffFiles\\Test1\\Capture1.PNG"
		}],
		TestingImages: [{
			Name: "Capture",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\TestingFiles\\Test1\\Capture.PNG",
			RelativePath: ".\\images\\TestingFiles\\Test1\\Capture.PNG"
		},
		{
			Name: "Capture1",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\TestingFiles\\Test1\\Capture1.PNG",
			RelativePath: ".\\images\\TestingFiles\\Test1\\Capture1.PNG"
		}],
		StableImages: [{
			Name: "Capture",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\StableFiles\\Test1\\Capture.PNG",
			RelativePath: ".\\images\\StableFiles\\Test1\\Capture.PNG"
		},
		{
			Name: "Capture1",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\StableFiles\\Test1\\Capture1.PNG",
			RelativePath: ".\\images\\StableFiles\\Test1\\Capture1.PNG"
		}]
	}
},
{
	TestName: "Test2",
	Artifacts: {
		DiffImages: [{
			Name: "Capture",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\DiffFiles\\Test2\\Capture.PNG",
			RelativePath: ".\\images\\DiffFiles\\Test2\\Capture.PNG"
		},
		{
			Name: "Capture1",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\DiffFiles\\Test2\\Capture1.PNG",
			RelativePath: ".\\images\\DiffFiles\\Test2\\Capture1.PNG"
		}],
		TestingImages: [{
			Name: "Capture",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\TestingFiles\\Test2\\Capture.PNG",
			RelativePath: ".\\images\\TestingFiles\\Test2\\Capture.PNG"
		},
		{
			Name: "Capture1",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\TestingFiles\\Test2\\Capture1.PNG",
			RelativePath: ".\\images\\TestingFiles\\Test2\\Capture1.PNG"
		}],
		StableImages: [{
			Name: "Capture",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\StableFiles\\Test2\\Capture.PNG",
			RelativePath: ".\\images\\StableFiles\\Test2\\Capture.PNG"
		},
		{
			Name: "Capture1",
			Path: "D:\\C#Projects\\ReactTutorials\\maxfarseer.tutorial\\images\\StableFiles\\Test2\\Capture1.PNG",
			RelativePath: ".\\images\\StableFiles\\Test2\\Capture1.PNG"
		}]
	}
}];
            
var TestNameElement = React.createClass({
    render: function(){
        return (
            <div className="navbar-header">
                  <a className="navbar-brand">Something</a>
            </div>
        );
    }
})

var NavigationResultBar = React.createClass({
    render: function(){
        return(
            <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><a href="#" id="previousTest" onClick={this.props.clickEvent}><i  id="previousTest" className="fa fa-step-backward"></i></a></li>
                    <li><a href="#" id="previousFail" onClick={this.props.clickEvent}><i id="previousFail" className="fa fa-backward"></i></a></li>
                    <li><a href="#" id="nextFail" onClick={this.props.clickEvent}><i id="nextFail" className="fa fa-forward"></i></a></li>
                    <li><a href="#" id="nextTest" onClick={this.props.clickEvent}><i id="nextTest" className="fa fa-step-forward"></i></a></li>
                    <li><a href="#" id="showDiff" onClick={this.props.clickEvent}><i id="showDiff" className={this.props.diffIcon}></i></a></li>
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
})

var PageContent = React.createClass({
    getInitialState: function(){
        var maxTests = this.props.data.length;

        var testingImages = this.props.data[0].Artifacts.TestingImages;
        var initialImageName = testingImages[0].Name;
        return ({
            //index of the test in current fails
            testIndex: 0,
            //image index in the current test
            imageIndex: 0,
            //max images count
            maxImages: testingImages.length,
            maxTests: maxTests,
            imageName: initialImageName,
            //show diff image instead of stable or not
            showDiff: false,
            diffIconClass: "fa fa-eye"
        });
    },
    handleChildClick: function(event){
        if(event.target.id === "showDiff"){
            if(this.state.showDiff){
                this.setState({diffIconClass: "fa fa-eye", showDiff: false})
            } else{
                this.setState({diffIconClass: "fa fa-eye-slash", showDiff: true})
            }
            return;
        }
        if(event.target.id === "nextFail"){
            if(this.state.imageIndex === (this.state.maxImages - 1)) return;
            //todo handle end of the fails (our of range);            
            this.setState({
                imageIndex: this.state.imageIndex + 1,
                diffIconClass: "fa fa-eye",
                showDiff: false
            });
            return;
        }
        if(event.target.id === "previousFail"){
            if(this.state.imageIndex === 0) return;
            
            this.setState({
                imageIndex: this.state.imageIndex - 1,
                diffIconClass: "fa fa-eye",
                showDiff: false
            });
            return;
        }
        if(event.target.id === "previousTest"){
            if(this.state.testIndex === 0) return;
            
            this.setState({
                testIndex: this.state.testIndex - 1,
                imageIndex: 0,
                diffIconClass: "fa fa-eye",
                showDiff: false
            });
            return;
        }
         if(event.target.id === "nextTest"){
            if(this.state.testIndex === (this.state.maxTests - 1)) return;
            
            this.setState({
                testIndex: this.state.testIndex + 1,
                imageIndex: 0,
                diffIconClass: "fa fa-eye",
                showDiff: false
            });
            return;
        }
        console.log(event.target);
    },
    render: function(){
        var testData = this.props.data[this.state.testIndex];
        var testName = testData.TestName;
        var artifacts = testData.Artifacts;
        
        var stableImage;
        var testingImage;
        
        if(this.state.showDiff){
            testingImage = artifacts.DiffImages[this.state.imageIndex];
        } else{
            testingImage = artifacts.TestingImages[this.state.imageIndex];
        }
        stableImage = artifacts.StableImages[this.state.imageIndex];
        
              
        return(
            <div className="flexChild columnParent"> 
                <nav className="navbar navbar-default">
                    <div className="container-fluid">
                        <TestNameElement />
                        <NavigationResultBar 
                            clickEvent={this.handleChildClick}
                            diffIcon = {this.state.diffIconClass}
                        />
                    </div>
                </nav>
                <TestResultContainer left={stableImage.RelativePath} right={testingImage.RelativePath}/>
            </div>
        );
    }
});

ReactDOM.render(
    <PageContent data={allData}/>,
    document.getElementById('root')
)

