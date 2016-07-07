import React from 'react'

import NavigationResultBar from './NavigationResultBar'
import TestResultContainer from './TestResultContainer'

var fakePreviewData = [{"TestName":"","Artifacts":[{"StableFile":{"Name":"","Path":"","RelativePath":""},"TestingFile":{"Name":"","Path":"","RelativePath":""},"DiffFile":{"Name":"","Path":"","RelativePath":""}}]}]

export default React.createClass({
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
        this.loadDataFromServer();
    },
    componenWillUnmount: function() {
        $(document.body).off('keydown', this.handleKeyDown);
    },
    loadDataFromServer: function() {
        var projectId = this.props.params.projectId;
        var suiteId = this.props.params.suiteId;

        var url = "/api/" + projectId + "/" + suiteId + "/tests";
        console.log(location.host);
        $.ajax({
            url: url,
            dataType: 'json',
            cache: false,
            success: function(data) {
                var artifacts = data[0].Artifacts;
                var imageName = this.getCurrentImageName(artifacts[0]);
                var maxImages = artifacts.length;
                var maxTests = data.length;
                var hasDiff = artifacts[0].DiffFile.Name !== "";

                this.setState({
                    testData: data,
                    maxImages: maxImages,
                    imageName: imageName,
                    maxTests: maxTests,
                    showDiff: false,
           	        hasDiff: hasDiff,
                })
            }.bind(this),
            error: function(xhr, status, err) {
                console.error(this.props.url, status, err.toString());
            }.bind(this)
        });
    },
    getInitialState: function() {
        var fakeData = fakePreviewData;
        
        var artifacts = fakeData[0].Artifacts;
        
        var imageName = this.getCurrentImageName(artifacts[0]);
        var maxImages = artifacts.length;
        var maxTests = fakeData.length;
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
            testData: fakeData
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
        //[Route("{projectId}/{suiteId}/{testId}/accept")]
        if (id === "acceptFail") {
            var testName = this.state.testData[this.state.testIndex].TestName;
            var projectName = this.props.params.projectId;
            var suiteName = this.props.params.suiteId;
            if(testName === "") return;
            
            var url = '../api/' + projectName + "/" + suiteName + '/' + testName + '/accept';
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
                testingImagePath = "..\\images\\nodiff.png";
            } else {
                testingImagePath = "..\\images\\notesting.png"
            }
        }

        const stableImage = artifacts[this.state.imageIndex].StableFile;
        var stableImagePath
        if (stableImage.RelativePath !== "") {
            stableImagePath = stableImage.RelativePath;
        } else {
            stableImagePath = "..\\images\\nostable.png";
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
                            showAcceptReject = {this.state.hasDiff}
                        />
                    </div>
                </nav>
                <TestResultContainer left={stableImagePath} right={testingImagePath} scrollEvent={this.handleScroll}/>
            </div>
        );
    }
});