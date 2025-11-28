using System.Runtime.InteropServices;

void TopLevelMethod([DefaultParameterValue(5)] int j) { } //Noncompliant
void TopLevelOptional([DefaultParameterValue(5), Optional] int j) { }
