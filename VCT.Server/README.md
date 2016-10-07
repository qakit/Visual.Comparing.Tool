# Configuring Server

## Building soluition using VS

1. Run solution application
2. Right-click on it and check 'Enable NuGet package Restore'
3. Build solution

## Building bundle.js for main server page
To be able preview results of testing it's necessary to build bundle.js file. For now it could be done
only manually. To build it you have to install node.js (with npm). After latest npm version will be available
run cmd and call

* `npm install` - this will install all necessary dependencies;
* `npm run dev` - to build dev version of bundle.js (helps to debug things if necessary) with hotwatch func;
* `npm run production` - production bundle.js file version (compressed/minified).

## Building solution using NAnt
Navigate back to VCT folder with `nant.build` file inside it. Run cmd and call `nant server` target. 
After that you'll get all necessary libs in 'VCT.Server/Deploy' directory.

After all steps above you need to configure and patch App.config file.

## App.config configuration

App.config contains main settings and allow configure folder in which files will be stored, main app url.

* `rootUrl` - url which will be used to run server (it should be available for any test)
* `rootDir` - main server folder
* `storage` - local folder which will store all testing files

## Running app
After building solution using VS or NAnt (and buliding bundle.js file) run server application.
After that navigate to the url specified in the app.config file and you'll see main app page
with projects information. Here in future you'll see list of projects in which you use VCT.

## How it looks like...

![ProjectsPagePreview] (/docimages/ProjectsPagePreview.PNG "Projects Page Preview")
