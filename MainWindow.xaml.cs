using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private ObservableCollection<MyItem> myItem = new ObservableCollection<MyItem>();
        Process pProcess = new Process();
        Process qProcess = new Process();
        static Thread thread_youtube_dl_download, thread_youtube_dl_collect_data;
        public MainWindow()
        {
            InitializeComponent();
            lvFiles.ItemsSource = myItem;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            //strCommand is path and file name of command to run
            startInfo.FileName = "C:\\Program Files\\TubeCatcher\\youtube-dl.exe";
            
            //strCommandParameters are parameters to pass to program
            startInfo.Arguments = "https://www.youtube.com/playlist?list=PLrryvZ-gdCErP01mLIHNidblxMovnockz -i --get-filename";

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            //Set output of program to be written to process output stream
            startInfo.RedirectStandardOutput = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            pProcess.StartInfo = startInfo;
            string status_str = "Queuing";
            thread_youtube_dl_collect_data= new Thread(()=>this.youtube_dl_collect_data(status_str));
            thread_youtube_dl_collect_data.Name = "thread1";
            thread_youtube_dl_collect_data.Start();



            ProcessStartInfo startInfo1 = new ProcessStartInfo();
            //strCommand is path and file name of command to run
            startInfo1.FileName = "C:\\Program Files\\TubeCatcher\\youtube-dl.exe";

            startInfo1.UseShellExecute = false;
            startInfo1.CreateNoWindow = true;
            //Set output of program to be written to process output stream
            startInfo1.RedirectStandardOutput = true;
            startInfo1.WindowStyle = ProcessWindowStyle.Hidden;

            qProcess.StartInfo = startInfo1;
            qProcess.StartInfo.Arguments = "https://www.youtube.com/playlist?list=PLrryvZ-gdCErP01mLIHNidblxMovnockz -i";
            thread_youtube_dl_download=new Thread( youtube_dl_download);
            thread_youtube_dl_download.Name = "thread2";
            thread_youtube_dl_download.Start();

        }

        private void youtube_dl_collect_data(string status_str)
        {
            pProcess.Start();
            var i = 1;
            pProcess.BeginOutputReadLine();
            pProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
               
                // Prepend line numbers to each line of the output.
                if (!String.IsNullOrEmpty(e.Data))
                {
                    //json_str+= e.Data.ToString();

                        this.Dispatcher.Invoke(() =>
                        {
                            myTextBlock.Text = "Collecting Data...";
                            myItem.Add(new MyItem { SLNum = i, Status = status_str, Name = e.Data.ToString().ToString(), Size = "--", Parcent = "--" });
                            i++;
                        });
              

                }
            });
            
            pProcess.WaitForExit();
            pProcess.CancelOutputRead();
            
            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Downloading";
            });
            //JObject json = JObject.Parse(json_str);
            //System.IO.File.WriteAllText(@"C:\path.txt", json_str);

        }


        private void youtube_dl_download()
        {
            thread_youtube_dl_collect_data.Join();
            qProcess.Start();
            var i = 0;
            qProcess.BeginOutputReadLine();
            qProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {

                // Prepend line numbers to each line of the output.
                if (!String.IsNullOrEmpty(e.Data))
                {
                    //json_str+= e.Data.ToString();

                    this.Dispatcher.Invoke(() =>
                    {
                        myTextBlock.Text = "Downloading";
                        // MyItem item = (MyItem)lvFiles.Items.GetItemAt(2);0
                        //item.Status = "Downloading";
                        MyItem item=(MyItem)myItem.ElementAt(i);
                        item.Status = "Downloading";
                    });
                }
            });

            qProcess.WaitForExit();
            qProcess.CancelOutputRead();

            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Finished";
            });
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }



        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }
    }
}
public class MyItem : INotifyPropertyChanged
{
    private string name;
    private string status;
    public int SLNum { get; set; }

    public string Name
    {
        get { return this.name; }
        set
        {
            if (this.name != value)
            {
                this.name = value;
                this.NotifyPropertyChanged("Name");
            }
        }

    }
    public string Status
    {
        get { return this.status; }
        set
        {
            if (this.status != value)
            {
                this. status= value;
                this.NotifyPropertyChanged("Status");
            }
        }

    }
    public string Size { get; set; }
    public string Parcent { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    public void NotifyPropertyChanged(string propName)
    {
        if (PropertyChanged != null)
            this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }
}