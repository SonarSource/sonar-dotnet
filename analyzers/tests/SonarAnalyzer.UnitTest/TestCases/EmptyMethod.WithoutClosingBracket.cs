using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class EmptyMethod
    {
        void F1() // Noncompliant
        {
        }

        void F2() // Noncompliant
        {
