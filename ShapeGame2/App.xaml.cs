using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace ShapeGame2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void SerialWindow_Startup(object sender, StartupEventArgs e)
        {
            SerialConnector mainWindow = new SerialConnector();
            mainWindow.Top = 20;
            mainWindow.Left = 400;
            mainWindow.Show();
        }
    }
}
