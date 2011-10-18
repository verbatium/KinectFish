using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Ports;

namespace ShapeGame2
{
    /// <summary>
    /// Interaction logic for SerialConnector.xaml
    /// </summary>
    public partial class SerialConnector : Window
    {
        static SerialPort robotFishPort, feedbackPort;

        public SerialConnector()
        {
            InitializeComponent();
            RobotPortList.Items.Add(Properties.Settings.Default.RobotFishPort);
            RobotPortList.SelectedIndex = 0;
            FeedbackPortList.Items.Add(Properties.Settings.Default.FeedbackPort);
            FeedbackPortList.SelectedIndex = 0;

        }

        static public bool SetFanSpeeds(byte leftFan, byte rightFan)
        {
            if (feedbackPort == null)
                return false;
            if (!feedbackPort.IsOpen)
                return false;

            try
            {
                feedbackPort.WriteLine("164");
                feedbackPort.WriteLine(leftFan.ToString());
                feedbackPort.WriteLine("165");
                feedbackPort.WriteLine(rightFan.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return true;
        }
        private void RobotPortList_DropDownOpened(object sender, EventArgs e)
        {
            // Could this be done with data binding in XAML instead?
            RobotPortList.Items.Clear();
            foreach (string portName in SerialPort.GetPortNames())
                RobotPortList.Items.Add(portName);
        }

        private void FeedbackPortList_DropDownOpened(object sender, EventArgs e)
        {
            FeedbackPortList.Items.Clear();
            foreach (string portName in SerialPort.GetPortNames())
                FeedbackPortList.Items.Add(portName);

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.RobotFishPort = (string)RobotPortList.SelectedItem;
            Properties.Settings.Default.FeedbackPort = (string)FeedbackPortList.SelectedItem;
        }

        private void RobotConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                robotFishPort = new SerialPort((string)RobotPortList.SelectedItem, 115200, Parity.None, 8, StopBits.One);
                robotFishPort.NewLine = "\n";
                robotFishPort.Open();
                RobotConnectButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FeedbackConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                feedbackPort = new SerialPort((string)FeedbackPortList.SelectedItem, 115200, Parity.None, 8, StopBits.One);
                feedbackPort.NewLine = "\n";
                feedbackPort.Open();
                FeedbackConnectButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
