using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Sentry; 

namespace PeopleWithResearch
{ 

    public  class CrashDetected
    {
            private static readonly Type[] IgnoredExceptions =
            {
                typeof(HttpRequestException),
                typeof(TaskCanceledException),
                typeof(WebException),
                typeof(OperationCanceledException)
            };

        public static void LogCrash(Exception ex, INavigation navigation, string sourceContext)
        {
            Task.Run(async () => await RecordSentryCrash(ex, sourceContext));
        }

        public static void LogCrash(Exception ex, string sourceContext)
        {
            Task.Run(async () => await RecordSentryCrash(ex, sourceContext));
        }

        private static async Task RecordSentryCrash(Exception ex, string sourceContext)
        {
            if (ex == null) return;

            var actualEx = ex is AggregateException aggEx ? aggEx.InnerException ?? ex : ex;

            //if (IgnoredExceptions.Any(type => type.IsAssignableFrom(actualEx.GetType())))
            //{
            //    return;
            //}

            try
            {
                string userId = Preferences.Default.Get("userid", "Unknown");
                string userEmail = Preferences.Default.Get("email", "Unknown");

                SentrySdk.CaptureException(actualEx, scope =>
                {
                    scope.User = new SentryUser { Id = userId, Email = userEmail };
                    scope.SetTag("context", sourceContext);
                });

                await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
            }
            catch (Exception semtex)
            {
                System.Diagnostics.Debug.WriteLine($"Sentry logging failed: {semtex.Message}");
            }
        }
    }
}
