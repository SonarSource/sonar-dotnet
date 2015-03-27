using System;

namespace NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SqaleSubCharacteristicAttribute : Attribute
    {
        public SqaleSubCharacteristic SubCharacteristic { get; set; }

        public SqaleSubCharacteristicAttribute(SqaleSubCharacteristic subCharacteristic)
        {
            SubCharacteristic = subCharacteristic;
        }
    }
}