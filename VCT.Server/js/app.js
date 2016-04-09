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
        const imageIndex = this.props.currentImageIndex + 1;
        return(
            <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul className="nav navbar-nav">
                    <li><a href="#" id="previousTest" onClick={this.props.clickEvent}><i  id="previousTest" className="fa fa-step-backward"></i></a></li>
                    <li><a href="#" id="previousFail" onClick={this.props.clickEvent}><i id="previousFail" className="fa fa-backward"></i></a></li>
                    <li><p className="navbar-text">{imageIndex} / {this.props.maxImages}</p></li>
                    <li><a href="#" id="nextFail" onClick={this.props.clickEvent}><i id="nextFail" className="fa fa-forward"></i></a></li>
                    <li><a href="#" id="nextTest" onClick={this.props.clickEvent}><i id="nextTest" className="fa fa-step-forward"></i></a></li>
                    <li><a href="#" id="showDiff" onClick={this.props.clickEvent}><i id="showDiff" className={diffIconClass}></i></a></li>
                    <li><p className="navbar-text">{this.props.testName}</p></li>
                </ul>
                <ul className="nav navbar-nav navbar-right">
                    <li><a href="#" id="acceptFail" onClick={this.props.clickEvent}><i id="acceptFail" className="fa fa-check"></i></a></li>
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
                <div id="leftImage" className="flexChild">
                    <img src={this.props.left}/>
                </div>
                <div id="rightImage" className="flexChild">
                    <img src={this.props.right} />
                </div>
            </div>
        );
    }
});

var PageContent = React.createClass({
    loadDataFromServer: function() {
        $.ajax({
            url: this.props.url,
            dataType: 'json',
            cache: false,
            success: function(data) {
                var maxTests = data.length;
                
                var testingImages = data[0].Artifacts[0].TestingImages;
                var initialImageName = testingImages.length > 0 ? testingImages[0].Name : "";
                
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
        $(document.body).on('keydown', this.handleKeyDown);
        this.loadDataFromServer(); 
    },
    componenWillUnmount: function(){
        $(document.body).off('keydown', this.handleKeyDown);  
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
        var imageIndex = this.state.imageIndex;
        var showDiff = this.state.showDiff;
        var testIndex = this.state.testIndex;
        var currentImageName = this.state.imageName;
        var maxImages = this.state.maxImages;
        var id;
        if(typeof event === "string"){
            id = event;
        }else{
            id = event.target.id;
        }
                
        if(id === "showDiff"){
            showDiff = !this.state.showDiff;
        }
        if(id === "nextFail"){
            if(this.state.imageIndex === (this.state.maxImages - 1)) return;
            
            imageIndex = this.state.imageIndex + 1;
            showDiff = false;
            
            var testingImages = this.state.testData[this.state.testIndex].Artifacts[0].TestingImages;
            currentImageName = testingImages.length > 0 ? testingImages[imageIndex].Name : "";
        }
        if(id === "previousFail"){
            if(this.state.imageIndex === 0) return;
            imageIndex = this.state.imageIndex - 1,
            showDiff = false
            
            var testingImages = this.state.testData[this.state.testIndex].Artifacts[0].TestingImages;
            currentImageName = testingImages.length > 0 ? testingImages[imageIndex].Name : "";
        }
        if(id === "previousTest"){
            if(this.state.testIndex === 0) return;
            
            imageIndex = 0;
            showDiff = false;
            testIndex = this.state.testIndex - 1;
            
            var testingImages = this.state.testData[testIndex].Artifacts[0].TestingImages;
            
            maxImages = testingImages.length;
            currentImageName = testingImages.length > 0 ? testingImages[imageIndex].Name : "";
        }
         if(id === "nextTest"){
            if(this.state.testIndex === (this.state.maxTests - 1)) return;
            
            testIndex = this.state.testIndex + 1;
            imageIndex = 0;
            showDiff = false;
            maxImages = this.state.testData.length;
            
            var testingImages = this.state.testData[testIndex].Artifacts[0].TestingImages;
            
            maxImages = testingImages.length;
            currentImageName = testingImages.length > 0 ? testingImages[imageIndex].Name : "";
        }
        if(id === "acceptFail"){
            var url = 'tests//' + this.state.testData[this.state.testIndex].TestName + '//stable';
            $.ajax({
                url: url,
                dataType: 'text',
                type: 'POST',
                cache: false,
                success: function(data){
                    this.loadDataFromServer();
                }.bind(this),
                error: function(xhr, status, err){
                    console.error(url, status.err.toString());
                }.bind(this)
            });
            return;
        }
        
        this.setState({
            imageIndex: imageIndex,
            showDiff: showDiff,
            testIndex : testIndex,
            imageName: currentImageName,
            maxImages: maxImages
        })
    },
    handleKeyDown: function(event){
        //rigth arrow
        if(event.keyCode === 39){
            this.handleChildClick("nextFail");
            return;
        }
        //left arrow
        if(event.keyCode === 37){
            this.handleChildClick("previousFail");
            return;
        }
        //up arrow
        if(event.keyCode === 38){
            this.handleChildClick("previousTest");
            return;
        }
        //down arrow
        if(event.keyCode === 40){
            this.handleChildClick("nextTest");
            return;
        }
        //space
        if(event.keyCode === 32){
            this.handleChildClick("showDiff");
            return;
        }
    },
    render: function(){
        const {TestName: testName, Artifacts: artifacts} = this.state.testData[this.state.testIndex];
        const collection = this.state.showDiff ? artifacts[0].DiffImages : artifacts[0].TestingImages;
        
        const imageName = this.state.imageName;

        const testingImage = collection.filter(function(element){
            return element.Name === imageName
        })[0];
        
        var testingImagePath;
        if(testingImage){
            testingImagePath = testingImage.RelativePath;
        } else{
            if(this.state.showDiff){
                testingImagePath = "images\\nodiff.png";
            } else{
                testingImagePath = "images\\notesting.png"
            }
        }
                
        const stableImage = artifacts[0].StableImages.filter(function(element){
            return element.Name === imageName;
        })[0];
        var stableImagePath = stableImage ? stableImage.RelativePath : "images\\nostable.png";

        return(
            <div className="flexChild columnParent"> 
                <nav className="navbar navbar-default">
                    <div className="container-fluid">
                        <NavigationResultBar 
                            clickEvent={this.handleChildClick}
                            showDiff = {this.state.showDiff}
                            testName = {testName}
                            currentImageIndex = {this.state.imageIndex}
                            maxImages = {this.state.maxImages}
                        />
                    </div>
                </nav>
                <TestResultContainer left={stableImagePath} right={testingImagePath}/>
            </div>
        );
    }
});

ReactDOM.render(
    <PageContent url="http://192.168.33.21:9111/tests/fails"/>,
    document.getElementById('root')
)

var $imageContainers = $("#leftImage, #rightImage");

var sync = function(e){
    var $other = $imageContainers.not(this).off('scroll'), other = $other.get(0);
        
    var verticalPercentage = this.scrollTop / (this.scrollHeight - this.offsetHeight);
    var horizontalPercentage = this.scrollLeft / (this.scrollWidth - this.offsetWidth);
    other.scrollLeft = horizontalPercentage * (other.scrollWidth - other.offsetWidth);
    other.scrollTop = verticalPercentage * (other.scrollHeight - other.offsetHeight);
    
    setTimeout( function(){ $other.on('scroll', sync ); },10);
}

$imageContainers.on('scroll', sync);