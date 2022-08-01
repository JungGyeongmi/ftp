using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading;
/// <summary>
///  FTP server 비동기 접속 테스트
/// </summary>
namespace FtpUpload
{
    class AsynFtpSvr
    {
        public class FtpState
        {
            private ManualResetEvent wait;
            private FtpWebRequest request;
            private string fileName;
            private Exception operationException = null;
            string status;

            public FtpState()
            {
                wait = new ManualResetEvent(false);
            }
            public ManualResetEvent OperationComplete
            {
                get { return wait; }
            }
            public FtpWebRequest Request
            {
                get { return request; }
                set { request = value; }
            }
            public string FileName
            {
                get { return fileName; }
                set { fileName = value; }
            }
            public Exception OperationException
            {
                get { return operationException; }
                set { operationException = value; }
            }
            public string StatusDescription
            {
                get { return status; }
                set { status = value; }
            }
        }

        public bool Upload(string sTarget, string file)
        {
            bool succeed = false;
            // Create a Uri instance with the specified URI string. 
            // If the URI is not correctly formed, the Uri constructor 
            // will throw an exception.
            ManualResetEvent waitObject;

            Uri target = new Uri(sTarget);
            string fileName = file;
            FtpState state = new FtpState();

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(target);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            request.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["ftp_user"], ConfigurationManager.AppSettings["ftp_password"]);

            // Store the request in the object that we pass into the 
            // asynchronous operations.
            state.Request = request;
            state.FileName = fileName;

            // Get the event to wait on.
            waitObject = state.OperationComplete;

            // Asynchronously get the stream for the file contents.
            request.BeginGetRequestStream(
                new AsyncCallback(EndGetStreamCallback),
                state
            );

            // Block the current thread until all operations are complete.
            waitObject.WaitOne();

            // The operations either completed or threw an exception. 
            if (state.OperationException != null)
            {
                Console.WriteLine("ERROR", "FTP error", sTarget.Substring(0, sTarget.IndexOf("_")).Substring(sTarget.LastIndexOf("\\") + 1), sTarget);
                Console.WriteLine(state.OperationException.ToString() + (state.OperationException.InnerException != null ? state.OperationException.InnerException.ToString() : ""), sTarget);
                //throw state.OperationException;
            }
            else
            {
                succeed = true;
                Console.WriteLine("The operation completed - {0}", state.StatusDescription);
            }

            return succeed;
        }

        private static void EndGetStreamCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;

            Stream requestStream = null;
            // End the asynchronous call to get the request stream. 
            try
            {
                Console.WriteLine("Opened the stream");
                using (requestStream = state.Request.EndGetRequestStream(ar))
                {
                    // Copy the file contents to the request stream. 
                    const int bufferLength = 2048;
                    byte[] buffer = new byte[bufferLength];
                    int count = 0;
                    int readBytes = 0;
                    using (FileStream stream = File.OpenRead(state.FileName))
                    {
                        do
                        {
                            readBytes = stream.Read(buffer, 0, bufferLength);
                            requestStream.Write(buffer, 0, readBytes);
                            count += readBytes;
                        }
                        while (readBytes != 0);
                        Console.WriteLine("Writing {0} bytes to the stream.", count);
                        // IMPORTANT: Close the request stream before sending the request.
                        requestStream.Close();
                        stream.Close();
                    }
                }
                Console.WriteLine("Closed the stream");
                // Asynchronously get the response to the upload request.
                state.Request.BeginGetResponse(
                    new AsyncCallback(EndGetResponseCallback),
                    state
                );
            }
            // Return exceptions to the main application thread. 
            catch (Exception e)
            {
                Console.WriteLine("Could not get the request stream.");
                state.OperationException = e;
                state.OperationComplete.Set();

                if (requestStream != null)
                {
                    requestStream.Close();
                }
                return;
            }

        }

        // The EndGetResponseCallback method   
        // completes a call to BeginGetResponse. 
        private static void EndGetResponseCallback(IAsyncResult ar)
        {
            FtpState state = (FtpState)ar.AsyncState;
            FtpWebResponse response = null;

            try
            {
                response = (FtpWebResponse)state.Request.EndGetResponse(ar);
                response.Close();
                state.StatusDescription = response.StatusDescription;
                // Signal the main application thread that  
                // the operation is complete.
                state.OperationComplete.Set();
            }
            // Return exceptions to the main application thread. 
            catch (Exception e)
            {
                Console.WriteLine("Error getting response.");
                state.OperationException = e;
                state.OperationComplete.Set();

                if (response != null)
                {
                    response.Close();
                }
            }
        }
    }

}
