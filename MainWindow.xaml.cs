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
        
        Process pProcess = new Process();
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
            startInfo.Arguments = "https://www.youtube.com/playlist?list=PLrryvZ-gdCErP01mLIHNidblxMovnockz --get-filename";

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            //Set output of program to be written to process output stream
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            pProcess.StartInfo = startInfo;
            string status_str = "Queuing";
            Thread thread_youtube_dl = new Thread(()=>youtube_dl(status_str,0));
            thread_youtube_dl.Start();
            thread_youtube_dl.Join();
            startInfo.Arguments = "https://www.youtube.com/playlist?list=PLrryvZ-gdCErP01mLIHNidblxMovnockz";
            pProcess.StartInfo = startInfo;
            status_str = "Downloading";
            Thread thread_youtube_dl1 = new Thread(() => youtube_dl(status_str, 1));

            

        }

        private void youtube_dl(string status_str,int state)
        {
            pProcess.Start();
            var i = 1;
            pProcess.BeginOutputReadLine();
            pProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
               
                // Prepend line numbers to each line of the output.
                if (!String.IsNullOrEmpty(e.Data))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (state == 0)
                        {
                            this.lvFiles.Items.Add(new MyItem { SLNum = i, Status = status_str, Name = e.Data.ToString(), Size = "--", Parcent = "--" });
                            i++;
                        }
                        else
                        {
                            this.lvFiles.Items.Insert(i - 1, new MyItem {Status=status_str});
                            i++;
                        }
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
public class MyItem
{
    public int SLNum { get; set; }
    public string Status { get; set; }
    public string Name { get; set; }
    public string Size { get; set; }
    public string Parcent { get; set; }
}