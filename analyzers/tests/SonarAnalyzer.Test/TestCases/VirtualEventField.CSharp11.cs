using System;

public interface IVirtualEvent
{
    public static virtual event EventHandler OnRefueled; // Noncompliant {{Remove this 'virtual' modifier of 'OnRefueled'.}}
}
