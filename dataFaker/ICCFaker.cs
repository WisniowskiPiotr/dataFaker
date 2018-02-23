using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace dataFaker
{
    class ICCFaker : IFakerInterface
    {
        public static string[] Options = new string[] {
            "Jasło",
            "Zagórz",
            "Sanok",
            "Biecz",
            "Strzyżów",
            "Gorlice",
            "Rzeszów",
            "Kraków",
            "Gdynia"
        };

        SiteRequest getStation = new SiteRequest("https://www.intercity.pl/station/get/", "q");
        SiteRequest alertStations = new SiteRequest("https://www.intercity.pl/pl/api/station/alert.html", "station[]", "station[]", "station[]", "station[]", "date", "_");
        SiteRequest searchConnection = new SiteRequest("https://www.intercity.pl/pl/site/dla-pasazera/informacje/wyszukiwarka-polaczen.html", "arr", "date", "hafasPage", "hafasSort", "search", "stname[0]", "stname[1]", "time", "viaid[0]", "viaid[1]", "vianame[0]", "vianame[1]");
        
        string StartStation;
        string EndStarion;
        string referer = "https://www.intercity.pl/pl/site/dla-pasazera/informacje/wyszukiwarka-polaczen.html?src=";

        public ICCFaker(string startStation, string endStation)
        {
            StartStation= startStation;
            EndStarion= endStation;
        }

        public async Task SendQueryAsync()
        {
            HttpConnector httpConnector = new HttpConnector();
            HttpConnector.UserAgent userAgent = HttpConnector.GetRandomUserAgent();
            DateTime date = GetRandomDateTime();
            Uri uri = new Uri("https://www.intercity.pl");

            Tuple<string,string> phpSessId = await httpConnector.GetResponseAsync(searchConnection.GetUri(), "", null, "", HttpConnector.HttpMethod.Get, userAgent);
            Console.Write("ICCFaker - Initialization done\n");

            Tuple<string, string> startStationIdresponse = await GetStationId(StartStation, phpSessId.Item2, uri, httpConnector, userAgent);
			string startStationId = JsonSelector.GetFirstValueOfToken("h", startStationIdresponse.Item1);
			string startStationName = JsonSelector.GetFirstValueOfToken("n", startStationIdresponse.Item1).Replace(' ','+');
            Console.Write("ICCFaker - Asked for station " + startStationName + "\n");
            Tuple<string, string> alertresult1 = await GetStationAlert(startStationId, date, startStationIdresponse.Item2, uri, httpConnector, userAgent);
            Console.Write("ICCFaker - Alerted station " + startStationName + "\n");

            Tuple <string, string> endStationIdresponse = await GetStationId(EndStarion, phpSessId.Item2, uri, httpConnector, userAgent);
            string endStationId = JsonSelector.GetFirstValueOfToken("h", endStationIdresponse.Item1);
            string endStationName = JsonSelector.GetFirstValueOfToken("n", endStationIdresponse.Item1).Replace(' ', '+');
            Console.Write("ICCFaker - Asked for station " + endStationName + "\n");
            Tuple <string, string> alertresult2 = await GetStationAlert(endStationId, date, endStationIdresponse.Item2, uri, httpConnector, userAgent);
            Console.Write("ICCFaker - Alerted station " + endStationName + "\n");

            Tuple <string, string> searchResult = await GetSearchResult(startStationId, endStationId, startStationName, endStationName, date, endStationIdresponse.Item2, uri, httpConnector, userAgent);
            Console.Write("ICCFaker - Ended Search" + "\n");
        }

        private DateTime GetRandomDateTime()
        {
            return DateTime.UtcNow.AddMinutes(new Random().Next(120, 129600));
        }

        private async Task<Tuple<string, string>> GetStationId(string startStation, string setCookies, Uri uri, HttpConnector httpConnector, HttpConnector.UserAgent userAgent)
        {
            CookieContainer cookies = new CookieContainer();
            if (!string.IsNullOrWhiteSpace(setCookies))
            {
                string phpSessionId = "PHPSESSID";
                cookies.Add(uri, new Cookie(phpSessionId, GetValueFromCookieString(phpSessionId, setCookies)));
            }
            cookies.Add(uri, new Cookie("eic_login", "0"));
            getStation.SetParameter("q", startStation, false);
            Tuple<string, string> response = await httpConnector.GetResponseAsync(
                getStation.GetUri(),
                getStation.GetAllParameters(),
                cookies,
                referer,
                HttpConnector.HttpMethod.Get,
                userAgent
                );
            return response;
        }

        private async Task<Tuple<string, string>> GetStationAlert(string startStationId, DateTime date, string setCookies, Uri uri, HttpConnector httpConnector, HttpConnector.UserAgent userAgent)
        {
            CookieContainer cookies = new CookieContainer();
            if (!string.IsNullOrWhiteSpace(setCookies))
            {
                string phpSessionId = "PHPSESSID";
                cookies.Add(uri, new Cookie(phpSessionId, GetValueFromCookieString(phpSessionId, setCookies)));
            }
            cookies.Add(uri, new Cookie("eic_login", "0"));
            alertStations.SetParameter("date", string.Format("{0:YYYY-MM-dd+HH:mm}", date), false);
            alertStations.SetParameter("station[]", startStationId);
            Tuple<string, string> startStation = await httpConnector.GetResponseAsync(
                alertStations.GetUri(),
                alertStations.GetAllParameters(),
                cookies,
                referer,
                HttpConnector.HttpMethod.Get,
                userAgent
                );
            return startStation;
        }

        private async Task<Tuple<string, string>> GetSearchResult(string startStationId, string endStationId, string startStationName, string endStationName, DateTime date, string setCookies, Uri uri, HttpConnector httpConnector, HttpConnector.UserAgent userAgent)
        {
            CookieContainer cookies = new CookieContainer();
            if (!string.IsNullOrWhiteSpace(setCookies))
            {
                string phpSessionId = "PHPSESSID";
                cookies.Add(uri, new Cookie(phpSessionId, GetValueFromCookieString(phpSessionId, setCookies)));
            }
            cookies.Add(uri, new Cookie("eic_login", "0"));
            searchConnection.SetParameter("arr", "0", false);
            searchConnection.SetParameter("date", string.Format("{0:YYYY-MM-dd}", date), false);
            searchConnection.SetParameter("search", "1", false);
            searchConnection.SetParameter("stid[0]", startStationId, false);
            searchConnection.SetParameter("stid[1]", endStationId, false);
            searchConnection.SetParameter("stname[0]", startStationName, false);
            searchConnection.SetParameter("stname[1]", endStationName, false);
            searchConnection.SetParameter("time", string.Format("{0:HH:mm}", date), false);
            Tuple<string, string> response = await httpConnector.GetResponseAsync(
                searchConnection.GetUri(),
                searchConnection.GetAllParameters(),
                cookies,
                referer,
                HttpConnector.HttpMethod.Post,
                userAgent
                );
            return response;
        }

        private string GetValueFromCookieString(string cookieName, string cookieString)
        {
            int index = cookieString.IndexOf(cookieName);
            if (index < 0)
                return string.Empty;
            index = index + cookieName.Length+1;
            if (index > cookieString.Length - 1)
                return string.Empty;
            if (cookieString[index] == '=')
                index++;
            int end = cookieString.IndexOf(';', index);
            return cookieString.Substring(index, end - index );
        }
    }
}
