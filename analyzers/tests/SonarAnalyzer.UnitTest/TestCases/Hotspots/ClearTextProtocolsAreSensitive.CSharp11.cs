using System;


const string a = """http://foo.com"""; // Noncompliant {{Using http protocol is insecure. Use https instead.}}

var b = "http://foo.com"u8; // FN
var c = """http://foo.com"""u8; // FN
var d = """
    http://foo.com
    """u8; // FN
