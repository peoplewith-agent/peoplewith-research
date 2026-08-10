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

        /// <summary>
        /// Testable filter: returns true if this exception type should be silently ignored
        /// (transient network noise that should not flood Sentry).
        /// </summary>
        public static bool IsIgnoredException(Exception ex)
        {
            if (ex == null) return false;
            var actual = ex is AggregateException agg ? agg.InnerException ?? ex : ex;
            return IgnoredExceptions.Any(type => type.IsAssignableFrom(actual.GetType()));
        }
        /// <summary>
        /// Unwraps AggregateException to its first InnerException, or returns the exception unchanged.
        /// </summary>
        public static Exception UnwrapException(Exception ex)
        {
            if (ex is AggregateException agg)
                return agg.InnerException ?? ex;
            return ex;
        }


        private static async Task RecordSentryCrash(Exception ex, string sourceContext)
        {
            if (ex == null) return;

            var actualEx = ex is AggregateException aggEx ? aggEx.InnerException ?? ex : ex;

            // Re-enabled: filter transient network noise so Release crashes surface clearly
            if (IgnoredExceptions.Any(type => type.IsAssignableFrom(actualEx.GetType())))
            {
                return;
            }

            try
            {
                string userId    = Preferences.Default.Get("userid", "Unknown");
                string userEmail = Preferences.Default.Get("email",  "Unknown");

#if ANDROID
                SentrySdk.AddBreadcrumb(
                    message: "Android native UI context at crash",
                    category: "ui.layout",
                    level: BreadcrumbLevel.Error,
                    data: new Dictionary<string, string>
                    {
                        ["source"] = sourceContext
                    }
                );
#endif

                SentrySdk.CaptureException(actualEx, scope =>
                {
                    scope.User = new SentryUser { Id = userId, Email = userEmail };
                    scope.SetTag("context",      sourceContext);
                    scope.SetTag("platform",     DeviceInfo.Platform.ToString());
                    scope.SetTag("device_model", DeviceInfo.Model);
#if DEBUG
                    scope.SetTag("build_config", "Debug");
#else
                    scope.SetTag("build_config", "Release");
#endif
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
