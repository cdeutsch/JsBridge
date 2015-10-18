using System;
using System.Drawing;
using cdeutsch;

using Foundation;
using UIKit;

namespace JsBridge.iOS.Unified.Sample
{
    public partial class HybridViewController : UIViewController
    {
        static bool UserInterfaceIdiomIsPhone {
            get { return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone; }
        }

        public HybridViewController (IntPtr handle) : base (handle)
        {
        }

        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();

            // Release any cached data, images, etc that aren't in use.
        }

        #region View lifecycle

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();

            cdeutsch.JsBridge.EnableJsBridge();

            //// load our local index.html file 
            // get path to file.
            string path = NSBundle.MainBundle.PathForResource( "www/index", "html" );
            // create an address and escape whitespace
            string address = string.Format("file:{0}", path).Replace( " ", "%20" );

            // be sure to enable JS Bridge before trying to fire events.
            webView.LoadRequest(new NSUrlRequest(new NSUrl(address)));

            // listen for the doNativeStuff event triggered by the browser.
            webView.AddEventListener( "doNativeStuff", delegate(FireEventData arg) {
                Console.WriteLine("doNativeStuff Callback:");   
                Console.WriteLine(arg.Data["msg"]);

                // trigger doBrowserStuff event in browser.
                webView.FireEvent( "doBrowserStuff", new {
                    Message = "The Native code says hi back. ;)",
                    Extra = "more properties",
                    Success = true
                });
            });

            // listen for the nativeSheet event triggered by the browser.
            webView.AddEventListener( "nativeSheet", delegate(FireEventData arg) {

                // show a native action sheet
                BeginInvokeOnMainThread (delegate { 
                    var sheet = new UIActionSheet ( "Your Action Sheet" );
                    sheet.AddButton ( arg.Data["msg"].ToString() );
                    sheet.AddButton ( "Cancel" );
                    sheet.CancelButtonIndex = 1;
                    sheet.ShowInView ( View );
                });

            });

            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
        }

        public override void ViewWillDisappear (bool animated)
        {
            base.ViewWillDisappear (animated);
        }

        public override void ViewDidDisappear (bool animated)
        {
            base.ViewDidDisappear (animated);
        }

        #endregion

    }
}

