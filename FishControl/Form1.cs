using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FishControl
{
    public partial class Form1 : Form
    {

        bool CMDIsReady = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            serialPort5.WriteLine(txtCmd.Text);
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!serialPort5.IsOpen)
                serialPort5.Open();

            if (serialPort5.IsOpen) lblPortStatus.Text = "OPEN";
            else lblPortStatus.Text = "Closed";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        delegate void SetTextCallback(string text);
        private void AppendText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtTerminal.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(AppendText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.txtTerminal.AppendText(text);
                txtTerminal.ScrollToCaret();
            }
        }

        bool checkForCMD()
        {
            return txtTerminal.Text.Contains("[root@netus]/root#");
        }

        private void serialPort5_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            string input = serialPort5.ReadExisting().Replace("\n", "\r\n").Trim('\0');
            
            //0,0,0,0,0,3693    +30
            //0,0,0,0,0,3000     0 
            //0,0,0,0,0,2306    -30

            AppendText(input);
            if (!CMDIsReady)
            {
                CMDIsReady = checkForCMD();
                if (CMDIsReady)
                {
                    lblCMDReady.Text = "OK";
                    serialPort5.WriteLine("./pwm -b");
                }
            }
               
        }

        private void disconectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serialPort5.Close();
            if (serialPort5.IsOpen) lblPortStatus.Text = "OPEN";
            else lblPortStatus.Text = "Closed";
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            label1.Text =Math.Round(((trackBar1.Value - 143.5) / 3.7 - 0.14),2).ToString();
            serialPort5.Write(new byte[] { (byte)trackBar1.Value }, 0, 1);
            // Convert.ToChar(trackBar1.Value);
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trackBar1.Value = 144;
            label1.Text = Math.Round(((trackBar1.Value - 143.5) / 3.7 - 0.14), 2).ToString();
            serialPort5.Write(new byte[] { (byte)trackBar1.Value }, 0, 1);
        }
    }
}
