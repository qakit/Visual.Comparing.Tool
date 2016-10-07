# Configuring and Working With Client

## Building soluction using VS
1. Run solution application
2. Right-click on it and check 'Enable NuGet package Restore'
3. Build solution

## Building solution using NAnt
Navigate back to VCT folder with `nant.build` file inside it. Run cmd and call `nant client` target. 
After that you'll get all necessary libs in 'VCT.Client/Deploy' directory.

## Using SDK in your tests library
1. Add reference in your test project to the VCT.Client library
2. Add using to `using VCT.Client.Modules`

Now you can use comparing method inside VCT.Client library which will compare your file with file on the server.

```

//Make screenshot of your page/element here
...
//Now compare your screenshot to the stable one on server
var comparingModule = new Comparing("TestProject");
//Passing testname is necessary. moreover this test name must be unique in scope of all
//visual tests
var equal = comparingModule.CompareImageToStable(outputScreenFile, testName);

//And you can use result to assert
Assert.IsTrue(equal, "Some message");
```

That's all. Pay attention - if you run test at first time and code is fine, it will fail anyway, because
you don't need stable version on the server. And you manually must apply or reject this version and to see
that really was stored on server.

## Functional buttons and the pictures
Once you enter in preview mode for all your tests in suite you can navigate between:
* Tests using up and down keyboard buttons or the same buttons in toolbar
* Images in scope of single test using right or left keyboard buttons or the same buttons in toolbar
* Testing/Difference images by pressing 'Space' keyboard button or using 'Eye' button in toolbar

From the left side you see 'Stable' image from the rigth side 'Testing' or 'Diff' image depending on what you
want to see.

## How it looks like
