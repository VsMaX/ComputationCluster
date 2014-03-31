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
            computationalClient.Connect("192.168.0.100");
            //byte[] msg = Encoding.UTF8.GetBytes(wiadomosc.Text);
            //SolveRequest msgs = new SolveRequest();
            //msgs.Data = msg;

            //SolveRequestMessage sr = new SolveRequestMessage()
            //    { Data = null, ProblemType = "Ciężki problem", SolvingTimeout = 10000, SolvingTimeoutSpecified = true };
            //computationalClient.SendSolveRequest(sr);


            button1.ClickMode = ClickMode.Release;
            SolveRequestMessage sr2 = computationalClient.ReceiveDataFromServer();
            potwierdzenie.Text = sr2.ProblemType.ToString() + " " + sr2.SolvingTimeout.ToString() + " ";
        }
    }
}
