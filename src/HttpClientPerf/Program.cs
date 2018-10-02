using System;
using System.Diagnostics;
using HttpClientPerf.Sender;
using System.Threading;
using Microsoft.Extensions.CommandLineUtils;

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
            command.OnExecute(() => ExecuteGetCommand(urlOption, iterationsOption, disableKeepAliveOption, requestTimeoutOption));
        }
        static int ExecuteGetCommand(CommandOption urlOption, CommandOption iterationsOption, CommandOption disableKeepAliveOption, CommandOption requestTimeoutOption)
        {
            var iterationsString = iterationsOption.Value();
            int iterations = 1;
            int.TryParse(iterationsString, out iterations);

            var timeoutString = requestTimeoutOption.Value();
            int timeout = 100;
            
            int.TryParse(timeoutString, out timeout);
        
            using (var sender = new HttpMessageSender(disableKeepAliveOption.HasValue(), timeout))
            {
                for (int i = 0; i < iterations; i++)
                {
                    using (var l = new PerfTimerLogger("get request"))
                    {
                        try {
                            var result = sender.Send(new System.Uri(urlOption.Value()), "{ 'test' }", null, "application/json", CancellationToken.None);
                            Debug.WriteLine(result.Result.ToString());
                        } 
                        catch (AggregateException ex)
                        {
                            var flatException = ex.Flatten();
                            Debug.WriteLine(flatException.Message);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }

                    }
                }
            }
            return 0;
        }

    }
}
