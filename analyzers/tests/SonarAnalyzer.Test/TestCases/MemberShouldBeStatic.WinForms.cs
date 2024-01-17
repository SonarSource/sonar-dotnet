using System;
using System.Windows.Forms;

namespace MemberShouldBeStatic
{
    public class MyForm : IContainerControl
    {
        public Control ActiveControl { get; set; }

        public bool ActivateControl(Control active) { return true; }

        private void MethodWith1Argument(object sender) { Handle(); } // Noncompliant

        private void MethodWith3Argument(object sender, MyEventArgs e, string other) { Handle(); } // Noncompliant
        
        private void SenderArgumentNotObject(string input, MyEventArgs e) { Handle(); } // Noncompliant

        private void EventArgs_Handler(object sender, EventArgs e) { Handle(); } // Compliant

        private void MyEventArgs_Handler(object sender, MyEventArgs e) { Handle(); } // Compliant

        static void Handle() { }
    }

    public class NoForm 
    {
        private void EventArgs_Handler(object sender, EventArgs e) { Handle(); } // Noncompliant

        private void MyEventArgs_Handler(object sender, MyEventArgs e) { Handle(); } // Noncompliant

        static void Handle() { }
    }

    public class MyEventArgs : EventArgs { }
}
