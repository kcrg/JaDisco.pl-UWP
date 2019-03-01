using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Shared
{
    public static class ApiRequestHelper
    {
        public static async Task<string> ExecuteWebRequestAsync(string url, Dictionary<string, string> header = null)
        {
            WebRequest request = WebRequest.Create(url);

            if (request != null)
            {
                request.Method = "GET";
                request.Timeout = 6000;
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
                    return await sr.ReadToEndAsync();
                }
            }

            return null;
        }
    }
}