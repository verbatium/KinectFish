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
        public SerialConnector()
        {
            InitializeComponent();
        }

        private void RobotPortList_DropDownOpened(object sender, EventArgs e)
        {
            // Could this be done with data binding in XAML instead?
            RobotPortList.Items.Clear();
            foreach (string portName in SerialPort.GetPortNames())
                RobotPortList.Items.Add(portName);
        }
    }
}
