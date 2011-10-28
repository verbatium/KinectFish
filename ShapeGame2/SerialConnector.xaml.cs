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
using System.Windows.Threading;
using System.IO.Ports;

namespace ShapeGame2
{
    /// <summary>
    /// Interaction logic for SerialConnector.xaml
    /// </summary>
    public partial class SerialConnector : Window
    {
        static SerialPort robotFishPort, feedbackPort;
        bool robotReady = false;
        string bootLog = "";

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
                robotFishPort.NewLine = ((Char)(0x0D)).ToString();
                robotFishPort.WriteBufferSize = 2;
                robotFishPort.Open();
                
                RobotConnectButton.IsEnabled = false;
                RobotConnectButton.Content = "Waiting for robot";
                robotFishPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(robotFishPort_DataReceived);
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
                feedbackPort.NewLine = ((Char)(0x0D)).ToString();
                feedbackPort.Open();
                FeedbackConnectButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // TODO Add a timer that checks if any data is received. If boot position data is received, 
        // then robot is working already.
        private void robotFishPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (!robotReady)
            {
                string input = robotFishPort.ReadExisting().Replace("\n", "\r\n").Trim('\0');

                //0,0,0,0,0,3693    +30
                //0,0,0,0,0,3000     0 
                //0,0,0,0,0,2306    -30

                bootLog += input;
                robotReady = checkForCMD();
                if (robotReady)
                {
                    //RobotConnectButton.Content = "Robot ready!";
                    
                    robotFishPort.WriteLine("./pwm -b");
                    robotConnected();
                }
            }
        }
        bool checkForCMD()
        {
            return bootLog.Contains("[root@netus]/root#");
        }

        double maxAngle = 30;
        double minAngle = -30;
        byte motorCommand(double angle)
        {
            angle = Math.Max(angle, minAngle);
            angle = Math.Min(angle, maxAngle);
            return (byte)(3.7 * angle + 143.5);
        }

        byte previousCommand = 143;
        // if you access this method at 50Hz, then it moves the fish robot to desired position quickly
        // prevents too quick turns
        public void turnFish(double angle)
        {
            if (robotReady)
            {
                byte target = motorCommand(angle);

                if (target > previousCommand)
                {
                    previousCommand += 8;
                    robotFishPort.Write(new byte[] { (byte)previousCommand }, 0, 1);
                }
                else if (target < previousCommand)
                {
                    previousCommand -= 8;
                    robotFishPort.Write(new byte[] { (byte)previousCommand }, 0, 1);
                }
                
                
            }
        }
        private void robotConnected()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(changeRobotButton));
        }
        private void changeRobotButton()
        {
            RobotConnectButton.Content = "Robot ready!";
        }
    }
}
