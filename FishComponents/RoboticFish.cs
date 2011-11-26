using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace FishComponents
{
    public class RoboticFish
    {
        public SerialPort robotFishPort;

        string bootLog = "";

        public RoboticFish(String SerialPort)
        {
            try
            {
                robotFishPort = new SerialPort(SerialPort, 115200, Parity.None, 8, StopBits.One);
                robotFishPort.NewLine = ((Char)(0x0D)).ToString();
                robotFishPort.WriteBufferSize = 2;
                robotFishPort.Open();
                robotFishPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(robotFishPort_DataReceived);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }
        public void Open()
        { 
        }
        public void Close()
        { 
        }
        bool isready = false;
        public bool IsReady { get { return isready; } }

        private void robotFishPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (!isready)
            {
                string input = robotFishPort.ReadExisting().Replace("\n", "\r\n").Trim('\0');

                //0,0,0,0,0,3693    +30
                //0,0,0,0,0,3000     0 
                //0,0,0,0,0,2306    -30

                string[] inputArray = input.Split('\n');

                for (int i = 0; i < inputArray.Length; i++)
                {
                    //looking for command prompt
                    isready = checkForCMD(bootLog + inputArray[i]);
                    if (isready)
                    {
                        //start programm
                        robotFishPort.WriteLine("./pwm -b");
                        OnConnected(EventArgs.Empty);
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
                                isready = true;
                                OnConnected(EventArgs.Empty);
                                break;
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
        public event EventHandler Connected;
        // Invoke the Connected event;
        protected virtual void OnConnected(EventArgs e)
        {
            if (Connected != null)
                Connected(this, e);
        }
        bool checkForCMD(string val)
        {
            return val.Contains("[root@netus]/root#");
        }
        double maxAngle = 15;
        double minAngle = -15;
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
            if (IsReady)
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
            set { targetAngle = 0.8 * targetAngle + 0.2 * value; } //reduce jitter
            get { return realAngle; }
        }

    }
}
