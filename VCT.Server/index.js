import React from 'react'
import { render } from 'react-dom'
import { Router, Route, browserHistory } from 'react-router'

import App from './modules/App'
import HistoryContent from './modules/HistoryContent'
import ResultsPreviewContent from './modules/ResultsPreviewContent'

require("./styles.scss")

render(
    <Router history={browserHistory}>
        <Route path="/" component={App}/>
        <Route path="/:projectId" component={HistoryContent}/>
        <Route path="/:projectId/:suiteId" component={ResultsPreviewContent}/>
    </Router>, document.getElementById('root'))
