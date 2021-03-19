﻿// Copyright (c) 2021 github.com/cocoon
// 
// The copyright notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace jxlNET.Decoder
{
    public class DecoderOptions : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


        public static string BaseDir = AppDomain.CurrentDomain.BaseDirectory;

        private  string outDir = System.IO.Path.Combine(BaseDir, "Out");
        public string OutDir 
        { 
            get { return outDir; }
            set { outDir = value; NotifyPropertyChanged(); }
        }

        private string decoderPath = System.IO.Path.Combine(BaseDir, "djxl.exe");
        public string DecoderPath
        {
            get { return decoderPath; }
            set { decoderPath = value; NotifyPropertyChanged(); }
        }

        private string outFilePrefix = DateTime.Now.ToLocalTime().ToString("yyyy-MM-ddTHH-mm-ss-fffffffZ") + "_";
        public string OutFilePrefix
        {
            get { return outFilePrefix; }
            set { outFilePrefix = value; NotifyPropertyChanged(); }
        }

        private string workingDirectory = BaseDir;
        public string WorkingDirectory
        {
            get { return workingDirectory; }
            set { workingDirectory = value; NotifyPropertyChanged(); }
        }


        private Version decoderVersion;

        public Version DecoderVersion
        {
            get { return decoderVersion; }
            set { decoderVersion = value; NotifyPropertyChanged(); }
        }

        public Version TryGetDecoderVersionInfo()
        {
            Version result = null;

            if (File.Exists(DecoderPath))
            {

                var version = FileVersionInfo.GetVersionInfo(DecoderPath);
                Console.WriteLine("version from FileVersionInfo: " + version);

                if (!string.IsNullOrEmpty(version.FileVersion))
                {
                    System.Version.TryParse(version.FileVersion, out result);
                    DecoderVersion = result;
                }
                else
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            Arguments = new Deccoder.Parameters.Version().Param,
                            FileName = DecoderPath,
                            WorkingDirectory = WorkingDirectory,

                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        Process proc = new Process();
                        proc.StartInfo = startInfo;
                        proc.Start();

                        string processStandardOutput = proc.StandardOutput.ReadLine();
                        //string processStandardError = proc.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(processStandardOutput))
                        {
                            string searchString = "[v";
                            int index = processStandardOutput.IndexOf(searchString);
                            if (index != -1)
                            {
                                string v = processStandardOutput.Substring(index + searchString.Length, 5);
                                Console.WriteLine("version from output: " + v);
                                string[] vArray = v.Split('.');
                                if (vArray != null && vArray.Length == 3)
                                {
                                    System.Version.TryParse(v, out result);
                                    Console.WriteLine("Version parsed: " + result);
                                    DecoderVersion = result;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    { }
                }
            }


            return result;
        }





        public DecoderOptions()
        { }

        public DecoderOptions(string OutDir, string DecoderPath, string OutFilePrefix, string WorkingDirectory)
        {
            this.OutDir = OutDir;
            this.DecoderPath = DecoderPath;
            this.OutFilePrefix = OutFilePrefix;
            this.WorkingDirectory = WorkingDirectory;
        }

    }
}
