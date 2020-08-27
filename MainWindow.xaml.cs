
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TubeCatcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<String> files = new List<string>();
        string args1;
        string args2;
        string report;
        string path;
        int state;
        float total_progress;
        float current_progress;
        string youtube_dl = "youtube-dl.exe";
        private ObservableCollection<MyItem> myItem = new ObservableCollection<MyItem>();
        Process pProcess; 
        Process qProcess; 

      
        static Thread thread_youtube_dl_download, thread_youtube_dl_collect_data,thread_move_downloaded_files;
        public MainWindow()
        {
            InitializeComponent();
            lvFiles.ItemsSource = myItem;
            BtnStop.IsEnabled = false;
            BtnRetry.IsEnabled = false;
            path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            path += "\\Videos";
          
            this.Dispatcher.Invoke(() =>
            {
                output.Text = "Output : " + path;
            });
            //dummy_list_for_test();
        }
        private void dummy_list_for_test()
        {
            this.Dispatcher.Invoke(() =>
            {

                myItem.Add(new MyItem { SLNum = 0, Status = "status_str", Name = "dummy", Size = "--", Percent = "--" });
                myItem.Add(new MyItem { SLNum = 0, Status = "status_str", Name = "dummy", Size = "--", Percent = "--" });
                myItem.Add(new MyItem { SLNum = 0, Status = "status_str", Name = "dummy", Size = "--", Percent = "--" });
                myItem.Add(new MyItem { SLNum = 0, Status = "status_str", Name = "dummy", Size = "--", Percent = "--" });
                myItem.Add(new MyItem { SLNum = 0, Status = "status_str", Name = "dummy", Size = "--", Percent = "--" });
            });
        }
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            initialize();
            //Console.OutputEncoding = Encoding.Unicode;
            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Connecting...";
            });

            if (state == 0)
            {
                args1 = url.Text + " -i --get-filename --restrict-filenames";
                args2 = url.Text + " -i --restrict-filenames";
            }
            else
            {
                args1 = url.Text + " -i --playlist-start " + start_num.Text.ToString() + " --playlist-end " + end_num.Text.ToString() + " --get-filename --restrict-filenames";
                args2 = url.Text + " -i --restrict-filenames --playlist-start " + start_num.Text + " --playlist-end " + end_num.Text;
            }

            if (!checkUrl())
            {
                this.Dispatcher.Invoke(() =>
                {
                    BtnStop.IsEnabled = false;
                    BtnRetry.IsEnabled = false;
                    BtnStart.IsEnabled = true;
                    myTextBlock.Text = "url not found";
                });

            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                //strCommand is path and file name of command to run
                startInfo.FileName = youtube_dl;

                //strCommandParameters are parameters to pass to program
                startInfo.Arguments = args1;

                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                //Set output of program to be written to process output stream
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.RedirectStandardInput = true;

                pProcess.StartInfo = startInfo;
                string status_str = "Queuing";
                thread_youtube_dl_collect_data = new Thread(() => this.youtube_dl_collect_data(status_str));
                thread_youtube_dl_collect_data.Name = "thread1";
                thread_youtube_dl_collect_data.Start();



                ProcessStartInfo startInfo1 = new ProcessStartInfo();
                //strCommand is path and file name of command to run
                startInfo1.FileName = youtube_dl;
                startInfo1.UseShellExecute = false;
                startInfo1.CreateNoWindow = true;
                //Set output of program to be written to process output stream
                startInfo1.RedirectStandardOutput = true;
                startInfo1.RedirectStandardError = true;
                startInfo1.WindowStyle = ProcessWindowStyle.Hidden;
                qProcess.StartInfo = startInfo1;
                qProcess.StartInfo.Arguments = args2;
                thread_youtube_dl_download = new Thread(youtube_dl_download);
                thread_youtube_dl_download.Name = "thread2";
                thread_youtube_dl_download.Start();

                thread_move_downloaded_files = new Thread(move_files);
                thread_move_downloaded_files.Name = "thread3";
                thread_move_downloaded_files.Start();
            }
        }

        private bool checkUrl()
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.youtube.com/oembed?format=json&url="+url.Text);
            // Assign the response object of 'HttpWebRequest' to a 'HttpWebResponse' variable.

            if (myHttpWebRequest.Headers.ToString() == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        private void youtube_dl_collect_data(string status_str)
        {

            this.Dispatcher.Invoke(() =>
            {
                progressbar.IsIndeterminate=true;
            });
            pProcess.Start();

            int i = 1;
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
                            string[] tmp=e.Data.Split('.');
                            myItem.Add(new MyItem { SLNum = i, Status = status_str, Name = tmp[0], Size = "--", Percent = "--" });
                            files.Add(tmp[0]);
                            i++;
                    });
              

                }
            });
            
            pProcess.WaitForExit();
            pProcess.CancelOutputRead();
            
            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Downloading";
                total_progress = (float)myItem.Count;
            });
            //JObject json = JObject.Parse(json_str);
            //System.IO.File.WriteAllText(@"C:\path.txt", json_str);
        }
        private void youtube_dl_download()
        {
            thread_youtube_dl_collect_data.Join();
            this.Dispatcher.Invoke(() =>
            {
                progressbar.IsIndeterminate = false;
            });
            qProcess.Start();
            //qProcess.StandardInput.WriteLine("chcp 65001");
            int i = 0;

            qProcess.BeginOutputReadLine();
            qProcess.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
            // Prepend line numbers to each line of the output.
                if (!String.IsNullOrEmpty(e.Data))
                {
                    try 
                    {
                        report += e.Data;
                        this.Dispatcher.Invoke(() =>
                        {
                            myTextBlock.Text = e.Data;
                        });
                        string[] result = e.Data.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                        MyItem item = item = (MyItem)myItem.ElementAt(i);

                        this.Dispatcher.Invoke(() =>
                        {
                            item.Status = "Downloading";
                        });
                        if (result[2].Contains("of"))
                        {
                            float k = float.Parse(result[1].Remove(result[1].Length - 1, 1), CultureInfo.InvariantCulture);
                            current_progress = (float)(((float)(i) * 100.0) + k);
                            this.Dispatcher.Invoke(() =>
                            {
                                progressbar.Value = current_progress / total_progress;
                            });
                            if (result[1].Contains("100%"))
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    item.Status = "Downloaded";
                                });

                                i++;
                            }
                            item.Percent = result[1];
                            item.Size = result[3];
                        }
                        report += " \n";
                    }
                    catch(Exception exc)
                    {
                        report += exc.Message + "\n";
                    }

                 }
                    
            });

            qProcess.WaitForExit();
            qProcess.CancelOutputRead();
            if (myItem.Count == i)
            {
                this.Dispatcher.Invoke(() =>
                {
                    myTextBlock.Text = "Downloaded";
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    BtnRetry.IsEnabled = true;
                    myTextBlock.Text = "Retry!!!";
                });
               
                thread_move_downloaded_files.Abort();
            }
            //write string to file

        }
        private void url_TextChanged(object sender, TextChangedEventArgs e)
        {
            string str=url.Text;
            this.Dispatcher.Invoke(() =>
            {
                if (str.Contains("https://www.youtube.com/playlist?list"))
                {
                    myTextBlock.Text = "Playlist detected";
                    playlist.IsEnabled = true;
                    playlist.IsChecked = true;
                }
                else
                {
                    myTextBlock.Text = "Playlist not detected";
                    playlist.IsEnabled = false;
                    playlist.IsChecked = false;
                    reactivate();
                }
                    
            });
        }
        private void playlist_Checked(object sender, RoutedEventArgs e)
        {
            start_num.IsEnabled = false;
            start_num.Clear();
            end_num.IsEnabled = false;
            end_num.Clear();
            state = 0;
            reactivate();
        }
        private void initialize()
        {
            pProcess = new Process();
            qProcess = new Process();
            report = null;
            myItem.Clear();
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;
        }
        private void reactivate()
        {
            this.Dispatcher.Invoke(() =>
            {
                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
            });
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void playlist_Unchecked(object sender, RoutedEventArgs e)
        {
            start_num.IsEnabled = true;
            end_num.IsEnabled = true;
            state = 1;
            this.Dispatcher.Invoke(() =>
            {
                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = false;
            });

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                thread_move_downloaded_files.Abort();
            }
            catch (Exception exc)
            {
                report += exc.Message + "\n";
            }
            try
            {
                thread_youtube_dl_download.Abort();
            }
            catch (Exception ex)
            {
                report += ex.Message + "\n";
            }
            try
            {
                thread_youtube_dl_collect_data.Abort();
            }
            catch (Exception ex)
            {
                report += ex.Message + "\n";
            }


            try
            {
                qProcess.Kill();
            }
            catch(Exception ex)
            {
                report += ex.Message + "\n" ;
            }
            try
            {
                qProcess.CancelOutputRead();
            }
            catch (Exception ex)
            {
                report += ex.Message + "\n";
            }


            try
            {
                
                pProcess.CancelOutputRead();
            }
            catch (Exception ex)
            {
                report += ex.Message + "\n";
            }
            try
            {

                pProcess.Kill();
            }
            catch (Exception ex)
            {
                report += ex.Message + "\n";
            }




            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Process Terminated !";
            });
            this.Dispatcher.Invoke(() =>
            {
                BtnStart.IsEnabled = true;
                BtnStop.IsEnabled = false;
                progressbar.IsIndeterminate = false;
                progressbar.Value = 0;
            });
                myItem.Clear();
        }

        private void end_num_TextChanged(object sender, TextChangedEventArgs e)
        {
            int start=0, end=0;
            try
            {
                start = Int32.Parse(start_num.Text);
            }
            catch (Exception exc)
            {
                report += exc.Message + "\n";
            }
            try
            {
                end = Int32.Parse(end_num.Text);
            }catch(Exception exc)
            {
                report += exc.Message + "\n";
            }
            if (end > start)
            {
                reactivate();
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    BtnStart.IsEnabled = false;
                    BtnStop.IsEnabled = false;
                });
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(path);
        }

        private void BtnRetry_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                BtnRetry.IsEnabled = false;
                BtnStart.IsEnabled = false;
                BtnStop.IsEnabled = true;
            });

            ProcessStartInfo startInfo1 = new ProcessStartInfo();
            //strCommand is path and file name of command to run
            startInfo1.FileName = youtube_dl;
            startInfo1.UseShellExecute = false;
            startInfo1.CreateNoWindow = true;
            //Set output of program to be written to process output stream
            startInfo1.RedirectStandardOutput = true;
            startInfo1.RedirectStandardError = true;
            startInfo1.WindowStyle = ProcessWindowStyle.Hidden;
            qProcess.StartInfo = startInfo1;
            qProcess.StartInfo.Arguments = args2;
            thread_youtube_dl_download = new Thread(youtube_dl_download);
            thread_youtube_dl_download.Name = "thread2";
            thread_youtube_dl_download.Start();

            thread_move_downloaded_files = new Thread(move_files);
            thread_move_downloaded_files.Name = "thread3";
            thread_move_downloaded_files.Start();
        }

        private void move_files()
        {
            
            thread_youtube_dl_download.Join();
            report += "started moving.....\n";

            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Moving...";
                progressbar.IsIndeterminate = true;
            });
            string source_dir = Directory.GetCurrentDirectory();
            string directoryName = path;
            DirectoryInfo dirInfo = new DirectoryInfo(directoryName);
            if (dirInfo.Exists == false)
                Directory.CreateDirectory(directoryName);
            var filters = new String[] { ".mp4", ".webm", ".flv", ".mkv", ".wmv", ".avi", ".mpeg", ".mpg" };
            List<String> MyAllFiles = Directory
                               .GetFiles(source_dir, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp4") || s.EndsWith(".webm") || s.EndsWith(".mkv")).ToList();

            foreach (string file in MyAllFiles)
            {
                FileInfo mFile = new FileInfo(file);
                // to remove name collisions
                if (new FileInfo(dirInfo + "\\" + mFile.Name).Exists == false)
                {
                    mFile.MoveTo(dirInfo + "\\" + mFile.Name);
                }
                else
                {
                    File.Delete(mFile.Name);
                }

            }
            this.Dispatcher.Invoke(() =>
            {
                myTextBlock.Text = "Finished";
                progressbar.IsIndeterminate = false;
            });
            File.WriteAllText(@"report.log", report);
            reactivate();
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
