// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace JsBridgeMacSample
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		WebKit.WebView webDisplay { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (webDisplay != null) {
				webDisplay.Dispose ();
				webDisplay = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
