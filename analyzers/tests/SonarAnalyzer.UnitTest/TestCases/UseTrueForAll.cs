using System.ComponentModel;
using System.Text;

StringBuilder sb = new();

sb.ToString();//Noncompliant

sb.Append("").ToString();//Noncompliant
sb.Append("").Append("").ToString();//Noncompliant
sb.Append("").Append("").Append("").ToString();//Noncompliant

sb.ToString().ToLower();//Noncompliant
sb.ToString().ToLower().ToLower();//Noncompliant
sb.ToString().ToLower().ToLower().ToLower();//Noncompliant

sb.Append("").Append("").Append("").ToString().ToLower();//Noncompliant
sb.Append("").Append("").ToString().ToLower().ToLower();//Noncompliant
sb.Append("").ToString().ToLower().ToLower().ToLower();//Noncompliant

sb?.Append("").ToString();//Noncompliant
sb?.Append("").Append("").ToString();//Noncompliant
sb?.Append("").Append("").Append("").ToString();//Noncompliant

sb?.ToString().ToLower();//Noncompliant
sb?.ToString().ToLower().ToLower();//Noncompliant
sb?.ToString().ToLower().ToLower().ToLower();//Noncompliant

sb?.Append("").Append("").Append("").ToString().ToLower();//Noncompliant
sb?.Append("").Append("").ToString().ToLower().ToLower();//Noncompliant
sb?.Append("").ToString().ToLower().ToLower().ToLower();//Noncompliant

sb.Append("")?.ToString();//Noncompliant
sb.Append("")?.Append("").ToString();//Noncompliant
sb.Append("")?.Append("").Append("").ToString();//Noncompliant

sb.ToString()?.ToLower();//Noncompliant
sb.ToString()?.ToLower().ToLower();//Noncompliant
sb.ToString()?.ToLower().ToLower().ToLower();//Noncompliant

sb.Append("")?.Append("").Append("").ToString().ToLower();//Noncompliant
sb.Append("").Append("")?.ToString().ToLower().ToLower();//Noncompliant
sb.Append("").ToString().ToLower().ToLower()?.ToLower();//Noncompliant

sb.Append("")?.Append("").Append("").ToString().ToLower();//Noncompliant
sb.Append("").Append("")?.ToString().ToLower().ToLower();//Noncompliant
sb.Append("").ToString().ToLower().ToLower()?.ToLower();//Noncompliant


_ = sb.Append("").Append("").ToString().ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("").Append("").ToString().ToLower()?.ToUpperInvariant(); // Noncompliant
_ = sb.Append("").Append("").ToString()?.ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("").Append("").ToString()?.ToLower()?.ToUpperInvariant(); // Noncompliant
_ = sb.Append("").Append("")?.ToString().ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("").Append("")?.ToString().ToLower()?.ToUpperInvariant(); // Noncompliant
_ = sb.Append("").Append("")?.ToString()?.ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("").Append("")?.ToString()?.ToLower()?.ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("").ToString().ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("").ToString().ToLower()?.ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("").ToString()?.ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("").ToString()?.ToLower()?.ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("")?.ToString().ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("")?.ToString().ToLower()?.ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("")?.ToString()?.ToLower().ToUpperInvariant(); // Noncompliant
_ = sb.Append("")?.Append("")?.ToString()?.ToLower()?.ToUpperInvariant(); // Noncompliant
