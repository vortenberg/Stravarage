(function () {
    StatGraph = function (segment, segmentEfforts, draw_std_dev) {
        var efforts, minTime, maxTime, meanTime, modeTime, yourTime, standardDeviation, showStdDev;
        var percentile90Time, percentile80Time, percentile70Time, percentile60Time, percentile50Time, percentile40Time, percentile30Time, percentile20Time, percentile10Time;



        yourTime = segment.athlete_segment_stats.pr_elapsed_time;
        efforts = segmentEfforts;



        showStdDev = draw_std_dev;

        calculateVitalStats();

        var canvass = $("#canvasPR")[0];
        var width = canvass.width;
        var height = canvass.height;
        var context = canvass.getContext("2d");
        var yOfBaseline = 250;
        var numIncrements = 50.0;
        var widthOfTimeLabel = 40;
        var normalizationFactor = 1;
        var incrementSeconds = (maxTime - minTime) / numIncrements;
        var incrementPixels = (width - widthOfTimeLabel) / numIncrements;
        var startX = widthOfTimeLabel / 2;
        var stopX = width - startX;
        var tickYTop = height - yOfBaseline - 2;
        var tickYBottom = height - yOfBaseline + 2;
        var histogram = new Array(numIncrements);
        for (var j = 0; j < histogram.length; j++)
            histogram[j] = 0;


        renderGraph();



        this.minTime = function () { return secondsTimeSpanToHMS(minTime); }
        this.maxTime = function () { return secondsTimeSpanToHMS(maxTime); }
        this.meanTime = function () { return secondsTimeSpanToHMS(meanTime); }
        this.yourTime = function () { return secondsTimeSpanToHMS(yourTime); }
        this.standardDeviation = function () { return secondsTimeSpanToHMS(standardDeviation); }
        this.modeTime = function () { return secondsTimeSpanToHMS(modeTime); }
        this.numEfforts = function () { return efforts.length; }


        function calculateVitalStats() {
            var total = 0;
            var min = efforts[0].TimeInSeconds;
            var max = efforts[efforts.length - 1].TimeInSeconds;

            var arr = new Array(efforts[efforts.length - 1].TimeInSeconds);
            for (var k = 0; k < arr.length; k++)
                arr[k] = 0;

            for (var i = 0; i < efforts.length; i++) {
                total += efforts[i].TimeInSeconds;
                arr[efforts[i].TimeInSeconds]++;
            }

            modeTime = meanTime;
            arrMax = arr[0];
            for (var k = 0; k < arr.length; k++) {
                if (arr[k] > arrMax) {
                    modeTime = k;
                    arrMax = arr[k];
                }
            }

            minTime = min;
            maxTime = max;

            var mean = total / efforts.length
            meanTime = mean;

            var chi = 0
            for (var i = 0; i < efforts.length; i++)
                chi += Math.pow((efforts[i].TimeInSeconds - mean), 2);

            var stdDev = Math.sqrt(chi / efforts.length);

            standardDeviation = stdDev;

            var ix90 = Math.round(0.1 * efforts.length);
            var ix80 = Math.round(0.2 * efforts.length);
            var ix70 = Math.round(0.3 * efforts.length);
            var ix60 = Math.round(0.4 * efforts.length);
            var ix50 = Math.round(0.5 * efforts.length);
            var ix40 = Math.round(0.6 * efforts.length);
            var ix30 = Math.round(0.7 * efforts.length);
            var ix20 = Math.round(0.8 * efforts.length);
            var ix10 = Math.round(0.9 * efforts.length);

            percentile90Time = efforts[ix90].TimeInSeconds;
            percentile80Time = efforts[ix80].TimeInSeconds;
            percentile70Time = efforts[ix70].TimeInSeconds;
            percentile60Time = efforts[ix60].TimeInSeconds;
            percentile50Time = efforts[ix50].TimeInSeconds;
            percentile40Time = efforts[ix40].TimeInSeconds;
            percentile30Time = efforts[ix30].TimeInSeconds;
            percentile20Time = efforts[ix20].TimeInSeconds;
            percentile10Time = efforts[ix10].TimeInSeconds;

        }

        function renderGraph() {

            context.clearRect(0, 0, width, height);

            renderHistogram();

            if (showStdDev) {
                for (var t = meanTime - standardDeviation * 4; t <= meanTime + standardDeviation * 4; t += standardDeviation) {
                    if (Math.abs(t - meanTime) < 1)
                        continue;
                    renderTimeTickmark(t, height, "#000000", "", "#000000");
                }
            }
            else {
                renderTimeTickmark(percentile10Time, height, "#000000", "10%", "#000000");
                renderTimeTickmark(percentile20Time, height, "#000000", "20%", "#000000");
                renderTimeTickmark(percentile30Time, height, "#000000", "30%", "#000000");
                renderTimeTickmark(percentile40Time, height, "#000000", "40%", "#000000");
                renderTimeTickmark(percentile50Time, height, "#000000", "50%", "#000000");
                renderTimeTickmark(percentile60Time, height, "#000000", "60%", "#000000");
                renderTimeTickmark(percentile70Time, height, "#000000", "70%", "#000000");
                renderTimeTickmark(percentile80Time, height, "#000000", "80%", "#000000");
                renderTimeTickmark(percentile90Time, height, "#000000", "90%", "#000000");
            }

            renderTimeTickmark(yourTime, 80, "#0000ff", "You:");
            renderTimeTickmark(minTime, 140, "#0000ff", "KOM:");

            if (showStdDev)
                renderTimeTickmark(meanTime, height, "#ff0000", "Mean:");



            $(".time-tooltip").remove();
            var tooltip = $('<div class="time-tooltip"></div>');
            tooltip.appendTo($('body'));

            canvass.addEventListener('mousemove',
                                        function (evt)
                                        {
                                            var coords = getMousePosition(evt);
                                            var p = coords.x;
                                            var t = minTime + (maxTime - minTime) * (p - startX) / (stopX - startX);

                                            tooltip.html(secondsTimeSpanToHMS(t));
                                            tooltip.css({ position: 'absolute', top: coords.browserY - 20, left: coords.browserX });
                                            
                                        });

        }

        function getMousePosition(evt)
        {
            var rect = canvass.getBoundingClientRect();
            return {
                x: evt.clientX - rect.left,
                y: evt.clientY - rect.top,
                browserX: evt.clientX,
                browserY: evt.clientY
            }
        }

        function renderHistogram()
        {
            var ix = 0;
            var prevTime = minTime;

            for (var k = 0; k < efforts.length; k++) {
                if (efforts[k].TimeInSeconds >= prevTime + incrementSeconds) {
                    ix++;
                    prevTime += incrementSeconds;
                }
                histogram[ix]++;
            }

            var nMax = histogram[0];
            for (var k = 0; k < histogram.length; k++)
                if (histogram[k] > nMax)
                    nMax = histogram[k];


            context.beginPath();
            context.strokeStyle = "#a9a9a9";

            for (var k = 0; k < histogram.length; k++) {
                var h = histogram[k] * height / nMax;
                var x = startX + k * incrementPixels;
                var y = height - h;

                context.strokeRect(x, y, incrementPixels, h);
                context.fillStyle = "#cccccc";
                context.fillRect(x + 1, y + 1, incrementPixels - 1, h - 1);
                context.strokeStyle = "#666666";
                context.strokeText(histogram[k], x, y + 8);
            }

            console.log("Efforts: " + efforts.length);
            console.log("Mode: " + secondsTimeSpanToHMS(modeTime));
        }


        function renderTimeTickmark(time, tickHeight, color, label, labelColor) {
            if (labelColor == null)
                labelColor = color;
            context.beginPath();
            context.strokeStyle = color;
            t = time;
            p = (t - minTime) * (stopX - startX) / (maxTime - minTime) + startX;
            context.moveTo(p, height/2 - tickHeight / 2);
            context.lineTo(p, height/2 + tickHeight / 2);
            context.stroke();
            context.strokeStyle = labelColor;
            context.strokeText(secondsTimeSpanToHMS(time), p, (height - yOfBaseline + tickHeight / 2) - 1);
            context.strokeText(label, p, (height - yOfBaseline + tickHeight / 2) - tickHeight + 10);
            context.closePath();
        }

        function secondsTimeSpanToHMS(s) {
            var h = Math.floor(s / 3600); //Get whole hours
            s -= h * 3600;
            var m = Math.floor(s / 60); //Get remaining minutes
            s -= m * 60;
            return (h < 10 ? '0' + h : h) + ":" + (m < 10 ? '0' + m : m) + ":" + (s < 10 ? '0' + Math.round(s) : Math.round(s)); //zero padding on minutes and seconds
        }
    }
})();
