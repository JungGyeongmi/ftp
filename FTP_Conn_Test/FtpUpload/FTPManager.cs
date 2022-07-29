using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FtpUpload
{
    public class FTPManager
    {
        public delegate void ExceptionEventHandler(string LocationID, Exception ex);
        public event ExceptionEventHandler ExceptionEvent;
        public Exception LastException = null;
        public bool IsConnected { get; set; }
        private string ipAddr = string.Empty;
        private string port = string.Empty;
        private string userId = string.Empty;
        private string pwd = string.Empty;
        private string path = string.Empty;


        public bool ConnectToServer(string ip, string port, string userId, string pwd, string path)
        {
            this.IsConnected = false;
            this.ipAddr = ip;
            this.port = port;
            this.userId = userId;
            this.pwd = pwd;
            this.path = path;
            string url = string.Format(@"FTP://{0}:{1}/{2}", this.ipAddr, this.port, this.path);
            // /home/woorifsftp/LIVE/SEND/FNP_recv_info_bond.20220701 
            // fnpprd/batch_gmjung/src/test.pc
            Console.WriteLine("START");
            Console.WriteLine();
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(url);
                ftpRequest.Credentials = new NetworkCredential(userId, pwd);
                ftpRequest.KeepAlive = false;
                ftpRequest.UsePassive = false;
                /// 메서드
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                string data = String.Empty;
                Console.WriteLine($"List Directory Connect And Read..");

                FtpWebResponse response = ftpRequest.GetResponse() as FtpWebResponse;
                StreamReader directoryReader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.ASCII);

                if (response != null)
                {
                    StreamReader streamReader = new StreamReader(response.GetResponseStream(), Encoding.Default);
                    data = streamReader.ReadToEnd();
                }

                string[] directorys = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var test = (from directory in directorys
                            select directory.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                           into directoryinfos select directoryinfos).ToList();
                /*                        into directoryInfos
                //                        where directoryInfos[0][0] != 'd'
                                        select directoryInfos[10]).Any(name => name.Substring(name.Length - 8) == "20220728");
                */
                ///test[n][8]  file name
                //Console.WriteLine(test[0][5] +" "+test[0][6]);
                //Console.WriteLine(test[0][7]);
                //CultureInfo ci = new CultureInfo("ko-KR");

                //Console.WriteLine(DateTime.Parse(test[0][7]));
                //string test1 = DateTime.Parse(test[0][7]).ToString();
                //var test3 = TimeZoneInfo.ConvertTimeToUtc(DateTime.Parse(test[0][7]));
               // var test3 = DateTime.Parse(test[0][7]).ToLocalTime();
                // DateTime dt = DateTime.ParseExact(test1, "ddd MMM dd yyyy HH:mm:ss 'GMT'K", ci);
                 //Console.WriteLine(test3);
                  foreach(string[] t in test)
                {
                    if (t.Length == 9)
                    {
                        string filename = t[8];
                        if (filename.Substring(filename.Length - 8) == "20220728")
                        {
                            if(filename.Contains("FNP_recv_proc_etf") && filename.Contains("end"))
                            {
                                /// 32.21 의 경우에는 UCT 라 convert 필요함
                               /// Console.WriteLine(DateTime.Parse(t[7]).ToLocalTime());
                                Console.WriteLine(DateTime.Parse(t[7]));
                                Console.WriteLine(t[8]);
                                Console.WriteLine("------------               ------");

                            }
                        }
                    }
                }


                /*
                                List<string> data = null;
                                while (!directoryReader.EndOfStream)
                                {

                                    string readLine = directoryReader.ReadLine();
                                  *//*  var test = (from directory
                                                 in directorys
                                     select directory.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                                             into directoryInfos
                                     //                            where directoryInfos[0][0] != 'd'
                                     select directoryInfos[8]).ToArray();

                                    data.Add(test[0].ToString());*//*

                                    if (readLine.Substring(readLine.Length - 8) == "20220728")
                                    {
                                        Console.WriteLine(directoryReader.ReadLine());
                                    }

                                }*/
                /*                Console.WriteLine(data.Count);
                                foreach (string read in data)
                                {
                                    Console.WriteLine(read);
                                }*/

                ///.Any(name => name == "");

/*                foreach (string i in directorys)
                {
                    Console.WriteLine(i);
                }*/
                Console.WriteLine();
                Console.WriteLine("END");
                this.IsConnected = true;
            }
            catch (WebException e)
            {
               /* FtpWebResponse response = (FtpWebResponse)e.Response;

                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    Console.WriteLine("Does not exist");
                }
                else if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    Console.WriteLine("Status Code : {0}", ((HttpWebResponse)e.Response).StatusCode);
                    Console.WriteLine("Status Description : {0}", ((HttpWebResponse)e.Response).StatusDescription);
                }
                else
                {
                    Console.WriteLine("Error: " + e.Message);
                }*/

                return false;
            }

            return true;
        }

        public void regexCheck(FtpWebResponse response)
        {
            string pattern = @"^(?<dir>[-d])(?<permission>(?:[-r][-w][-xs]){3})\s+\d+\s+\w+(?:\s+\w+)?\s+(?<size>\d+)\s+(?<timestamp>\w+\s+\d+(?:\s+\d+(?::\d+)?))\s+(?!(?:\.|\.\.)\s*$)(?<name>.+?)\s*$";
            Regex re = new Regex(pattern, RegexOptions.Multiline);

            string source = @"
                -rwxr-xr-x 1 root  46789 Feb  7 23:15 certbot-auto
                drwxr-xr-x 2 root   4096 Mar 22 16:29 test dir
                drwxr-xr-x 4 root   4096 Feb 10 15:50 www
                -rw-rw-rw- 1 generic 235 Mar 22 11:21 fromDoder/DOD997ABCD.20170322112114159.1961812284.txt
                -rw-rw-rw- 1 cmuser cmuser 904 Mar 23 15:04 20170323110427785_3741647.edi
                drw-rw-rw- 1 user   group    0 Apr 23  2016 .
                drw-rw-rw- 1 user   group    0 Apr 23  2016 ..
                drw-rw-rw- 1 user   group    0 Apr 23  2016 .cache
                drw-rw-rw- 1 user   group    0 Apr 23  2016 .bashrc
                ";
            var lastModiDt = response.LastModified;
           
            response.GetResponseStream();
            MatchCollection matches = re.Matches(source);

            Console.WriteLine(matches.Count);

            foreach (Match match in matches)
            {
                Console.WriteLine(match.Groups["dir"]);
                Console.WriteLine(match.Groups["permission"]);
                Console.WriteLine(match.Groups["size"]);
                Console.WriteLine(match.Groups["timestamp"]);
                Console.WriteLine(match.Groups["name"]);
                Console.WriteLine();
            }
        }


        public bool UpLoad(string folder, string filename)

        {
            return upload(folder, filename);
        }

        private bool upload(string folder, string filename)
        {
            try
            {
                makeDir(folder);

                FileInfo fileInf = new FileInfo(filename);

                folder = folder.Replace('\\', '/');

                filename = filename.Replace('\\', '/');

                string url = string.Format(@"FTP://{0}:{1}/{2}{3}", this.ipAddr, this.port, folder, fileInf.Name);

                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(url);

                ftpRequest.Credentials = new NetworkCredential(userId, pwd);

                ftpRequest.KeepAlive = false;

                ftpRequest.UseBinary = false;

                ftpRequest.UsePassive = false;

                ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

                ftpRequest.ContentLength = fileInf.Length;

                int buffLength = 2048;

                byte[] buff = new byte[buffLength];

                int contentLen;

                using (FileStream fs = fileInf.OpenRead())
                {
                    using (Stream strm = ftpRequest.GetRequestStream())
                    {
                        contentLen = fs.Read(buff, 0, buffLength);

                        while (contentLen != 0)
                        {
                            strm.Write(buff, 0, contentLen);

                            contentLen = fs.Read(buff, 0, buffLength);
                        }
                    }
                    fs.Flush();
                    fs.Close();

                }

                if (buff != null)
                {
                    Array.Clear(buff, 0, buff.Length);
                    buff = null;
                }
            }

            catch (Exception ex)
            {

                this.LastException = ex;

                System.Reflection.MemberInfo info = System.Reflection.MethodInfo.GetCurrentMethod();

                string id = string.Format("{0}.{1}", info.ReflectedType.Name, info.Name);

                if (this.ExceptionEvent != null)
                    this.ExceptionEvent(id, ex);
                return false;

            }

            return true;
        }

        private void makeDir(string dirName)
        {
            string[] arrDir = dirName.Split('\\');

            string currentDir = string.Empty;


            try
            {
                foreach (string tmpFoler in arrDir)
                {
                    try

                    {
                        if (tmpFoler == string.Empty) continue;

                        currentDir += @"/" + tmpFoler;

                        string url = string.Format(@"FTP://{0}:{1}{2}", this.ipAddr, this.port, currentDir);

                        FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(url);

                        ftpRequest.Credentials = new NetworkCredential(userId, pwd);

                        ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

                        ftpRequest.KeepAlive = false;

                        ftpRequest.UsePassive = false;
                        FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                        response.Close();

                    }
                    catch { }
                }
            }

            catch (Exception ex)
            {
                this.LastException = ex;
                System.Reflection.MemberInfo info = System.Reflection.MethodInfo.GetCurrentMethod();

                string id = string.Format("{0}.{1}", info.ReflectedType.Name, info.Name);
                if (this.ExceptionEvent != null)
                    this.ExceptionEvent(id, ex);
            }
        }

        private void checkDir(string localFullPathFile)
        {
            FileInfo fInfo = new FileInfo(localFullPathFile);
            if (!fInfo.Exists)

            {
                DirectoryInfo dInfo = new DirectoryInfo(fInfo.DirectoryName);

                if (!dInfo.Exists)

                {
                    dInfo.Create();
                }
            }
        }
    }
}