using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dataFaker
{
    public static class Extensions
    {
        public static string GetStringValue(this HttpConnector.UserAgent userAgent)
        {
            if (userAgent == HttpConnector.UserAgent.Random)
            {
                userAgent = HttpConnector.GetRandomUserAgent();
            }
            switch (userAgent)
            {
                case HttpConnector.UserAgent.Mozilla:
                    return "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                case HttpConnector.UserAgent.Chrome:
                    return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";
                case HttpConnector.UserAgent.Egde:
                    return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063";
                default:
                    throw new NotImplementedException();
            }

        }
        public static string GetStringValue(this HttpConnector.HttpMethod method)
        {
            switch (method)
            {
                case HttpConnector.HttpMethod.Get:
                    return "GET";
                case HttpConnector.HttpMethod.Post:
                    return "POST";
                default:
                    throw new NotImplementedException();
            }

        }
    }
    public class HttpConnector
    {
        public enum UserAgent
        {
            Random,
            Mozilla,
            Chrome,
            Egde
        }
        public enum HttpMethod
        {
            Get,
            Post
        }
        public async Task<Tuple<string,string>> GetResponseAsync(
            string url, 
            string content, 
            CookieContainer cookies=null, 
            string referer="", 
            HttpMethod method = HttpMethod.Get, 
            UserAgent userAgent = UserAgent.Random)
        {
			
            if (!string.IsNullOrWhiteSpace(content) && method == HttpMethod.Get)
            {
                url += "?" + content;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.GetStringValue();
            //request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate, br";
            request.Headers[HttpRequestHeader.AcceptLanguage] = "en-Us,en,1=0.5";
            //request.Headers[HttpRequestHeader.UserAgent] = userAgent.GetStringValue();
            //if (!string.IsNullOrWhiteSpace(referer))
            //    request.Headers[HttpRequestHeader.Referer] = referer;
            //request.Headers[HttpRequestHeader.Connection] = "close";
            if (cookies!= null)
                request.CookieContainer = cookies;
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            if (!string.IsNullOrWhiteSpace(content) && method == HttpMethod.Post)
            {
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] byteArray = Encoding.ASCII.GetBytes(content);
                //request.Headers["Content-Length"] = byteArray.Length.ToString();
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    requestStream.Write(byteArray, 0, byteArray.Length);
                    requestStream.Flush();
                }
            }
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            string newCookies = response.Headers[HttpResponseHeader.SetCookie];
            return new Tuple<string, string>(GetStringWithProperEncoding(response), newCookies);
        }
        public static UserAgent GetRandomUserAgent()
        {
            Random random = new Random();
            Array possibleAgents = Enum.GetValues(typeof(HttpConnector.UserAgent));
            UserAgent userAgent;
            do
            {
                userAgent = (UserAgent)possibleAgents.GetValue(random.Next(0, possibleAgents.Length - 1));
            } while (userAgent == UserAgent.Random);
            return userAgent;
        }

        private static string GetStringWithProperEncoding(HttpWebResponse response)
        {
            byte[] bytes = default(byte[]);
            string result;
            //using (StreamReader sr = new StreamReader(response.GetResponseStream(),true))
            //{
            //    result = sr.ReadToEnd();
            //}

            using (var memstream = new MemoryStream())
            {
                response.GetResponseStream().CopyTo(memstream);
                bytes = memstream.ToArray();
            }

            if (response.ContentType.Contains("UTF-32"))
            {
                result = Encoding.UTF32.GetString(bytes);
            }
            else if (response.ContentType.Contains("UTF-8"))
            {
                result = Encoding.UTF8.GetString(bytes);
            }
            else if (response.ContentType.Contains("UTF-7"))
            {
                result = Encoding.UTF7.GetString(bytes);
            }
            else if (response.ContentType.Contains("ASCII"))
            {
                result = Encoding.ASCII.GetString(bytes);
            }
            else
            {
                result = Encoding.Unicode.GetString(bytes);
            }
            //result = Convert.(bytes);
			
            return DoubleCheckEncoding(result);
        }
        private static string DoubleCheckEncoding(string text)
        {
            MatchCollection matchCollection = Regex.Matches(text, "\\\\u[0-9a-f]{4}", RegexOptions.IgnoreCase);
            foreach (Match match in matchCollection)
            {
                char c = (char)Int16.Parse(match.Value.Substring(2), NumberStyles.AllowHexSpecifier);
                text = text.Replace(match.Value, c.ToString());
            }
            return text;
        }
    }
}
