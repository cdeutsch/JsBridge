using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace cdeutsch
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		// class-level declarations
		UIWindow window;
		MT_JsBridgeViewController viewController;

		//
		// This method is invoked when the application has loaded and is ready to run. In this 
		// method you should instantiate the window, load the UI into it and then make the window
		// visible.
		//
		// You have 17 seconds to return from this method, or iOS will terminate your application.
		//
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
			viewController = new MT_JsBridgeViewController ();
			window.RootViewController = viewController;
			window.MakeKeyAndVisible ();
						
			//viewController.WebView.LoadRequest(new NSUrlRequest(new NSUrl("http://slashdot.org/")));
			
			// get path to file.
			string path = NSBundle.MainBundle.PathForResource("www/index", "html");
			// create an address and escape whitespace
			string address = string.Format("file:{0}", path).Replace(" ", "%20");
			
			//AppProtocolHandler.RegisterSpecialProtocol();
			
			viewController.WebView.EnableJsBridge();
			viewController.WebView.LoadRequest(new NSUrlRequest(new NSUrl(address)));
			
			viewController.WebView.AddEventListener("doNativeStuff", delegate(FireEventData arg) {
				Console.WriteLine("doNativeStuff Callback:");	
				Console.WriteLine(arg.Event["msg"]);
				
				// fire msg back
				viewController.WebView.FireEvent("doBrowserStuff", new LogData() {
					Level = "log",
					Message = "The Native code says hi back. ;)"
				});
			});
			
			return true;
		}
	}
}

