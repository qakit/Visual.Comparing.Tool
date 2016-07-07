import React from 'react'

export default React.createClass({
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