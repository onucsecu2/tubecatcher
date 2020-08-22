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
        string report;
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
            string url_str = url.Text;
            ProcessStartInfo startInfo = new ProcessStartInfo();
            //strCommand is path and file name of command to run
            startInfo.FileName = "C:\\Program Files\\TubeCatcher\\youtube-dl.exe";
            
            //strCommandParameters are parameters to pass to program
            startInfo.Arguments = url_str+" -i --get-filename";

            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            //Set output of program to be written to process output stream
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
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
            startInfo1.RedirectStandardError = true;
            startInfo1.WindowStyle = ProcessWindowStyle.Hidden;

            qProcess.StartInfo = startInfo1;
            qProcess.StartInfo.Arguments = url_str+" -i";
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
                    report += e.Data + "\n";    
                    this.Dispatcher.Invoke(() =>
                        {
                            myTextBlock.Text = "Collecting Data...";
                            myItem.Add(new MyItem { SLNum = i, Status = status_str, Name = e.Data.ToString().ToString(), Size = "--", Percent = "--" });
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
            int i = 0;
            int j=0;
            qProcess.BeginOutputReadLine();
            qProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {

                // Prepend line numbers to each line of the output.
                if (!String.IsNullOrEmpty(e.Data))
                {
                    //json_str+= e.Data.ToString();

                    this.Dispatcher.Invoke(() =>
                    {
                        report += e.Data ;
                        myTextBlock.Text = e.Data;
                        string[] result = e.Data.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                        try
                        {
                            if (i != 0)
                            {
                                j = i - 1;
                            }
                            MyItem item = (MyItem)myItem.ElementAt(j);                        
                            item.Status = "Downloading";
                            //if (e.Data.Contains("[download] Downloading video"))
                            //{
                            //    item.Status = "Downloaded";
                              
                            //    i++;
                            //}

                            if (result[2].Contains("of"))
                            {
                                if (result[1].Contains("100.0%"))
                                {
                                    item.Status = "Downloaded";
                                    i++;
                                }
                                item.Percent = result[1];
                                item.Size = result[3];
                            }

                        }catch(Exception ec)
                        {
                            myTextBlock.Text = e.Data+" "+ec.Message;
                        }

                        report +=" \n";
                    });
                }
            });

            qProcess.WaitForExit();
            qProcess.CancelOutputRead();

            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Finished";
            });
            //write string to file
            File.WriteAllText(@"C:\path1.txt", report);
        }
    }
}
public class MyItem : INotifyPropertyChanged
{
    private string name;
    private string status;
    private string percent;
    private string size;
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
    public string Percent
    {
        get { return this.percent; }
        set
        {
            if (this.percent != value)
            {
                this.percent = value;
                this.NotifyPropertyChanged("Percent");
            }
        }

    }
    public string Size
    {
        get { return this.size; }
        set
        {
            if (this.size != value)
            {
                this.size = value;
                this.NotifyPropertyChanged("Size");
            }
        }

    }
    public event PropertyChangedEventHandler PropertyChanged;
    public void NotifyPropertyChanged(string propName)
    {
        if (PropertyChanged != null)
            this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }
}