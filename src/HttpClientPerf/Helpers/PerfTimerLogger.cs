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
        public long ElapssedMilliseconds => _timer.ElapsedMilliseconds;
        public void Dispose()
        {
            this._timer.Stop();
            var ms = this._timer.ElapsedMilliseconds;
            Console.WriteLine(string.Format("{0} - Elapsed Milliseconds: {1}", this._message, ms));
        }
    }
}