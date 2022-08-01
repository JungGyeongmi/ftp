using FtpUpload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

class Program
{
    private static Dictionary<string, Dictionary<string, string>> CoNameDirectory = new Dictionary<string, Dictionary<string, string>>();
    /// <summary>
    ///  우리펀드서비스, 미래에셋펀드서비스, 예탁결제원 ,하나펀드, KB국민, 삼성자산, 신한아이타스, HSBC펀드, 한국펀드
    ///  4129 4130 4131 4132 4133 4134 4135 4136 4137
    /// </summary>
    private static string[] directoryNames = { "woorifsftp", "miraefsftp", "ksdfsftp", "hanafsftp", "kbbankftp", "samsungam", "aitasftp", "hsbcfsftp", "kfsftp" };
    static int Main(string[] args)
    {
        //FTP 접속에 필요한 정보
        string addr = string.Empty;
        string user = string.Empty;
        string pwd = string.Empty;
        string port = string.Empty;
        string path = string.Empty;
        string pathTest = string.Empty;

        if (args.Length < 4)
        {
            Console.WriteLine("IP FTP_ID FTP_PW PORT");
            return 1;
        }

        addr = args[0]; //IP 주소
        user = args[1]; //FTP 접속 계정
        pwd = args[2]; //FTP 계정 비밀번호
        port = args[3];  //FTP 접속 Port
        path = args[4]; // 파일 경로

        pathTest = @"/home/{0}/LIVE/SEND";
        FTPManager manager = new FTPManager();

        foreach (string name in directoryNames)
        {
            var _values = manager.ConnectToServer(addr, port, user, pwd, string.Format(pathTest, name));
            CoNameDirectory.Add(name, _values);
        }


        foreach (var contest in CoNameDirectory)
        {
            Console.WriteLine(contest.Key);
            foreach (KeyValuePair<string, string> item in contest.Value)
            {
                if(item.Value != null)
                {
                    Console.WriteLine($"파일명 : {item.Key}  최종수정시간 : {item.Value}");
                }
            }
        }

        #region test 
        /*  string path = string.Empty;

          string fileName = string.Empty;

          string localPath = @"C:\Users\Desktop\"; //바탕화면 경로를 Local Path 기준으로 둠

          path = @"DATA"; //업로드 할 파일 저장할 FTP 경로 지정

          // DirectoryInfo dirInfo = new DirectoryInfo(localPath);

          // FileInfo[] infos = dirInfo.GetFiles();

          if (result == true)
          {
              Console.WriteLine("FTP 접속 성공");
              *//*    foreach (FileInfo info in dirInfo.GetFiles())
                  {
                      if (Path.GetExtension(info.Name) == ".txt") //txt 확장자 파일만 FTP 서버에 Upload
                      {
                          if (manager.UpLoad(path, info.FullName) == false) //파일 업로드
                          {
                              Console.WriteLine("FTP Upload 실패");
                          }
                          else
                          {
                              Console.WriteLine("FTP Upload 시작");
                              Console.WriteLine("FTP Upload 완료");
                          }
                      }
                  }*//*
          }
          else
          {
              Console.WriteLine("FTP 접속 실패");
          }
  */
        #endregion
        return 0;
    }

    static int FtpReader(string[] args)
    {
        try
        {
            if (args.Length < 5)
            {
                Console.WriteLine("FtpUpload.exe [ftpserver] [userid] [userpass] [serverpath] [filename]");
                return 1;
            }

            string serverAddress = args[0];
            string userId = args[1];
            string userPass = args[2];
            string dir = args[3];
            string filePath = args[4];
            string fileName = Path.GetFileName(filePath);

            string uploadPath = "ftp://" + serverAddress + dir + fileName;

            FtpWebRequest request = WebRequest.Create(uploadPath) as FtpWebRequest;

            Console.WriteLine("Passive: " + request.UsePassive);

            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(userId, userPass);

            byte[] fileContents = File.ReadAllBytes(filePath);
            Stream requestStream = request.GetRequestStream();
            request.ContentLength = fileContents.Length;
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = request.GetResponse() as FtpWebResponse;

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            Console.WriteLine(reader.ReadToEnd());

            Console.WriteLine("Complete, status {0}, length {1}", response.StatusDescription, response.ContentLength);

            response.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            return 1;
        }
        return 0;
    }

    static int FtpDownload()
    {
        // 코드 단순화를 위해 하드코드함
        string ftpPath = "ftp://ftp.daum.net";
        string user = "anonymous";  // FTP 익명 로그인시. 아니면 로그인/암호 지정.
        string pwd = "";
        // string outputFile = "index.txt";

        // WebRequest.Create로 Http,Ftp,File Request 객체를 모두 생성할 수 있다.
        FtpWebRequest req = (FtpWebRequest)WebRequest.Create(ftpPath);
        Console.WriteLine("Ftp Web Request");

        // FTP 다운로드한다는 것을 표시
        //req.Method = WebRequestMethods.Ftp.DownloadFile;
        // Directory details 
        req.Method = "LIST";
        //WebRequestMethods.Ftp.ListDirectoryDetails;

        // 익명 로그인이 아닌 경우 로그인/암호를 제공해야
        req.Credentials = new NetworkCredential(user, pwd);

        // FTP Request 결과를 가져온다.
        using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
        {
            Console.WriteLine("Response ... ");
            // FTP 결과 스트림
            Stream stream = resp.GetResponseStream();

            // 결과를 문자열로 읽기 (바이너리로 읽을 수도 있다)
            string data;
            using (StreamReader reader = new StreamReader(stream))
            {
                data = reader.ReadToEnd();
                Console.WriteLine(data);
            }

            // 로컬 파일로 출력
            //File.WriteAllText(outputFile, data);
            Console.WriteLine("Write All Text");
        }
        return 1;
    }
}
