var express = require('express');
var app = express();

app.set('port', (process.env.PORT || 3001));

app.use('/', express.static(__dirname));

app.use(function(req, res, next){
    res.setHeader('Access-Control-Allow-Origin', '*');

    // Disable caching so we'll always get the latest comments.
    res.setHeader('Cache-Control', 'no-cache');
    next();
});

app.listen(app.get('port'), function(){
    console.log('Server started at: http://localhost:' + app.get('port') + '/');
});