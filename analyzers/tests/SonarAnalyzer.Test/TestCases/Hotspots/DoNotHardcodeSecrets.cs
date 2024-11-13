using System;

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

    string varString = "token=rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; // Noncompliant {{"token" detected here, make sure this is not a hard-coded secret.}}
    string secretString = "token=rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; // Noncompliant {{"secret and token" detected here, make sure this is not a hard-coded secret.}}

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
        public void SeaShark7VariableDeclaration()
        {
            api.key = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="; // Compliant

            var d_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant
            d_auth += "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant
            d_auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Noncompliant

            var c_auth = "a";
            c_auth = c_auth + "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y";  // Compliant does not compile to a constant
        }
    }

    public class Scaffold
    {
        public string key { get; set; }
    }
}
