using System;

using Foundation;
using AppKit;

namespace JsBridge.Mac.Unified.Sample
{
    public partial class AppDelegate : NSApplicationDelegate
    {
        MainWindowController mainWindowController;

        public AppDelegate ()
        {
        }

        public override void DidFinishLaunching (NSNotification notification)
        {
            mainWindowController = new MainWindowController ();
            mainWindowController.Window.MakeKeyAndOrderFront (this);
        }
    }
}
