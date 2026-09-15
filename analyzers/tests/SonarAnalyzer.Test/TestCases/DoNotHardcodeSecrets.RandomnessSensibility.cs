public class Program
{
    // 'randomnessSensibility' is set to 5.2 in SonarLint.xml, so only secrets with a higher Shannon entropy are reported.

    // Entropy 5.63, above the configured sensibility.
    string auth = "rf6acB24J//1FZLRrKpjmBUYSnUX5CHlt/iD5vVVcgVuAIOB6hzcWjDnv16V6hDLevW0Qs4hKPbP1M4YfuDI16sZna1/VGRLkAbTk6xMPs4epH6A3ZqSyyI-H92y"; // Noncompliant

    // Entropy 5.01, below the configured sensibility. This one is reported when the default sensibility of 3 is used.
    string token = "1IfHMPanImzX8ZxC-Ud6+YhXiLwlXq$f_-3v~.="; // Compliant
}
