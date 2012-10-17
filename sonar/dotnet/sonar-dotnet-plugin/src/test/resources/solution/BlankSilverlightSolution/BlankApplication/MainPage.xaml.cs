using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BlankClassLibrary;

namespace BlankApplication
{
    public partial class MainPage : UserControl
    {
        private DummyService service = new DummyService();

        public MainPage()
        {
            InitializeComponent();
        }

        private void Hello_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Browser.HtmlPage.Window.Alert(service.SayHello(sender.ToString())); 
        }
    }
}
