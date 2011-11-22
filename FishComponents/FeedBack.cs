using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Windows.Threading;

namespace FishComponents
{
    public class FeedBack
    {

        public SerialPort feedbackPort;

        public FeedBack(String SerialPort)
        {

            try
            {
                feedbackPort = new SerialPort((string)SerialPort, 115200, Parity.None, 8, StopBits.One);
                feedbackPort.NewLine = ((Char)(0x0D)).ToString();
                feedbackPort.Open();

                feedbackPort.RtsEnable = true;
                feedbackPort.PinChanged += new SerialPinChangedEventHandler(feedbackPort_PinChanged);

            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public bool SetFanSpeeds(byte leftFan, byte rightFan)
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
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        //static byte[] motorAddress = { 161, 162, 163, 164, 165, 166, 167, 168, 169, 170 }; // left to right
        byte[] motorAddress = { 168, 167, 166, 165, 160, 160, 164, 163, 162, 161 }; // for 8 ventilators
        byte[] prevMotorSpeeds = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public bool SetFanSpeeds(byte[] motorSpeeds)
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
                Console.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        void feedbackPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            if (e.EventType == SerialPinChange.CtsChanged)
                ButtonStateChanged(((SerialPort)sender).CtsHolding);
        }

        private void ButtonStateChanged(bool pressed)
        {
            if (pressed)

                ResetPressed();// Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(ResetPressed));
            else
                ResetUnPressed(); // Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Normal, new Action(ResetUnPressed));
        }
        private void ResetPressed()
        {
            //FeedbackConnectButton.Content = "Reset Pressed";
            //OnChanged(EventArgs.Empty);
        }
        private void ResetUnPressed()
        {
            //FeedbackConnectButton.Content = "Reset Un Pressed";
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
