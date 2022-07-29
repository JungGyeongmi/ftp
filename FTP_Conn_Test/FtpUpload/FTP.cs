using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;


namespace FtpUpload
{
    class FTP
    {
        private string _host;
        private NetworkCredential _credentials;
        private WebClient _wc;

        public FTP(string host, string username, string password)
        {
            _host = host;
            _credentials = new NetworkCredential(username, password);

            _wc = new WebClient()
            {
                BaseAddress = _host,
                Credentials = _credentials
            };
        }

        private WebRequest CreateRequest(string path, string method)
        {
            var req = WebRequest.Create(_host + path);
            req.Method = method;
            req.Credentials = _credentials;
            return req;
         }
        
        public bool DownloadFile(string remoteFile, string localFile)
        {
            try
            {
                _wc.DownloadFile(remoteFile, localFile);
                return true;
            } catch(Exception err)
            {
                Console.WriteLine(err.Message+"\n"+err.ToString());
                return false;
            }
        }
        
        public bool CreateDirectory(string remoteDir)
        {
            try
            {
                var req = CreateRequest(remoteDir, WebRequestMethods.Ftp.MakeDirectory);
                var resp = req.GetResponse();
                return true;
            } catch
            {
                return false;
            }
        }

        public List<string> GetFiles(string remoteDir)
        {
            try
            {
                var req = CreateRequest(remoteDir, WebRequestMethods.Ftp.ListDirectory);
                var resp = req.GetResponse();
                var sr = new StreamReader(resp.GetResponseStream());
                var l = new List<string>();

                string s = null;

                while(true)
                {
                    if(string.IsNullOrEmpty(s = sr.ReadLine()))
                    {
                        break;
                    }
                    l.Add(s);
                }
                sr.Close();
                return l;
            }
            catch
            {
                return null;   
            }
        }
    }
}
