import React from 'react'

export default React.createClass({
    render: function() {
        var base64Matcher = new RegExp("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{4})$");
        
        const leftImagePrefix = base64Matcher.test(this.props.left) ? "data:image/png;base64," : "";
        const rigthImagePrefix = base64Matcher.test(this.props.right) ? "data:image/png;base64," : "";

        return (
            <div className="flexChild rowParent">
                <div id="leftImage" onScroll={this.props.scrollEvent} className="flexChild">
                    <img src={`${leftImagePrefix} ${this.props.left}`}/>
                </div>
                <div id="rightImage" onScroll={this.props.scrollEvent} className="flexChild">
                    <img src={`${rigthImagePrefix} ${this.props.right}`} />
                </div>
            </div>
        );
    }
});