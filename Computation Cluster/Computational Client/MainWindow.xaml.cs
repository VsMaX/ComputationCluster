﻿using System;
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
using System.Threading;
using System.ComponentModel;

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
            this.defaultGrid.DataContext = this.textBoxContent;
        }

        public SolveRequestMessage solveRequestMessage;

        public ComputationClient computationalClient;

        public BaseNode bn = new BaseNode();

        public ulong problemId;

        public TextBoxContent textBoxContent = new TextBoxContent()
            {
                Content = "potwierdzenie od serwera"
            };

        private void ButtonSendSolutionRequest_Click(object sender, RoutedEventArgs e)
        {
            //byte[] msg = Encoding.UTF8.GetBytes(wiadomosc.Text);

            //var sr = new SolveRequestMessage() { Data = null, ProblemType = "DVRP", SolvingTimeout = 5000 };
            //this.computationalClient.SendSolveRequest(sr);

            //buttonSendSolutionRequest.ClickMode = ClickMode.Release;

            //string sr2 = this.computationalClient.ReceiveDataFromServer();
            //potwierdzenie.Text = sr2;

            //
            SolutionRequestMessage srm = new SolutionRequestMessage()
                {
                    Id = this.problemId
                };

            SolutionsMessage sm = null;
            String message = String.Empty;

            while(sm == null)
            {
                message = this.computationalClient.SendSolutionRequest(srm);
                if(message != String.Empty)
                {
                    switch (bn.GetMessageName(message))
                    {
                        case "Solutions":
                            var serializer = new ComputationSerializer<SolutionsMessage>();
                            sm = serializer.Deserialize(message);
                            foreach (var solution in sm.Solutions)
                            {
                                if (solution.Type == SolutionType.Final)
                                {
                                    this.potwierdzenie.Text += "\n\nFinal solution: "
                                                                   + System.Text.Encoding.UTF8.GetString(solution.Data);
                                    break;
                                }
                            }

                            sm = null;
                            break;

                        case "Other...?":
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    this.potwierdzenie.Text += "\n\n Message empty";
                }
                this.potwierdzenie.Text += "\n\n Computing...";
                Thread.Sleep(10000);
            }
        }

        private void ButtonSendSolveRequest_Click(object sender, RoutedEventArgs e)
        {
            var message = this.computationalClient.SendSolveRequest(this.solveRequestMessage);
            this.potwierdzenie.Text += "\nReceived form CC" + message;

            var serializer = new ComputationSerializer<SolveRequestResponseMessage>();
            SolveRequestResponseMessage srrm = serializer.Deserialize(message);
            this.problemId = srrm.Id;
            this.potwierdzenie.Text += "\n\nAssigned problem ID = " + this.problemId;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                string content = File.ReadAllText(filename);
                wiadomosc.Text = content;

                //CommunicationModule cm = new CommunicationModule("127.0.0.1", 5555, 5000);
                //var socket = cm.SetupClient();
                //cm.Connect(socket);
                
                this.solveRequestMessage = new SolveRequestMessage()
                {
                    Data = CommunicationModule.ConvertStringToData(content),
                    ProblemType = "DVRP",
                    SolvingTimeout = long.Parse(this.timeoutTextBox.Text)
                };

                this.computationalClient = new ComputationClient("127.0.0.1", 5555, 2000);
                //BaseNode bn = new BaseNode();

                //cm.SendData(bn.SerializeMessage(solveRequestMessage), socket);
                //cm.CloseSocket(socket);
            }
        }
    }
}
