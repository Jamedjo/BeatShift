using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net;
using System.IO;

namespace BBF_generator
{
    class AnalyseResponse
    {
        int statusCode;
        string statusMessage;
        float tempo;
        //string trackID;
        //string md5;
        float duration;
        string fullanalysis;
        JObject analysis;
        public AnalyseResponse(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jResponse = jObject["response"];
            JToken jTrack = jResponse["track"];
            JToken jStatus = jResponse["status"];
            JToken jSummary = jTrack["audio_summary"];
            statusCode = (int)jStatus["code"];
            statusMessage = (string)jStatus["message"];
            if (statusCode == 0)
            {
                fullanalysis = (string)jSummary["analysis_url"];
                duration = (float)jSummary["duration"];
                tempo = (float)jSummary["tempo"];
                HttpWebRequest HttpWReq = 
                    (HttpWebRequest)WebRequest.Create(fullanalysis);

                HttpWebResponse HttpWResp = (HttpWebResponse)HttpWReq.GetResponse();
                string jsonResponse = new StreamReader(HttpWResp.GetResponseStream()).ReadToEnd();
                analysis = JObject.Parse(jsonResponse);
                HttpWResp.Close();
            }
        }
        //string 


        public Boolean isSetup()
        {
            return (statusCode == 0);
        }

        public int GetDuration()
        {
            return (int)(duration*1000);
        }

        public List<int> GetBeats()
        {
            List<int> beats = new List<int>();

            JToken[] jBeats = analysis["tatums"].ToArray();
            for (int ii = 0; ii < jBeats.Length; ii++)
            {
                int temp = ((int)((float)jBeats[ii]["start"] * 1000));
                beats.Add(temp);
                Console.Out.WriteLine(temp);
            }

                return beats;
        }
    }
}
