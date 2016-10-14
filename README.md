# Visual Comparing Tool #

Visual Comparing Tool (VCT) - is a client-server application which aim to help QA members, and not only,
to perform visual comparison of pictures, screenshots which you can use in your tests. 
This tool based in idea of comparing between stable and testing images.
Stable Files/Images - are files which you as QA decide to be stable. 
E.g. you run test, after test fail (e.g. there is a difference between stable and testing images) you
go to server url and accept fail for this particular test. 
In this case this image becomes stable for this test. Testing images - any new image which produced 
by your test. More scenarios described below. VCT contains from two modules:

1. VCT.Server - server application which store your stable/testing/diff files from tests. 
Server has REST API which allow interact with it and get necessary file from server or put it to the server. More information in documentation.
2. VCT.Client - client library, which contains methods to work with server. These methods allow you to put/get files from/to server without knowing REST API and without producing your test logig. More information in documentation. 

Currently it's your responsibility to make screenshot, but lately this method can be moved to client library.

## Use-cases ##

For now only one use case of using this tool - it's comparing images e.g. screenshot based testing. 
Below sample usage scenario (more about [configuring server](/VCT.Server/README.md)/[client](/VCT.Client/README.md) described in documentation):

1. Run server on remote/local machine which is available from any place where you run your tests 
(e.g. in CI network);
2. Create test library. Add reference to client library (configure it, set server path);
3. Write test using client to store output screenshot on the server and compare it with existing stable screenshot;
4. As you run test at first time test will fail (because there is no stable version for your image);
5. After test failed, you go to the server url and observe test results in specified project;
6. You accept fail and set stable image(s) for this test
7. Run test again. If image will be changed test will fail. In this case you'll see diff file in preview reslts. More information in step by step tutorial.