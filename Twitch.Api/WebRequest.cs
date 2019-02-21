using System;
using System.Collections.Generic;
using System.Net;

namespace Twitch.Api
{
    public static class ApiRequestHelper
    {
        public static string ExecuteWebRequest(string url, Dictionary<string, string> header = null)
        {
            WebRequest request = WebRequest.Create(url);

            if (request != null)
            {
                request.Method = "GET";
                request.Timeout = 12000;
                request.ContentType = "application/json";

                if (header != null)
                {
                    foreach (KeyValuePair<string, string> h in header)
                    {
                        request.Headers.Add(h.Key, h.Value);
                    }
                }

                using (System.IO.StreamReader sr = new System.IO.StreamReader(request.GetResponse().GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    return sr.ReadToEnd();
                }
            }

            return null;
        }
    }
}