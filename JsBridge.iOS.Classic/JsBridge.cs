using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

#if __UNIFIED__
using UIKit;
using Foundation;
using WebView = UIKit.UIWebView;
using Class = ObjCRuntime.Class;

// Mappings Unified CoreGraphic classes to MonoTouch classes
using CGRect = global::System.Drawing.RectangleF;
using CGSize = global::System.Drawing.SizeF;
using CGPoint = global::System.Drawing.PointF;

// Mappings Unified types to MonoTouch types
using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.WebKit;
using WebView = MonoMac.WebKit.WebView;
using Class = MonoMac.ObjCRuntime.Class;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using WebView = MonoTouch.UIKit.UIWebView;
using Class = MonoTouch.ObjCRuntime.Class;
#endif


namespace cdeutsch
{
	public static class JsBridge
	{
        static bool protocolRegistered = false;

        public static void EnableJsBridge() {
            if (!protocolRegistered) {              
                NSUrlProtocol.RegisterClass (new Class (typeof (AppProtocolHandler)));

                protocolRegistered = true;
            }
        }

        public static void InjectMtJavascript(this WebView webView) {
            #if MONOMAC
                InjectMtJavascript(webView, JsBridgeCore.MT_JAVASCRIPT);
            #else 
                webView.EvaluateJavascript(JsBridgeCore.MT_JAVASCRIPT);
            #endif
        }

        public static void InjectMtJavascript(this WebView webView, string script) {
            #if MONOMAC
                var document = webView.MainFrame.DomDocument;
                document.EvaluateWebScript(script);
            #else 
                webView.EvaluateJavascript(script);
            #endif
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
            source.BeginInvokeOnMainThread ( delegate{ 
                source.InjectMtJavascript(string.Format("Mt.App._dispatchEvent('{0}', {1});", EventName, json));
            });
		}		
	}
	

    public class AppProtocolHandler : NSUrlProtocol {

        [Export ("canInitWithRequest:")]
        public static bool canInitWithRequest (NSUrlRequest request)
        {
            return request.Url.Scheme == "app";
        }

        [Export ("canonicalRequestForRequest:")]
        public static new NSUrlRequest GetCanonicalRequest (NSUrlRequest forRequest)
        {
            return forRequest;
        }

        public AppProtocolHandler(IntPtr ptr) : base(ptr)
        {
        }

        #if __UNIFIED__
        public AppProtocolHandler (NSUrlRequest request, NSCachedUrlResponse cachedResponse, INSUrlProtocolClient client) 
            : base (request, cachedResponse, client)
        {
        }
        #else

        #if !MONOMAC
        [Export ("initWithRequest:cachedResponse:client:")]
        #endif
        public AppProtocolHandler (NSUrlRequest request, NSCachedUrlResponse cachedResponse, NSUrlProtocolClient client) 
            : base (request, cachedResponse, client)
        {
        }

        #endif

        public override void StartLoading ()
        {
			// parse callback function name.
			// EX: callback=jXHR.cb0&data=%7B%22hello%22%3A%22world%22%7D&_=0.3452287893742323
			var parameters = Request.Url.Query.Split('&');
			if (parameters.Length > 2) {
				var callbackToks = parameters[0].Split('=');
				var dataToks = parameters[1].Split('=');
				if (callbackToks.Length > 1 && dataToks.Length > 1) {


					// Determine what to do here based on the url 
					var appUrl = new AppUrl() {
						Module = Request.Url.Host,
                        Method = Request.Url.RelativePath.Substring(1),
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
					var data = NSData.FromString(callbackToks[1] + "({'success' : '1'});");
					using (var response = new NSUrlResponse (Request.Url, "text/javascript", Convert.ToInt32(data.Length), "utf-8")) {
                        Client.ReceivedResponse (this, response, NSUrlCacheStoragePolicy.NotAllowed);
                        Client.DataLoaded (this, data);
                        Client.FinishedLoading (this);
					}

					return;                            

				}
			}

			Client.FailedWithError(this, NSError.FromDomain(new NSString("AppProtocolHandler"), Convert.ToInt32(NSUrlError.ResourceUnavailable)));
			Client.FinishedLoading(this);
        }

        public override void StopLoading ()
        {
        }
        
    }

}

