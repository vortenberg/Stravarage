using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using System.Web.Helpers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Configuration;

namespace Stravarage.Models
{
    public class Segment
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Segment(dynamic jsonObject)
        {
            this.Id = (int)jsonObject.id;
            this.Name = (string)jsonObject.name;

        }

        private static MemoryCache _leaderboardCache = MemoryCache.Default;

        private List<SegmentEffort> _leaderboard;
        public List<SegmentEffort> Leaderboard
        {
            get
            {
                if (this._leaderboard == null)
                {
                    if (_leaderboardCache.Contains(this.Name))
                        return _leaderboardCache.Get(this.Name) as List<SegmentEffort>;
                    else
                    {
                        _leaderboard = new List<SegmentEffort>();
                        Dictionary<int, bool> _dedup = new Dictionary<int, bool>();
                        int page = 1;
                        var token = Access.CurrentAccessToken;
                        var api = StravaAPI.GetInstance(token);
                        var effort = api.GetLeaderboard(this.Id, page, StravaAPI.MAX_RESULTS_PER_PAGE);
                        int entryCount = effort.entry_count;
                        int requests =  entryCount / 200 + 1;

                        
                        Task[] tasks = new Task[requests];

                        Action<int> startTask = (ix) =>
                        {
                            tasks[ix] = Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    var _api = StravaAPI.GetInstance(token);
                                    var effortJson = api.GetLeaderboard(this.Id, ix + 1, StravaAPI.MAX_RESULTS_PER_PAGE);
                                    foreach (dynamic s in effortJson.entries)
                                    {
                                        double moving_time = (double)s.moving_time;
                                        double elapsed_time = (double)s.elapsed_time;
                                        double rest_time = elapsed_time - moving_time;

                                        if (rest_time < 0.2 * elapsed_time)//don't count efforts with too much rest time in the middle
                                        {
                                            SegmentEffort eff = new SegmentEffort(s);
                                            _leaderboard.Add(eff);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ; //throttle limit exceeded, use whatever we got so far
                                }
                            });
                        };

                        for (int i = 0; i < requests; i++)
                        {
                            startTask(i);
                        }

                        Task.WaitAll(tasks);

                        _leaderboard = _leaderboard.Where(s => s != null).ToList();
                        _leaderboard = _leaderboard.GroupBy(x => x.AthleteName).Select(y => y.First()).ToList();


                        removeOutliers();

        
                        CacheItemPolicy policy = new CacheItemPolicy();
                        policy.AbsoluteExpiration = DateTime.Now.AddDays(1);

                        _leaderboardCache.Add(this.Name, _leaderboard, policy);

                        return _leaderboard;
                    }
                }
                else
                    return _leaderboard;
            }
        }

        private void removeOutliers()
        {
            try
            {
                _leaderboard.Sort(new Comparison<SegmentEffort>((s1, s2) => s1.TimeInSeconds - s2.TimeInSeconds));
                double median, thirdQ, firstQ, iqr;
                int len = _leaderboard.Count;
                if (len % 2 == 0)
                {
                    median = (_leaderboard[len / 2].TimeInSeconds + _leaderboard[len / 2 - 1].TimeInSeconds) / 2.0;
                    thirdQ = (_leaderboard[3 * (len / 4)].TimeInSeconds + _leaderboard[3 * (len / 4 - 1)].TimeInSeconds) / 2.0;
                    firstQ = (_leaderboard[(len / 4)].TimeInSeconds + _leaderboard[(len / 4 - 1)].TimeInSeconds) / 2.0;
                }
                else
                {
                    median = _leaderboard[(len / 2)].TimeInSeconds;
                    thirdQ = _leaderboard[3 * (len / 4)].TimeInSeconds;
                    firstQ = _leaderboard[(len / 4)].TimeInSeconds;
                }

                iqr = thirdQ - firstQ;
                double low = firstQ - iqr * 1.5;
                double high = thirdQ + iqr * 1.5;
                List<SegmentEffort> toRemove = new List<SegmentEffort>();
                foreach (SegmentEffort s in _leaderboard)
                {
                    if (s.TimeInSeconds < low || s.TimeInSeconds > high)
                        toRemove.Add(s);
                }

                foreach (SegmentEffort s in toRemove)
                    _leaderboard.Remove(s);
            }
            catch (IndexOutOfRangeException ex) {; }
        }
    }



    public class SegmentEffort
    {
        public int AthleteId { get; set; }
        public DateTime Date { get; set; }
        public int TimeInSeconds { get; set; }

        public string AthleteName { get; set; }

        public SegmentEffort(dynamic jsonObject)
        {
            this.AthleteName = (string)jsonObject.athlete_name;
            this.Date = Convert.ToDateTime(jsonObject.start_date);
            this.TimeInSeconds = (int)jsonObject.elapsed_time;
        }
    }
}