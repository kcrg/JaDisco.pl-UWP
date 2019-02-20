using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Twitch.Api
{
    public static class ApiRequestHelper
    { 
        static public string ExecuteWebRequest(string url, Dictionary<string, string> header = null)
        {
            var request = WebRequest.Create(url);

            if (request != null)
            {
                request.Method = "GET";
                request.Timeout = 12000;
                request.ContentType = "application/json";

                if (header != null)
                {
                    foreach (var h in header)
                    {
                        request.Headers.Add(h.Key, h.Value);
                    }
                }

                using (var sr = new System.IO.StreamReader(request.GetResponse().GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    return sr.ReadToEnd();
                }
            }


            return null;
        }
    }
}
