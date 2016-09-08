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
    public class Athlete
    {
        public int Id { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        private List<Segment> _starredSegments = null;

        public List<Segment> StarredSegments
        {
            get
            {
                if (_starredSegments == null)
                {
                    var _api = StravaAPI.GetInstance(Access.CurrentAccessToken);
                    var segmentsJson = _api.GetStarredSegments(this.Id);
                    List<Segment> segments = new List<Segment>();
                    foreach (dynamic s in segmentsJson)
                    {
                        segments.Add(new Segment(s));
                    }
                    return segments;
                }
                else
                    return _starredSegments;
            }
        }

        public static Athlete FetchLoggedInAthelete()
        {
            var _api = StravaAPI.GetInstance(Access.CurrentAccessToken);
            return new Athlete(_api.GetAthlete());
        }

        protected Athlete(dynamic jsonObject)
        {
            this.Id = (int)jsonObject.id;
            this.FirstName = (string)jsonObject.firstname;
            this.LastName = (string)jsonObject.lastname;
        }

    }
}