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
            JsBridge.EnableJsBridge();

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			
            // the MT_JsBridgeViewController just contains a single UIWebView exposed as "WebView".
			viewController = new MT_JsBridgeViewController ();
			window.RootViewController = viewController;
			window.MakeKeyAndVisible ();
			
            //// load our local index.html file	
			// get path to file.
			string path = NSBundle.MainBundle.PathForResource( "www/index", "html" );
			// create an address and escape whitespace
			string address = string.Format("file:{0}", path).Replace( " ", "%20" );
			
			// be sure to enable JS Bridge before trying to fire events.
			viewController.WebView.LoadRequest(new NSUrlRequest(new NSUrl(address)));
			
			// listen for the doNativeStuff event triggered by the browser.
			viewController.WebView.AddEventListener( "doNativeStuff", delegate(FireEventData arg) {
				Console.WriteLine("doNativeStuff Callback:");	
				Console.WriteLine(arg.Data["msg"]);
				
				// trigger doBrowserStuff event in browser.
				viewController.WebView.FireEvent( "doBrowserStuff", new {
					Message = "The Native code says hi back. ;)",
                    Extra = "more properties",
                    Success = true
				});
			});
			
            // listen for the nativeSheet event triggered by the browser.
            viewController.WebView.AddEventListener( "nativeSheet", delegate(FireEventData arg) {

                // show a native action sheet
                BeginInvokeOnMainThread (delegate { 
                    var sheet = new UIActionSheet ( "Your Action Sheet" );
                    sheet.AddButton ( arg.Data["msg"].ToString() );
                    sheet.AddButton ( "Cancel" );
                    sheet.CancelButtonIndex = 1;
                    sheet.ShowInView ( viewController.View );
                });

            });

			return true;
		}
	}
}

