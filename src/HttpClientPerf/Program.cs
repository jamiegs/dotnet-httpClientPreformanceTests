using System;
using System.Diagnostics;
using HttpClientPerf.Sender;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.Linq;

namespace HttpClientPerf
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "HttpClient Performance Test";
            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
                {
                    Console.WriteLine("HttpClient Performance Test");
                    return 0;
                });
            app.Command("get", (command) => ConfigureGetCommand(command));
            app.Execute(args);
        }
        static void ConfigureGetCommand(CommandLineApplication command)
        {
            command.HelpOption("-?|-h|--help");
            var urlOption = command.Option("-u|--url", "URL to send request to.", CommandOptionType.SingleValue);
            var iterationsOption = command.Option("-i|--iterations", "Number of iterations.", CommandOptionType.SingleValue);
            var disableKeepAliveOption = command.Option("-dk|--disable-keepalive", "Disables keepalive", CommandOptionType.NoValue);
            var requestTimeoutOption = command.Option("-t|--request-timeout", "Sets a timeout in milliseconds on each request.", CommandOptionType.SingleValue);
            var alwaysCreateClientOption = command.Option("-acc|--always-create", "Always creates an http client.", CommandOptionType.NoValue);
            command.OnExecute(() => ExecuteGetCommand(urlOption, iterationsOption, disableKeepAliveOption, requestTimeoutOption, alwaysCreateClientOption));
        }
        static int ExecuteGetCommand(CommandOption urlOption, CommandOption iterationsOption, CommandOption disableKeepAliveOption, CommandOption requestTimeoutOption, CommandOption alwaysCreateClientOption)
        {
            var iterationsString = iterationsOption.Value();
            int iterations = 1;
            int.TryParse(iterationsString, out iterations);

            var timeoutString = requestTimeoutOption.Value();
            int timeout = 100;
            
            int.TryParse(timeoutString, out timeout);
            bool alwaysCreateClient = alwaysCreateClientOption.HasValue();

            using (var sender = new HttpMessageSender(disableKeepAliveOption.HasValue(), timeout, alwaysCreateClient))
            {
                var results = new List<long>();
                var exceptions = 0;
                var non200s = 0;
                for (int i = 0; i < iterations; i++)
                {
                    using (var l = new PerfTimerLogger("get request"))
                    {
                        try {
                            var result = sender.Send(new System.Uri(urlOption.Value()), "{ 'test' }", null, "application/json", CancellationToken.None).Result;
                            if((int)result != 200)
                            {
                                Console.WriteLine(result.ToString());
                                non200s++;
                            }
                        } 
                        catch (AggregateException ex)
                        {
                            var flatException = ex.Flatten();
                            Console.WriteLine(flatException.Message);
                            exceptions++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            exceptions++;
                        }
                        results.Add(l.ElapssedMilliseconds);
                    }
                }
                Console.WriteLine(string.Empty);
                Console.WriteLine($"Max: {results.Max()}");
                Console.WriteLine($"Min: {results.Min()}");
                Console.WriteLine($"Average: {results.Average()}");
                Console.WriteLine($"Exceptions: {exceptions}");
                Console.WriteLine($"Non-200 responses: {non200s}");
            }
            return 0;
        }

    }
}
