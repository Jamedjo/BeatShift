using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using EchoNestLib.Properties;
using System.Net;
using System.IO;
using SeasideResearch.LibCurlNet;

namespace BBF_generator
{
    public static class SharedData
    {
        /// <summary>
        /// Returns the APIKey that is stored into settings
        /// </summary>
        public static string APIKey
        {
            get
            {
                return "DDFQ4IGNU8RKVHRKW";
            }
        }

        public static Int32 OnWriteData(Byte[] buf, Int32 size, Int32 nmemb,
        Object extraData)
        {

            WebRequestResult = System.Text.Encoding.UTF8.GetString(buf);
            return size * nmemb;
        }

        private static string WebRequestResult;

        /// <summary>
        /// Returns the result and empties it
        /// </summary>
        /// <returns></returns>
        public static string ReadWebRequestResult()
        {
            string res = WebRequestResult;
            WebRequestResult = string.Empty;
            return res;

        }

        #region POST
        public static void PerformPostMultipartRequest(string query, string postData)
        {
            Curl.GlobalInit((int)CURLinitFlag.CURL_GLOBAL_ALL);

            Easy easy = new Easy();

            Easy.WriteFunction wf = new Easy.WriteFunction(OnWriteData);
            easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf);

            
            // simple post - with a string
            easy.SetOpt(CURLoption.CURLOPT_POSTFIELDS,
                postData);

            easy.SetOpt(CURLoption.CURLOPT_USERAGENT,
                "C# EchoNest Lib");
            easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, true);
            easy.SetOpt(CURLoption.CURLOPT_URL,
                query);
            easy.SetOpt(CURLoption.CURLOPT_POST, true);

           
            CURLcode res = easy.Perform();

            if (res != CURLcode.CURLE_OK)
                throw new WebException("Post operation failed");
    
           
            
            easy.Cleanup();
            
            


            Curl.GlobalCleanup();
            
            
           

        }
        #endregion

        #region GET
        public static void PerformGetRequest(string query)
        {

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(query);

            request.Method = "GET";
            request.UserAgent = "C# EchoNest Lib";
            //request.ContentLength = 8192;
            HttpWebResponse response = null;

            string responseBody;

            int statusCode;


            try
            {

                response = (HttpWebResponse)request.GetResponse();
                //byte[] buf = new byte[8192];
                Stream respStream = response.GetResponseStream();
                StreamReader respReader = new StreamReader(respStream);



                responseBody = respReader.ReadToEnd();
                statusCode = (int)(HttpStatusCode)response.StatusCode;

            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
                responseBody = "No Server Response";


            }

            WebRequestResult = responseBody;
            

        }
        #endregion
    }
}
