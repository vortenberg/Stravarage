(function () {
    StravarageAPI = function () {
        this.getSegment = function (segmentId, callback) {
            $.getJSON("/home/Segment", { id: segmentId }, function (data) { callback(data) });
        }

        this.getLeaderboard = function (segmentId, callback) {
            $.getJSON("/home/Leaderboard", { segmentId: segmentId }, function (data) { callback(data) });
        }

        this.getRequestCount = function (callback) {
            $.getJSON("/home/RequestCount", function (data) { callback(data) });
        }
    }
})();