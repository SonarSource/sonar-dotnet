using System;
using System.IO;

var fileLiteral = """file://blah.txt"""; // Noncompliant {{Refactor your code not to use hardcoded absolute paths or URIs.}}
//                ^^^^^^^^^^^^^^^^^^^^^

var webUri1 = """http://www.mywebsite.com"""; // Noncompliant
var windowsDrivePath1 = """c:\\blah\\blah\\blah.txt"""; // Noncompliant
var windowsSharedDrivePath1 = """\\my-network-drive\folder\file.txt"""; // Noncompliant
var x = new Uri("""C:/test.txt"""); // Noncompliant

var unixPath1 = """/my/other/folder"""; // Compliant - we ignore unix paths by default
var unixPath2 = """~/blah/blah/blah.txt"""; // Compliant
var unixPath3 = """~\\blah\\blah\\blah.txt"""; // Compliant
var windowsPathStartingWithVariable = """%AppData%\\Adobe""";

var webChemin = """http://www.mywebsite.com"""; // FN
var windowsChemin = """c:\\blah\\blah\\blah.txt"""; // FN
