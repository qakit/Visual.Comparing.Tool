import React from 'react'

import NavigationResultBar from './NavigationResultBar'
import TestResultContainer from './TestResultContainer'

var fakePreviewData = [{"TestName":"","Environment":{"Id":0,"Browser":"","WindowSize":""},"Artifacts":[{"StableFile":{"Name":"","Value":""},"TestingFile":{"Name":"","Value":""},"DiffFile":{"Name":"","Value":""}}]}]

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
    loadDataFromServer: function(testIndex, imageIndex) {
        var projectId = this.props.params.projectId;
        var suiteId = this.props.params.suiteId;
        var correctTestIndex = testIndex ? testIndex : 0;
        var correctImageIndex = imageIndex ? imageIndex : 0;

        var url = "/api/" + projectId + "/" + suiteId + "/tests";
        $.ajax({
            url: url,
            dataType: 'json',
            cache: false,
            success: function(data) {
                var artifacts = data[correctTestIndex].Artifacts;

                var imageName = this.getCurrentImageName(artifacts[correctImageIndex]);
                var maxImages = artifacts.length;
                var maxTests = data.length;
                var hasDiff = artifacts[correctImageIndex].DiffFile.Name !== "";
                this.setState({
                    testData: data,
                    maxImages: maxImages,
                    imageName: imageName,
                    maxTests: maxTests,
                    showDiff: false,
           	        hasDiff: hasDiff,
                    testIndex: correctTestIndex,
                    imageIndex: correctImageIndex
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
            testData: fakeData,
            testsTreeViewState: "collapsed"
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
            var environmentId = this.state.testData[this.state.testIndex].Environment.Id;
            var projectName = this.props.params.projectId;
            var suiteName = this.props.params.suiteId;
            if(testName === "") return;
            
            var url = '../api/' + projectName + "/" + suiteName + '/' + testName + '/' + environmentId + '/accept';
            $.ajax({
                url: url,
                dataType: 'text',
                type: 'POST',
                cache: false,
                success: function(data) {
                    //Todo avoid this in future. Just update stable image shown in the left pane
                    //possibly add new states for right and left images
                    this.loadDataFromServer(this.state.testIndex, this.state.imageIndex);
                }.bind(this),
                error: function(xhr, status, err) {
                    console.error(url, status.err.toString());
                }.bind(this)
            });
            return;
        }
        if (id === "toggleList") {
            var newPanelState = this.state.testsTreeViewState === "collapsed" ? "" : "collapsed"
            this.setState({
                testsTreeViewState: newPanelState
            });
            return;
        }

        if( id === "back") {
            $(document.body).off('keydown', this.handleKeyDown);
            window.history.back();
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
    handleTestItemClick: function(event){
        var parent = event.target.parentNode;

        this.setState({
            testIndex: parent.id,
            imageIndex: 0
        });
    },
    render: function() {
        const {TestName: testName, Artifacts: artifacts} = this.state.testData[this.state.testIndex];
        const testingImage = this.state.showDiff ? artifacts[this.state.imageIndex].DiffFile : artifacts[this.state.imageIndex].TestingFile;
        const stableImage = artifacts[this.state.imageIndex].StableFile;
        const {Browser: browser, WindowSize: windowSize} = this.state.testData[this.state.testIndex].Environment;
        const imageName = this.state.imageName;

        var hasTesting = testingImage.Value !== "";
        var hasStable = stableImage.Value !== "";

        var testingImagePath;
        if (hasTesting) {
            testingImagePath = testingImage.Value;
        } else {
            testingImagePath = this.state.showDiff ? "..\\images\\nodiff.png" : "..\\images\\notesting.png";
        }
        
        var stableImagePath;
        if (hasStable) {
            stableImagePath = stableImage.Value;
        } else {
            stableImagePath = "..\\images\\nostable.png";
        }

        //we need to show accept reject if test result has stable image and has diff (so stable can be updated)
        //if there is no stable (first run or any other reason) and has testing image (no matter has diff or not)
        var showAcceptReject = ((hasStable && this.state.hasDiff) || (!hasStable && hasTesting))

        const acceptClass = `accept${!showAcceptReject && ' disabled' || ''}`;
        const rejectClass = `reject${!showAcceptReject && ' disabled' || ''}`;

        const { imageIndex, maxImages, hasDiff, showDiff, testData, testIndex } = this.state;        
        const diffClass = !hasDiff && 'disabled';
        const diffIconClass = showDiff ? "fa fa-eye-slash" : "fa fa-eye";
        const testItemSelectedClass = function(i){
            return testIndex === i && 'selected' || '';
        }

        return (
            <div className="flexChild columnParent preview"> 
                <nav className="navbar navbar-default">
                    <div className="container-fluid">
                        <div className="navbar navbar-default history" id="bs-example-navbar-collapse-1">
                            <ul className="nav navbar-nav">
                                <li><a href="#" id="back" onClick={this.handleChildClick}><i id="back" className="fa fa-arrow-left"></i></a></li>
                                <li><p className="navbar-text">{`${testName} - ${imageName} [Browser: ${browser}; WindowSize: ${windowSize}]`}</p></li>
                            </ul>
                            <ul id="toggleList" className="nav navbar-nav navbar-right">
                                <li><a href="#" id="toggleList" onClick={this.handleChildClick}><i id="toggleList" className="fa fa-list"></i></a></li>
                            </ul>
                        </div>
                    </div>
                </nav>
                <div className="flexChild rowParent">
                    <TestResultContainer left={stableImagePath} right={testingImagePath} scrollEvent={this.handleScroll}/>                
                    <div className={`strange-panel ${this.state.testsTreeViewState} `}>
                        {testData.map((data, i) => <a key={`${data.TestName} - ${data.Environment.Browser}|${data.Environment.WindowSize}`} href="#" onClick={this.handleTestItemClick} id={parseInt(i)} className={`${testItemSelectedClass(i)}`}>
                            <span>{(i+1)}</span>{data.TestName}</a>)}
                    </div>                    
                </div>
                <div className="control-panel">
                    <a href="#" id="previousTest" onClick={this.handleChildClick}><i id="previousTest" className="fa fa-step-backward"></i></a>
                    <a href="#" id="previousFail" onClick={this.handleChildClick}><i id="previousFail" className="fa fa-backward"></i></a>
                    <span>{`${imageIndex + 1} / ${maxImages}`}</span>
                    <a href="#" id="nextFail" onClick={this.handleChildClick}><i id="nextFail" className="fa fa-forward"></i></a>
                    <a href="#" id="nextTest" onClick={this.handleChildClick}><i id="nextTest" className="fa fa-step-forward"></i></a>
                    <div></div>
                    <a href="#" id="showDiff" className={diffClass} onClick={this.handleChildClick}><i id="showDiff" className={diffIconClass}></i></a>                    
                    <div></div>
                    <a href="#" id="acceptFail" className={acceptClass} onClick={this.handleChildClick}><i id="acceptFail" className="fa fa-check"></i></a>
                    <a href="#" id="rejectFail" className={rejectClass} onClick={this.handleChildClick}><i id="rejectFail" className="fa fa-ban"></i></a>
                </div>
                 {(!this.state.testData || this.state.testData[0].TestName === "") && <div className="loader">
                    <div className="cssload-loader">
                        <div className="cssload-inner cssload-one"></div>
                        <div className="cssload-inner cssload-two"></div>
                        <div className="cssload-inner cssload-three"></div>
                    </div>
                </div>}
            </div>
        );
    }
});