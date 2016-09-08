using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;

namespace Stravarage.Models
{
    public class StravaAPI
    {
        public static int requests_last_15_minutes { get; set; }
        public static int requests_today { get; set; }
        
        public static DateTime last_request { get; set; }

        public const int MAX_RESULTS_PER_PAGE = 200;

        private string access_token;

        protected StravaAPI(string token)
        {
            this.access_token = token;
        }

        public static StravaAPI GetInstance(string access_token)
        {
            if (DateTime.UtcNow.Date != last_request.Date)
            {
                requests_today = 0;
                requests_last_15_minutes = 0;
            }

            if (DateTime.UtcNow.Hour != last_request.Hour || checkQuarterHour(DateTime.UtcNow.Minute) != checkQuarterHour(last_request.Minute))
                requests_last_15_minutes = 0;

            return new StravaAPI(access_token);
        }

        private static int checkQuarterHour(int minute)
        {
            if (minute < 15)
                return 1;
            else if (minute < 30)
                return 2;
            else if (minute < 45)
                return 3;
            else
                return 4;
        }

        public dynamic GetAthlete()
        {
            return getJsonFromStrava("/athlete");
        }

        public dynamic GetStarredSegments(int athleteId)
        {
            return getJsonFromStrava(string.Format("/athletes/{0}/segments/starred", athleteId.ToString()));
        }

        public dynamic GetSegment(int id)
        {
            return getJsonFromStrava(string.Format("/segments/{0}", id));
        }

        public dynamic GetLeaderboard(int segmentId, int page, int per_page)
        {
            return getJsonFromStrava(string.Format("/segments/{0}/leaderboard", segmentId),
                                        string.Format("per_page={0}&page={1}", per_page, page));
        }

        private dynamic getJsonFromStrava(string controllerPath, string urlParams = "")
        {
            using (var wb = new WebClient())
            {
                string url = string.Format("https://www.strava.com/api/v3{1}?access_token={0}&{2}", this.access_token, controllerPath, urlParams);
                last_request = DateTime.UtcNow;
                requests_last_15_minutes++;
                requests_today++;
                var response = wb.DownloadData(url);
                string json = System.Text.Encoding.Default.GetString(response);
                var jsonObject = Json.Decode(json);
                return jsonObject;
            }
        }
    }
}