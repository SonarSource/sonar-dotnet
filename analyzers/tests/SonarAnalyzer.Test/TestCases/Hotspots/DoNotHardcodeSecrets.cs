using System;
using System.Collections.Generic;

public class Program
{
    string auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant {{"auth" detected here, make sure this is not a hard-coded secret.}}
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    string authWithBackSlash = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant 
    string authStartBackSlash = @"\mf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; //Compliant starts with backslash
    string authBackSlashIsEscape = "rf6acB24J//1FZLRrKpj\tmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; // Compliant has escape char

    string willNotRaise = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; // Compliant, name doesnt match
    string authLowEntropy = "aaaaaaaaaaaaaaaaaaaa"; // Compliant, low entropy

    string secret = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.=" + "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.=";     // Noncompliant
    string token = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.=";      // Noncompliant
    string credential = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="; // Noncompliant
    string api_key = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.=";    // Noncompliant

    string END_TOKEN = "EndGlobalSection"; // Compliant
    string AccessControlAllowCredentials = "Access-Control-Allow-Credentials"; // Compliant
    string AuthenticatorApp = "OrchardCore.Users.2FA.AuthenticatorApp"; // Compliant
    string BackOfficeTwoFactorRememberMeAuthenticationType = "UmbracoTwoFactorRememberMeCookie"; // Compliant
    string RequestVerificationToken = "__RequestVerificationToken";
    string TokenPasswordResetCode = "PasswordResetCode"; // Compliant
    string AuthenticationServiceName = "marketplacecommerceanalytics";
    string XAmzSecurityToken = "X-Amz-Security-Token";
    string ProductPublicKeyToken = "31bf3856ad364e35"; // Compliant, in Banlist
    static string OfficialDesktopPublicKeyToken = "b03f5f7f11d50a3a"; // Compliant, in Banlist
    string VisualBasicAssemblyPublicKeyToken = "PublicKeyToken=" + OfficialDesktopPublicKeyToken; // Compliant as it has 'Token' in name AND in value

    string varStrings = "PublicKeyToken=31bf3856ad364e35;"; // Compliant, in Banlist
    string varStringer = "PublicKeyToken=31bf3856ad364e35; Secret=31bf3856ad364e35"; // Noncompliant

    string varString = "token=rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; // Noncompliant {{"token" detected here, make sure this is not a hard-coded secret.}}
    string secretString = "token=rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; // Noncompliant {{"secretString" detected here, make sure this is not a hard-coded secret.}}

    public class Test
    {
        Scaffold api = new Scaffold();
        string p_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant

        string x_auth { get; set; } = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant

        string y_auth
        {
            get => "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant
            set => y_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant
        }

        string z_auth
        {
            get
            {
                return "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // FN
            }
            set
            {
                z_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant
            }
        }

        public void SeaShark7VariableDeclaration()
        {
            api.key = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="; // Compliant

            var d_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant
            d_auth += "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant
            d_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant

            var c_auth = "a";
            c_auth = c_auth + "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Compliant does not compile to a constant

            var authToken = "";
            var shouldNotRaise = ":)";
            if (authToken == "31bf3856ad364e35") {}// Noncompliant
            else if ("b03f5f7f11d50a3a" == authToken) {} // Noncompliant
            else if(shouldNotRaise == "31bf3856ad364e35") {} // Compliant
            else if("31bf3856ad364e35" == shouldNotRaise) {} // Compliant
            if ("31bf3856ad364e35".Equals(authToken)){ } // Noncompliant
            if (authToken.Equals("31bf3856ad364e35")) { } // Noncompliant
            if ("31bf3856ad364e35".Equals(shouldNotRaise)){ } // Compliant
            if (shouldNotRaise.Equals("31bf3856ad364e35")) { } // Compliant

            "AuthToken".Equals("31bf3856ad364e35"); // Compliant
            authToken.Equals(shouldNotRaise); // Compliant
            shouldNotRaise.Equals(null); // Compliant
            "AuthToken".Equals(null); // Compliant

            "31bf3856ad364e35".Equals(authToken, StringComparison.CurrentCulture);      // Noncompliant
            authToken.Equals("31bf3856ad364e35", StringComparison.CurrentCulture);      // Noncompliant

            var bar = new Bar();
            var foo = new Foo();

            if (bar.test.test.ShouldNotRaise.Equals("31bf3856ad364e35")) { }        // Compliant
            if("31bf3856ad364e35".Equals(bar.test.test.ShouldNotRaise)) { }         // Compliant
            if (bar.test.test.Token.Equals("31bf3856ad364e35")) { }                 // Noncompliant
            if("31bf3856ad364e35".Equals(bar.test.test.Token)) { }                  // Noncompliant

            if (foo.GetToken().Equals("31bf3856ad364e35")) { }                      // Compliant not string.Equals
            if (foo.GetAuthToken().Equals("31bf3856ad364e35")) { }                  // Noncompliant
            if ("31bf3856ad364e35".Equals(foo.GetToken())) { }                      // Noncompliant
            if (foo.GetToken().test.Equals("31bf3856ad364e35")) { }                 // Compliant
            if ("31bf3856ad364e35".Equals(foo.GetToken().test)) { }                 // Compliant

            string.Equals("31bf3856ad364e35", authToken);                                                           // Compliant FN
            string.Equals(authToken, "31bf3856ad364e35");                                                           // Compliant FN
            string.Equals("31bf3856ad364e35", authToken, StringComparison.CurrentCulture);                          // Compliant FN
            string.Equals(comparisonType: StringComparison.CurrentCulture, a: "31bf3856ad364e35", b: authToken);    // Compliant FN
            StringComparer.InvariantCulture.Equals("31bf3856ad364e35", authToken);                                  // Compliant FN
            StringComparer.InvariantCulture.Equals(authToken, "31bf3856ad364e35");                                  // Compliant FN
            EqualityComparer<string>.Default.Equals("31bf3856ad364e35", authToken);                                 // Compliant FN
            EqualityComparer<string>.Default.Equals(authToken, "31bf3856ad364e35");                                 // Compliant FN
            if ("31bf3856ad364e35" is "authToken") { }                                                              // Compliant FN
            if("authToken" is "31bf3856ad364e35") { }                                                               // Compliant FN
            switch (authToken)
            {
                case "31bf3856ad364e35":                                                                            // Compliant FN
                    break;
            }
        }
    }

    public class Foo
    {
        public Bar GetToken() => new Bar();
        public string GetAuthToken() => "31bf3856ad364e35";
    }

    public class Bar
    {
        public Bar test = new Bar();
        public string ShouldNotRaise;
        public string Token;
    }

    public class Scaffold
    {
        public string key { get; set; }
    }
}
