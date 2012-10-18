using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace BlankClassLibrary
{
    public class DummyService
    {
        public string SayHello(string name)
        {
            string result = "Hello " + name + "  " + new DateTime();
            try
            {
                dynamic toto = "eee";
                toto.MethodThatDoesNotExist();
            }
            catch (Exception e)
            {
                result += e;
            }
            return result;
        }

    }
}
