using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using Stravarage.Models;

namespace Stravarage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Access.AuthenticateStravaAthlete())
                return RedirectToAction("Welcome");
            return View();
        }

        public ActionResult Drawtest()
        {
            return View();
        }

        public ActionResult Authenticate()
        {
            if (Access.AuthenticateStravaAthlete())
                return RedirectToAction("Welcome");
            else
                return RedirectToAction("Index");
        }

        public ActionResult Welcome()
        {
            if (!Access.AuthenticateStravaAthlete())
                return RedirectToAction("Index");

            return View(Athlete.FetchLoggedInAthelete());
        }

        public string Segment(int id)
        {
            Response.ContentType = "application/json charset utf-8";
            var _api = StravaAPI.GetInstance(Access.CurrentAccessToken);
            return System.Web.Helpers.Json.Encode(_api.GetSegment(id));
        }

        public JsonResult Leaderboard(int segmentId)
        {
            var _api = StravaAPI.GetInstance(Access.CurrentAccessToken);
            Segment s = new Segment(_api.GetSegment(segmentId));
            return Json(s.Leaderboard, JsonRequestBehavior.AllowGet);
        }

        public string RequestCount()
        {
            return System.Web.Helpers.Json.Encode(new { requests_last_15 = StravaAPI.requests_last_15_minutes, requests_today = StravaAPI.requests_today });
        }
    }
}