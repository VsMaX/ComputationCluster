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
using System.IO;

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
            var computationalClient = new ComputationClient("127.0.0.1", 5555, 5000);
            byte[] msg = Encoding.UTF8.GetBytes(wiadomosc.Text);

            var sr = new SolveRequestMessage() {Data = null, ProblemType = "DVRP", SolvingTimeout = 5000};
            computationalClient.SendSolveRequest(sr);
            button1.ClickMode = ClickMode.Release;
            string sr2 = computationalClient.ReceiveDataFromServer();
            potwierdzenie.Text = sr2;
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            var computationalClient = new ComputationClient("127.0.0.1", 5555, 5000);

            var solutionRequest = new SolutionRequestMessage() { Id = 4 };
            computationalClient.SendSolutionRequest(solutionRequest);

            string sr3 = computationalClient.ReceiveDataFromServer();
            potwierdzenie.Text = sr3; 
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text documents (.txt)|*.txt|XML files (*.xml)|*.xml";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                string content = File.ReadAllText(filename);
                wiadomosc.Text = content;

                CommunicationModule cm = new CommunicationModule("127.0.0.1", 5555, 5000);
                var socket = cm.SetupClient();
                cm.Connect(socket);
                SolveRequestMessage solveRequestMessage = new SolveRequestMessage()
                {
                    Data = CommunicationModule.ConvertStringToData(content),
                    ProblemType = "DVRP",
                    SolvingTimeout = 100000
                };
                BaseNode bn = new BaseNode();
                cm.SendData(bn.SerializeMessage(solveRequestMessage), socket);
                cm.CloseSocket(socket);
            }
        }
    }
}
