using System;
using System.Collections.Generic;
using System.Linq;

using Android.Webkit;
using Java.Interop;
using System.IO;
using System.Text;

namespace cdeutsch
{
	public static class JsBridge
	{
		
		public static void InjectMtJavascript(this WebView webView) {
            webView.LoadUrl("javascript:" + JsBridgeCore.MT_JAVASCRIPT);
		}

		public static void InjectMtJavascript(this WebView webView, string script) {
			webView.LoadUrl("javascript:" + script);
		}
			
		public static void AddEventListener (this WebView source, string EventName, Action<FireEventData> Event) {
            JsBridgeCore.AddEventListener(source, EventName, Event);
		}

		public static void RemoveEventListener (this WebView source, string EventName, Action<FireEventData> Event) {
            JsBridgeCore.RemoveEventListener(source, EventName, Event);
		}

		public static void FireEvent (this WebView source, string EventName, Object Data) {
			// call javascript event hanlder code
			string json = SimpleJson.SerializeObject(Data);
			source.InjectMtJavascript(string.Format("Mt.App._dispatchEvent('{0}', {1});", EventName, json));
		}
	}

	public class HybridWebViewClient : WebViewClient
	{
		public override WebResourceResponse ShouldInterceptRequest (WebView webView, string url)
		{
			// If the URL is not our own custom scheme, just let the webView load the URL as usual
			var scheme = "app:";

			if (!url.StartsWith (scheme)) {
				return base.ShouldInterceptRequest (webView, url);
			}

			// This handler will treat everything between the protocol and "?"
			// as the method name.  The querystring has all of the parameters.
			var resources = url.Substring (scheme.Length).Split ('?');



			var parameters = resources[1].Split('&');
			if (parameters.Length > 2) {
				var callbackToks = parameters[0].Split('=');
				var dataToks = parameters[1].Split('=');
				if (callbackToks.Length > 1 && dataToks.Length > 1) {


					// Determine what to do here based on the url 
					var urlToks = resources[0].Substring(2).Split('/');
					var appUrl = new cdeutsch.AppUrl() {
						Module = urlToks[0],
						Method = urlToks[1],
						JsonData = System.Web.HttpUtility.UrlDecode(dataToks[1])
					};

					// this is a request from mt.js so handle it.
					switch (appUrl.Module.ToLower()) 
					{
					case "app":
						if (string.Equals(appUrl.Method, "fireEvent", StringComparison.InvariantCultureIgnoreCase)) {
							// fire this event.
							var feData = appUrl.DeserializeFireEvent();
							// find event listeners for this event and trigger it.
							JsBridgeCore.JsEventFired(feData);
						}

						break;

					case "api":
						if (string.Equals(appUrl.Method, "log", StringComparison.InvariantCultureIgnoreCase)) {
							// log output.
							var lData = appUrl.DeserializeLog();
							#if DEBUG
							Console.WriteLine("BROWSER:[" + lData.Level + "]: " + lData.Message);
							#endif
						}

						break;
					}

					// indicate success.
					var js = callbackToks[1] + "({'success' : '1'});";
					return new WebResourceResponse ("application/javascript", "UTF-8", new MemoryStream(Encoding.UTF8.GetBytes(js)));
				}
			}

			/*
				var method = resources [0];
				var parameters = System.Web.HttpUtility.ParseQueryString (resources [1]);
				if (method == "UpdateLabel") {
					var textbox = parameters ["textbox"];

					// Add some text to our string here so that we know something
					// happened on the native part of the round trip.
					var prepended = string.Format ("C# says \"{0}\"", textbox);

					// Build some javascript using the C#-modified result
					var js = string.Format ("SetLabelText('{0}');", prepended);

					using (MemoryStream stream = new MemoryStream ()) {
						StreamWriter writer = new StreamWriter (stream);
						writer.Write ("javascript:" + js);
						writer.Flush ();
						stream.Position = 0;

						return new WebResourceResponse ("", "UTF-8", stream);
					}
				}
				*/

			return base.ShouldInterceptRequest (webView, url);
		}
	}
}

