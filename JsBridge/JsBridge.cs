using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace cdeutsch
{
	public static class JsBridge
	{
	
        // most of this library is borrowed from Titanium, Copyright 2008-2012 Appcelerator, Inc. under the Apache License Version 2 http://www.apache.org/licenses/LICENSE-2.0
        //I haven't implemented all the features they have.
		#region MT_JAVASCRIPT
		private static string MT_JAVASCRIPT = @"Mt = {};
Mt.appId = 'jsbridge';
Mt.pageToken = 'index';
Mt.App = {};
Mt.API = {};
Mt.App._listeners = {};
Mt.App._listener_id = 1;
Mt.App.id = Mt.appId;
Mt.App._xhr = XMLHttpRequest;
Mt._broker = function (module, method, data) {
    try {
        var url = 'app://' + Mt.appId + '/_MtA0_' + Mt.pageToken + '/' + module + '/' + method + '?' + Mt.App._JSON(data, 1);
        var xhr = new Mt.App._xhr();
        xhr.open('GET', url, false);
        xhr.send(); 
    } catch (ex) {
        console.log('error');
        console.log(JSON.stringify(ex));
    }
};
Mt._hexish = function (a) {
    var r = '';
    var e = a.length;
    var c = 0;
    var h;
    while (c < e) {
        h = a.charCodeAt(c++).toString(16);
        r += '\\\\u';
        var l = 4 - h.length;
        while (l-- > 0) {
            r += '0'
        }
        ;
        r += h
    }
    return r
};
Mt._bridgeEnc = function (o) {
    return'<' + Mt._hexish(o) + '>'
};
Mt.App._JSON = function (object, bridge) {
    var type = typeof object;
    switch (type) {
        case'undefined':
        case'function':
        case'unknown':
            return undefined;
        case'number':
        case'boolean':
            return object;
        case'string':
            if (bridge === 1)return Mt._bridgeEnc(object);
            return'""' + object.replace(/""/g, '\\\\""').replace(/\\n/g, '\\\\n').replace(/\\r/g, '\\\\r') + '""'
    }
    if ((object === null) || (object.nodeType == 1))return'null';
    if (object.constructor.toString().indexOf('Date') != -1) {
        return'new Date(' + object.getTime() + ')'
    }
    if (object.constructor.toString().indexOf('Array') != -1) {
        var res = '[';
        var pre = '';
        var len = object.length;
        for (var i = 0; i < len; i++) {
            var value = object[i];
            if (value !== undefined)value = Mt.App._JSON(value, bridge);
            if (value !== undefined) {
                res += pre + value;
                pre = ', '
            }
        }
        return res + ']'
    }
    var objects = [];
    for (var prop in object) {
        var value = object[prop];
        if (value !== undefined) {
            value = Mt.App._JSON(value, bridge)
        }
        if (value !== undefined) {
            objects.push(Mt.App._JSON(prop, bridge) + ': ' + value)
        }
    }
    return'{' + objects.join(',') + '}'
};
//CDeutsch: removing evtid param
//Mt.App._dispatchEvent = function (type, evtid, evt) {
Mt.App._dispatchEvent = function (type, evt) {
    var listeners = Mt.App._listeners[type];
    if (listeners) {
        for (var c = 0; c < listeners.length; c++) {
            var entry = listeners[c];
            //CDeutsch: changing to look for type so we only have to call once.
            //if (entry.id == evtid) {
                entry.callback.call(entry.callback, evt)
            //}
        }
    }
};
Mt.App.fireEvent = function (name, evt) {
    Mt._broker('App', 'fireEvent', {name:name, event:evt})
};
Mt.API.log = function (a, b) {
    Mt._broker('API', 'log', {level:a, message:b})
};
Mt.API.debug = function (e) {
    Mt._broker('API', 'log', {level:'debug', message:e})
};
Mt.API.error = function (e) {
    Mt._broker('API', 'log', {level:'error', message:e})
};
Mt.API.info = function (e) {
    Mt._broker('API', 'log', {level:'info', message:e})
};
Mt.API.fatal = function (e) {
    Mt._broker('API', 'log', {level:'fatal', message:e})
};
Mt.API.warn = function (e) {
    Mt._broker('API', 'log', {level:'warn', message:e})
};
Mt.App.addEventListener = function (name, fn) {
    var listeners = Mt.App._listeners[name];
    if (typeof(listeners) == 'undefined') {
        listeners = [];
        Mt.App._listeners[name] = listeners
    }
    var newid = Mt.pageToken + Mt.App._listener_id++;
    listeners.push({callback:fn, id:newid});
    //CDeutsch: not going to do this (don't see the advatange right now
    //Mt._broker('App', 'addEventListener', {name:name, id:newid})
};
Mt.App.removeEventListener = function (name, fn) {
    var listeners = Mt.App._listeners[name];
    if (listeners) {
        for (var c = 0; c < listeners.length; c++) {
            var entry = listeners[c];
            if (entry.callback == fn) {
                listeners.splice(c, 1);
                //CDeutsch: not going to do this (don't see the advatange right now
                //Mt._broker('App', 'removeEventListener', {name:name, id:entry.id});
                break
            }
        }
    }
};";
	#endregion

        static bool protocolRegistered = false;

        public static void EnableJsBridge() {
            if (!protocolRegistered) {              
                NSUrlProtocol.RegisterClass (new MonoTouch.ObjCRuntime.Class (typeof (AppProtocolHandler)));

                protocolRegistered = true;
            }
        }

        public static void InjectMtJavascript(this UIWebView webView) {
            webView.EvaluateJavascript(MT_JAVASCRIPT);
        }
		
		private static List<EventListener> EventListeners = new List<EventListener>();
		
		public static void AddEventListener (this UIWebView source, string EventName, Func<FireEventData, Void> Event) {
			EventListeners.Add( new EventListener(source, EventName, Event) );
		}
		
		public static void RemoveEventListener (this UIWebView source, string EventName, Func<FireEventData, Void> Event) {
			for(int xx = 0; xx < EventListeners.Count; xx++) {
				var ee = EventListeners[xx];
				if (source == ee.WebView 
				    && string.Compare(EventName, ee.EventName, StringComparison.InvariantCultureIgnoreCase) == 0
					&& ee.Event == Event) {
					EventListeners.RemoveAt(xx);
					break;
				}
			}
		}
		
		public static void FireEvent (this UIWebView source, string EventName, Object Data) {
			// call javascript event hanlder code
			string json = SimpleJson.SerializeObject(Data);
            source.BeginInvokeOnMainThread ( delegate{ 
			    source.EvaluateJavascript(string.Format("Mt.App._dispatchEvent('{0}', {1});", EventName, json));
            });
		}
		
		public static void JsEventFired (FireEventData feData) {
			foreach(var ee in EventListeners.Where(oo => string.Compare(oo.EventName, feData.Name, StringComparison.InvariantCultureIgnoreCase) == 0)) {				
				ee.Event(feData);				
			}
		}
		
	}
	

    public class AppProtocolHandler : NSUrlProtocol {
        static bool inited = false;

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

        [Export ("initWithRequest:cachedResponse:client:")]
        public AppProtocolHandler (NSUrlRequest request, NSCachedUrlResponse cachedResponse, NSUrlProtocolClient client) 
            : base (request, cachedResponse, client)
        {
        }

        public override void StartLoading ()
        {
            // Determine what to do here based on the url 
            var appUrl = AppUrl.ParseUrl(Request.Url);
            if (appUrl != null) {
                // this is a request from mt.js so handle it.
                switch (appUrl.Module.ToLower()) 
                {
                    case "app":
                        if (string.Equals(appUrl.Method, "fireEvent", StringComparison.InvariantCultureIgnoreCase)) {
                            // fire this event.
                            var feData = appUrl.DeserializeFireEvent();
                            // find event listeners for this event and trigger it.
                            JsBridge.JsEventFired(feData);
                        }
                    
                        break;
                        
                    case "api":
                        if (string.Equals(appUrl.Method, "log", StringComparison.InvariantCultureIgnoreCase)) {
                            // log output.
                            var lData = appUrl.DeserializeLog();
#if DEBUG
                            Console.WriteLine("[" + lData.Level + "]: " + lData.Message);
#endif
                        }
                    
                        break;
                }

                var data = new NSData();
                using (var response = new NSUrlResponse (Request.Url, "text/plain", Convert.ToInt32(data.Length), "utf-8")) {
                    Client.ReceivedResponse (this, response, NSUrlCacheStoragePolicy.NotAllowed);
                    Client.DataLoaded (this, data);
                    Client.FinishedLoading (this);
                }
                return;
                    
            }           
            Client.FailedWithError(this, NSError.FromDomain(new NSString("AppProtocolHandler"), Convert.ToInt32(NSUrlError.ResourceUnavailable)));
            Client.FinishedLoading(this);

            /*
            var value = Request.Url.Path.Substring (1);
            using (var image = Render (value)) {
                using (var response = new NSUrlResponse (Request.Url, "image/jpeg", -1, null)) {
                    using (var data = image.AsJPEG ()) {
                        Client.ReceivedResponse (this, response, NSUrlCacheStoragePolicy.NotAllowed);
                        Client.DataLoaded (this, data);
                        Client.FinishedLoading (this);
                    }
                }
            }
            */
        }

        public override void StopLoading ()
        {
        }
        
    }


	public class EventListener {
		public UIWebView WebView { get; set; }
		public string EventName { get; set; }
		public Func<FireEventData, Void> Event { get; set; }
		
		public EventListener() {
		}
		
		public EventListener(UIWebView WebView, string EventName, Func<FireEventData, Void> Event) {
			this.WebView = WebView;
			this.EventName = EventName;
			this.Event = Event;
		}
	}
	
	public class AppUrl {
		public string Module { get; set; }
		public string Method { get; set; }
		public string JsonData { get; set; }
		
		public Object Deserialize() {
			return SimpleJson.DeserializeObject(JsonData);
		}
		
		public T Deserialize<T>() {
			return SimpleJson.DeserializeObject<T>(JsonData);
		}
		
		public FireEventData DeserializeFireEvent() {
			if (string.Equals(Method, "fireEvent", StringComparison.InvariantCultureIgnoreCase)) {
				return new FireEventData(JsonData);
			}
			else {
				return null;
			}
		}
		
		public LogData DeserializeLog() {
			if (string.Equals(Method, "log", StringComparison.InvariantCultureIgnoreCase)) {
				return new LogData(JsonData);
			}
			else {
				return null;
			}
		}
		
		public static AppUrl ParseUrl(NSUrl Url) {
			
			if (string.Compare(Url.Scheme, "app", StringComparison.InvariantCultureIgnoreCase) == 0) {
				// this is a request from mt.js so parse all data in it.
				AppUrl appUrl = new AppUrl();
				
				string[] toks = Url.ToString().Split('/');
				
				if (toks.Length > 5) {
					appUrl.Module = toks[4];
					string[] toks2 = toks[5].Split('?');
					if (toks2.Length > 1) {
						// parse data	
						var hexedJson = System.Web.HttpUtility.UrlDecode(toks2[1]);
						//Console.WriteLine(hexedJson);
						
						var unhexedJson = UnHexify(hexedJson);
						//Console.WriteLine(unhexedJson);
						
						// replace quotes with escaped quotes and then replace <> with quotes
						appUrl.JsonData = unhexedJson.Replace("\"", "\\\"").Replace("<", "\"").Replace(">", "\"");
						//Console.WriteLine(appUrl.JsonData);
					}					
					appUrl.Method = toks2[0];
				}
				
				
				return appUrl;
			}	
			else {
				return null;
			}
		}
		
		private static string UnHexify(string Value) {
			return Regex.Replace( Value, @"\\\\u(?<Value>[a-zA-Z0-9]{4})",
            	m => {
                	return ((char) int.Parse( m.Groups["Value"].Value, NumberStyles.HexNumber )).ToString();
			});
		}
	}
	

	public class FireEventData {
		public string Name { get; set; }
		public JsonObject Data { get; set; }
		public string JsonData { get; set; }
		
		public FireEventData() {
		}
		
		public FireEventData(string Json) {
			JsonObject feData = (JsonObject)SimpleJson.DeserializeObject(Json);
			this.Name = feData["name"].ToString();
			this.Data = (JsonObject)feData["event"];
			// save json of data so user can desiralize in a typed object.
			this.JsonData = SimpleJson.SerializeObject(this.Data);
		}
	}
	

	public class LogData {
		public string Level { get; set; }
		public string Message { get; set; }
		
		public LogData() {
		}
		
		public LogData(string Json) {
			JsonObject lData = (JsonObject)SimpleJson.DeserializeObject(Json);
			this.Level = lData["level"].ToString();
            if (lData["message"] != null) {
			    this.Message = lData["message"].ToString();
            }
		}
	}

}

