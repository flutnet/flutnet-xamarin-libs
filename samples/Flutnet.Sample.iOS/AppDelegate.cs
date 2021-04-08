using Flutnet.Interop;
using Foundation;
using UIKit;

namespace Flutnet.Sample
{
    [Register("AppDelegate")]
    public class AppDelegate : FlutterAppDelegate
    {
        [Export("application:didFinishLaunchingWithOptions:")]
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            return base.FinishedLaunching(application, launchOptions);
        }
    }
}