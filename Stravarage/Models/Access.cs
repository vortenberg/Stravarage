using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;

namespace Stravarage.Models
{
    public class Access
    {
        public const string STRAVA_APP_CLIENT_SECRET = "b32a298d4ac6f89e969834036efa4bbeba5da1f2";
        public const string STRAVA_APP_CLIENT_ID = "3562";
        public const string STRAVA_APP_PUBLIC_ACCESS_TOKEN = "2c3ea7a404f8f42d56d3ed3394a4c0bd38a09def";

        public static string CurrentAccessToken
        {
            get
            {
                string token = string.Empty;
                if (HttpContext.Current.Request.Cookies["access_token"] != null)
                {
                    token = HttpContext.Current.Request.Cookies["access_token"].Value;
                }

                return token;
            }
            set
            {
                HttpCookie accessTokenCookie = new HttpCookie("access_token", value);
                accessTokenCookie.Expires = DateTime.Now.AddHours(12);
                HttpContext.Current.Response.Cookies.Add(accessTokenCookie);
            }
        }

        public static bool AuthenticateStravaAthlete()
        {
            bool success = false;
            if (Access.CurrentAccessToken != string.Empty)
                return true;
            else
            {
                if (HttpContext.Current.Request["code"] != null)
                {
                    try
                    {
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        using (var wb = new WebClient())
                        {
                            var data = new NameValueCollection();
                            data["client_id"] = Access.STRAVA_APP_CLIENT_ID;
                            data["client_secret"] = Access.STRAVA_APP_CLIENT_SECRET;
                            data["code"] = (HttpContext.Current.Request["code"]);

                            var response = wb.UploadValues("https://www.strava.com/oauth/token", "POST", data);
                            string json = System.Text.Encoding.Default.GetString(response);
                            dynamic jsonObject = Json.Decode(json);
                            if (jsonObject.access_token != null && jsonObject.access_token != string.Empty)
                            {
                                Access.CurrentAccessToken = jsonObject.access_token;
                                success = true;
                            }

                        }
                    }
                    catch (Exception) { }

                }
            }
            return success;
        }
    }
}