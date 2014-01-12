using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using cdeutsch;

namespace JsBridgeMacSample
{
    public partial class MainWindowController : MonoMac.AppKit.NSWindowController
    {

        #region Constructors

        // Called when created from unmanaged code
        public MainWindowController (IntPtr handle) : base (handle)
        {
            Initialize ();
        }
        // Called when created directly from a XIB file
        [Export ("initWithCoder:")]
        public MainWindowController (NSCoder coder) : base (coder)
        {
            Initialize ();
        }
        // Call to load from the XIB/NIB file
        public MainWindowController () : base ("MainWindow")
        {
            Initialize ();
        }
        // Shared initialization code
        void Initialize ()
        {
        }

        #endregion

        //strongly typed window accessor
        public new MainWindow Window {
            get {
                return (MainWindow)base.Window;
            }
        }

        public override void AwakeFromNib ()
        {
            base.AwakeFromNib ();


            JsBridge.EnableJsBridge();

        

            //// load our local index.html file 
            // get path to file.
            string path = NSBundle.MainBundle.PathForResource( "www/index", "html" );
            // create an address and escape whitespace
            string address = string.Format("file:{0}", path).Replace( " ", "%20" );

            // be sure to enable JS Bridge before trying to fire events.
            webDisplay.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(address)));

            // listen for the doNativeStuff event triggered by the browser.
            webDisplay.AddEventListener( "doNativeStuff", delegate(FireEventData arg) {
                Console.WriteLine("doNativeStuff Callback:");   
                Console.WriteLine(arg.Data["msg"]);

                // trigger doBrowserStuff event in browser.
                webDisplay.FireEvent( "doBrowserStuff", new {
                    Message = "The Native code says hi back. ;)",
                    Extra = "more properties",
                    Success = true
                });
            });

            // listen for the nativeSheet event triggered by the browser.
            webDisplay.AddEventListener( "nativeSheet", delegate(FireEventData arg) {

                // show a native action sheet
                BeginInvokeOnMainThread (delegate { 


                    NSAlert alertView =NSAlert
                        .WithMessage( arg.Data["msg"].ToString(), "Cancel" ,"ok", "ok2", "");


                    alertView.RunModal ();

                });

            });

        }
    }
}

