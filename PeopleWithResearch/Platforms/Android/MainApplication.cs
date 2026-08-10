using Android.App;
using Android.Runtime;

namespace PeopleWithResearch
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
               var check = args.Exception;
                //ConfigureSentryUserScope();
                // SentrySdk.CaptureException(args.Exception);
            };
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
