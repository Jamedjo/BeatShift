using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace BBF_generator
{
    class UploadResponse
    {
        int statusCode;
        string statusMessage;

        string trackID;
        string md5;

        public UploadResponse(string json)
        {
            JObject jObject = JObject.Parse(json);
            JToken jResponse = jObject["response"];
            JToken jTrack = jResponse["track"];
            JToken jStatus = jResponse["status"];
            statusMessage = (string)jStatus["message"];
            statusCode = (int)jStatus["code"];
            if (statusCode == 0)
            {
                trackID = (string)jTrack["id"];
                md5 = (string)jTrack["md5"];
            }
        }
        //string 


        public Boolean isSetup()
        {
            return (statusCode == 0);
        }

        public string GetMD5()
        {
            return md5;
        }
    }
}
