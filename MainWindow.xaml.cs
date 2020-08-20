using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace TubeCatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Process pProcess = new System.Diagnostics.Process();
        public MainWindow()
        {
            InitializeComponent();

        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //strCommand is path and file name of command to run
            startInfo.FileName = "C:\\Program Files\\TubeCatcher\\youtube-dl.exe";

            //strCommandParameters are parameters to pass to program
            startInfo.Arguments = "https://www.youtube.com/watch?v=HrS2rgwakQY";

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            //Set output of program to be written to process output stream
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            pProcess.StartInfo = startInfo;
            Thread thread_youtube_dl = new Thread(youtube_dl);
            thread_youtube_dl.Start();
 
        }
        private void youtube_dl()
        {
            pProcess.Start();
            pProcess.BeginOutputReadLine();
            pProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                // Prepend line numbers to each line of the output.
                if (!String.IsNullOrEmpty(e.Data))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        myTextBlock.Text = e.Data.ToString();
                    });
                }
            });
            pProcess.WaitForExit();
        }

        private void lvFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
