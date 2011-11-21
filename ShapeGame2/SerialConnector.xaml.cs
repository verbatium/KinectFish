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
                return false;
            }

            return true;
        }

        //static byte[] motorAddress = { 161, 162, 163, 164, 165, 166, 167, 168, 169, 170 }; // left to right
        static byte[] motorAddress = { 168, 167, 166, 165, 160, 160, 164, 163, 162, 161 }; // for 8 ventilators
        static byte[] prevMotorSpeeds = new byte [] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        static public bool SetFanSpeeds(byte[] motorSpeeds)
        {
            if (feedbackPort == null)
                return false;
            if (!feedbackPort.IsOpen)
                return false;

            try
            {
                for (int i = 0; i < motorSpeeds.Length; i++)
                {
                    // This optimisation does not work -- sometimes, data is lost, so resending is a good thing.
                    //if (prevMotorSpeeds[i] != motorSpeeds[i])
                    {
                        prevMotorSpeeds[i] = motorSpeeds[i];
                        feedbackPort.WriteLine(motorAddress[i].ToString());
                        feedbackPort.WriteLine(motorSpeeds[i].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
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
                //robotConnected();
                //robotReady = true;
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

                feedbackPort.RtsEnable = true;
                feedbackPort.PinChanged += new SerialPinChangedEventHandler(feedbackPort_PinChanged);

            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void feedbackPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            if (e.EventType == SerialPinChange.CtsChanged) 
                ResetButton(((SerialPort)sender).CtsHolding);
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

                string[] inputArray = input.Split('\n');

                for (int i = 0; i < inputArray.Length; i++)
                {
                    //looking for command prompt
                    robotReady = checkForCMD(bootLog + inputArray[i]);
                    if (robotReady)
                    {
                        //start programm
                        robotFishPort.WriteLine("./pwm -b");
                        robotConnected();
                    }

                    //analize data for output from fis
                    string[] inputData = inputArray[i].Split(',');
                    if (inputData.Length == 6)
                    {
                        int value = 0;
                        if (int.TryParse(inputData[5], out value))
                        {
                            if (value > 2300 && value < 3700)
                            {
                                //if data found when connection succesfull
                                robotReady = true;
                                robotConnected();
                            }
                        }
                    }
                    //if data in array is last line without \n then save it to futer use
                    if (i != inputArray.Length - 1 && !input.EndsWith("\n"))
                        bootLog += inputArray[i];
                    else
                        bootLog = "";

                }
                    

            }
        }
        bool checkForCMD(string val)
        {
            return val.Contains("[root@netus]/root#");
        }

        double maxAngle = 30;
        double minAngle = -30;
        byte motorCommand(double angle)
        {
            angle = Math.Max(angle, minAngle);
            angle = Math.Min(angle, maxAngle);
            return (byte)(3.7 * angle + 143.5);
        }

        byte previousCommand = 143, newCommand;
        // if you access this method at 50Hz, then it moves the fish robot to desired position quickly
        // prevents too quick turns
        double targetAngle = 0, realAngle = 0;
        // these are sensible limits
        double maxTurningAcceleration = 1700.0; // deg/s^2, too low value makes the fish overshoot
        double maxAngularVelocity = 250.0; // deg/s, needed to limit overshoot
        double previousSpeed = 0; // deg/s^2
        double dataRate = 50.0; // commands per second
        public void turnFish()
        {
            if (robotReady)
            {
                // calculate angle change
                double angleDifference = targetAngle - realAngle;
                //calculate angular velocity, assuming 50 Hz data rate
                double angularSpeed = angleDifference * dataRate;
                
                // limit top speed to prevent overshoot
                if (angularSpeed > maxAngularVelocity)
                    angularSpeed = maxAngularVelocity;
                else if (angularSpeed < -maxAngularVelocity)
                    angularSpeed = -maxAngularVelocity;

                double speedDifference = angularSpeed - previousSpeed;
                double acceleration = speedDifference * dataRate;

                // limit max torque by limiting maximum change of turning speed
                if (Math.Abs(acceleration) > maxTurningAcceleration)
                {
                    acceleration = maxTurningAcceleration * Math.Sign(acceleration);
                    // find new angle from reduced acceleration
                    angularSpeed = acceleration / dataRate + previousSpeed;
                    realAngle = angularSpeed / dataRate + realAngle;
                }
                // if speed limit reached, but not acceleration limit
                else if (Math.Abs(angularSpeed) == maxAngularVelocity)
                {
                    realAngle = angularSpeed / dataRate + realAngle;
                }
                // if no limits were exceeded
                else
                    realAngle = targetAngle;

                previousSpeed = angularSpeed;
                
                newCommand = motorCommand(realAngle);

                if (newCommand != previousCommand)
                {
                    robotFishPort.Write(new byte[] { (byte)(newCommand) }, 0, 1);
                }

                previousCommand = newCommand;
            }
        }
        public double RobotAngle
        {
            set { targetAngle = 0.8*targetAngle + 0.2*value; } //reduce jitter
            get { return realAngle; }
        }

        private void robotConnected()
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(changeRobotButton));
        }
        private void changeRobotButton()
        {
            RobotConnectButton.Content = "Robot ready!";
        }

        private void ResetButton(bool pressed)
        {
            if (pressed)
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ResetPressed));
            else
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(ResetUnPressed));
        }
        private void ResetPressed()
        {
            FeedbackConnectButton.Content = "Reset Pressed";
            //OnChanged(EventArgs.Empty);
        }
        private void ResetUnPressed()
        {
            FeedbackConnectButton.Content = "Reset Un Pressed";
            OnChanged(EventArgs.Empty);
        }

        public event EventHandler Changed;

        // Invoke the Changed event; called whenever list changes:
        protected virtual void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

    }
}
