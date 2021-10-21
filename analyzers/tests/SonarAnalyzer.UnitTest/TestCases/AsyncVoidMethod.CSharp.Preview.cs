using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Net6Poc
{
    internal class MsTestCases
    {
        // MSTest V1 doesn't have proper support for async so people are forced to use async void
        [Generic<int>]
        public void M()
        {
            [TestMethod] async void Get1() => await Task.FromResult(1);
            [TestMethod, GenericAttribute<int>] async Task Get1s() => await Task.FromResult(1);
            async void Get2() => await Task.FromResult(2); // Compliant - FN
            async Task Get2s() => await Task.FromResult(2);

            Action a =[TestMethod] async () => { };
            Action b = async () => { };  // Compliant - FN
            Action c = [Generic<int>] async () => { };  // Compliant - FN
            Func<Task> d =[TestMethod] async () => await Task.Delay(0);
            Func<Task> e = async () => await Task.Delay(0);
        }
    }

    public class GenericAttribute<T> : Attribute { }
}
