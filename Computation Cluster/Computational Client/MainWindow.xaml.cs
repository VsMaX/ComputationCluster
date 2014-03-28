using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Communication_Library;

namespace Computational_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var computationalClient = new ComputationClient();
            computationalClient.Connect("192.168.110.38");
            string msg = wiadomosc.Text;
            SolveRequestMessage msgs = new SolveRequestMessage();
            msgs.Data = msg;
            computationalClient.SendSolveRequest(msgs);
            button1.ClickMode = ClickMode.Release;
            potwierdzenie.Text = computationalClient.ReceiveDataFromServer();
        }
    }
}
