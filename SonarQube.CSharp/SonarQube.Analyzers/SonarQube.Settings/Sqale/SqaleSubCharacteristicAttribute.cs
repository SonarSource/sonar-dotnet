using System;

namespace SonarQube.Analyzers.SonarQube.Settings.Sqale
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