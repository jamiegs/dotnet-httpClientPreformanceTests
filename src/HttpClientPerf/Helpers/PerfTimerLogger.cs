using System;
using System.Diagnostics;

namespace HttpClientPerf
{
    public class PerfTimerLogger : IDisposable
    {
        public PerfTimerLogger(string message)
        {
            this._message = message;
            this._timer = new Stopwatch();
            this._timer.Start();
        }

        string _message;
        Stopwatch _timer;

        public void Dispose()
        {
            this._timer.Stop();
            var ms = this._timer.ElapsedMilliseconds;

        // log the performance timing with the Logging library of your choice
        // Example:
            System.Diagnostics.Debug.WriteLine(string.Format("{0} - Elapsed Milliseconds: {1}", this._message, ms));
        }
    }
}