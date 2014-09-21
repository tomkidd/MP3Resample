using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace MP3Resample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool run = false;
        string originalDir = null;
        string newDir = null;

        public StringBuilder OutputText;

        private void OutputTextToBox(string sb)
        {
            textBox4.Text = sb;
        }
        public delegate void OutputTextCallback(string text);

        private string GetFirstEntry()
        {
            return listBox1.Items[0].ToString();
        }
        public delegate string GetFirstEntryCallback();

        private void RemoveFirstEntry()
        {
            listBox1.Items.RemoveAt(0);
        }
        public delegate void RemoveFirstEntryCallback();

        private void button1_Click(object sender, EventArgs e)
        {
            originalDir = textBox2.Text;
            newDir = textBox3.Text;

            run = true;
            Thread thread = new Thread(new ThreadStart(ResampleFiles));
            thread.Start();
        }

        private void DoStuff()
        {
            OutputText = new StringBuilder();
            while (run)
            {
                OutputText.Append(DateTime.Now.ToString() + Environment.NewLine);
                textBox4.Invoke(new OutputTextCallback(this.OutputTextToBox), OutputText.ToString());
                Application.DoEvents();
                Thread.Sleep(1000);
            }

            OutputText.Append("done" + Environment.NewLine);
            textBox4.Invoke(new OutputTextCallback(this.OutputTextToBox), OutputText.ToString());
        }

        private void ResampleFiles()
        {
            string logname = "mp3log-" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".txt";
            string logname2 = "mp3log2-" + DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".txt";

            while ((listBox1.Items.Count > 0) && (run))
            {
                string filename = listBox1.Invoke(new GetFirstEntryCallback(this.GetFirstEntry)).ToString();//.ToLower();
                string filename2 = filename.Replace(originalDir, newDir);
                string arguments = "--mp3input -b 192 --cbr \"" + filename + "\" \"" + filename2 + "\"";

                FileInfo fi2 = new FileInfo(filename2);
                if (!fi2.Directory.Exists) fi2.Directory.Create();

                FileInfo fi = new FileInfo(filename);
                if (File.Exists(fi.Directory.FullName + "\\folder.jpg"))
                {
                    if (!File.Exists(fi2.Directory.FullName + "\\folder.jpg"))
                    {
                        File.Copy(fi.Directory.FullName + "\\folder.jpg", fi2.Directory.FullName + "\\folder.jpg");
                    }
                }

                bool proceed = true;

                if (!fi.Exists)
                {
                    TextWriter tw = new StreamWriter(logname, true);
                    tw.WriteLine(fi.FullName + " does not exist");
                    tw.Close();
                }

                if (fi2.Exists)
                {
                    if (chkOverwrite.Checked)
                    {
                        fi2.Delete();
                    }
                    else
                    {
                        proceed = false;
                    }
                }

                if (proceed)
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.FileName = @"c:\lame\lame.exe";
                    p.StartInfo.Arguments = arguments;
                    p.Start();
                    string output = p.StandardError.ReadToEnd();
                    p.WaitForExit();

                    TextWriter tw = new StreamWriter(logname, true);
                    tw.Write(output + Environment.NewLine);
                    tw.Close();

                    FileInfo fileCompare1 = new FileInfo(filename);
                    FileInfo fileCompare2 = new FileInfo(filename2);

                    string NewFileIsSmaller = "false";
                    if ((File.Exists(filename)) && (File.Exists(filename2)))
                    {
                        if (fileCompare2.Length < fileCompare1.Length)
                        {
                            NewFileIsSmaller = "true";
                        }

                        string SizeLogLine = string.Format("{0};{1};{2};{3};{4}", filename, filename2, fileCompare1.Length, fileCompare2.Length, NewFileIsSmaller);

                        TextWriter tw2 = new StreamWriter(logname2, true);
                        tw2.WriteLine(SizeLogLine);
                        tw2.Close();
                    }

                    textBox4.Invoke(new OutputTextCallback(this.OutputTextToBox), output);
                }
                else
                {
                    TextWriter tw = new StreamWriter(logname, true);
                    tw.WriteLine(fi2.FullName + " exists, overwrite forbidden, skipping...");
                    tw.Close();
                }

                listBox1.Invoke(new RemoveFirstEntryCallback(this.RemoveFirstEntry));
                Application.DoEvents();
            }

            MessageBox.Show("Done");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringReader sr = new StringReader(textBox1.Text);
            
            string line = sr.ReadLine();

            while (line != null)
            {
                listBox1.Items.Add(line);
                line = sr.ReadLine();
            }

            textBox1.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            run = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            long originalSize = 0;
            long newSize = 0;

            originalDir = textBox2.Text;
            newDir = textBox3.Text;

            foreach (string filename in listBox1.Items)
            {
                string filename2 = filename.Replace(originalDir, newDir);

                FileInfo fi1 = new FileInfo(filename);
                FileInfo fi2 = new FileInfo(filename2);

                originalSize += fi1.Length;
                newSize += fi2.Length;
            }

            MessageBox.Show("Old Size: " + originalSize + "\nNew Size: " + newSize);
        }
    }
}
