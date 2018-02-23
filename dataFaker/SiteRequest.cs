using System;
using System.Collections.Generic;
using System.Text;

namespace dataFaker
{
    class SiteRequest
    {
        private string Url;
        private Dictionary<string, string[]> Parameters;
        public SiteRequest(string uri, params string[] parameters)
        {
            Tuple<string, string>[] defaultParameters = new Tuple<string, string>[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                defaultParameters[i] = new Tuple<string, string>(parameters[i], "" );
            }
            CreateSiteRequest(uri, defaultParameters);
        }
        public SiteRequest(string uri, params Tuple<string,string>[] parameters)
        {
            CreateSiteRequest(uri, parameters);
        }
        private void CreateSiteRequest(string uri, params Tuple<string, string>[] parameters)
        {
            Url = uri;
            Parameters = new Dictionary<string, string[]>();
            foreach (Tuple<string, string> parameter in parameters)
            {
                SetParameter(parameter.Item1, parameter.Item2);
            }
        }

        public string GetUri()
        {
            return Url;
        }
        public void SetParameter(string name, string value, bool append=true)
        {
            if (!Parameters.ContainsKey(name))
            {
                append = false;
                Parameters[name] = new string[1] { "" };
            }
            string[] newArray = new string[Parameters[name].Length + ((append == true && !string.IsNullOrWhiteSpace(Parameters[name][Parameters[name].Length-1])) ? 1 : 0)];
            int index = 0;
            while (append && index < Parameters[name].Length && !string.IsNullOrWhiteSpace(Parameters[name][index]))
            {
                newArray[index] = Parameters[name][index];
                index++;
            }
            newArray[index] = value;
            Parameters[name] = newArray;
        }
        public string[] GetParameter(string name)
        {
            return Parameters[name];
        }
        public string GetAllParameters()
        {
            string result = string.Empty;
            foreach (string key in Parameters.Keys)
            {
                foreach (string value in Parameters[key])
                {
                    result += Uri.EscapeUriString(key) + "=" + Uri.EscapeUriString(value) + "&";
                }
            }
            return result.Substring(0, result.Length - 1);
        }
    }
}
