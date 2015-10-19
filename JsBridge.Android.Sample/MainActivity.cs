using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using System.IO;
using System.Text;
using cdeutsch;

namespace JsBridge.Android.Sample
{
	[Activity (Label = "JsBridge.Android.Sample", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);


			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			var webView = FindViewById<WebView> (Resource.Id.webView);
			webView.Settings.JavaScriptEnabled = true;

			// Use subclassed WebViewClient to intercept hybrid native calls
            var jsBridgeWebViewClient = new cdeutsch.JsBridgeWebViewClient();
			webView.SetWebViewClient(jsBridgeWebViewClient);

			// Render the view from the type generated from RazorView.cshtml
			var model = new Model1() { Text = "Page Title from the Razor Model" };
			var template = new RazorView() { Model = model };
			var page = template.GenerateString();

			// Load the rendered HTML into the view with a base URL 
			// that points to the root of the bundled Assets folder
			webView.LoadDataWithBaseURL ("file:///android_asset/", page, "text/html", "UTF-8", null);

			// listen for the doNativeStuff event triggered by the browser.
			webView.AddEventListener( "doNativeStuff", delegate(FireEventData arg) {
				Console.WriteLine("doNativeStuff Callback:");	
				Console.WriteLine(arg.Data["msg"]);

				RunOnUiThread(() => {
					// trigger doBrowserStuff event in browser.
					webView.FireEvent( "doBrowserStuff", new {
						Message = "The Native code says hi back. ;)",
						Extra = "more properties",
						Success = true
					});
                });
			});

			// listen for the nativeSheet event triggered by the browser.
			webView.AddEventListener( "nativeSheet", delegate(FireEventData arg) {
				Console.WriteLine("nativeSheet");

                //run the alert in UI thread to display in the screen
                RunOnUiThread (() => {
                    //set alert for executing the task
                    AlertDialog.Builder alert = new AlertDialog.Builder (this);

                    alert.SetTitle (arg.Data["msg"].ToString());

                    alert.SetPositiveButton ("Ok", (senderAlert, args) => {
                        // handle ok pressed.
                    });

                    alert.Show();
                });
			});

		}


	}
}

