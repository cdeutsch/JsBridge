using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace cdeutsch
{
	public static class JsBridgeCore
	{
		// most of this library is borrowed from Titanium, Copyright 2008-2012 Appcelerator, Inc. under the Apache License Version 2 http://www.apache.org/licenses/LICENSE-2.0
		//I haven't implemented all the features they have.
		#region MT_JAVASCRIPT
		public static string MT_JAVASCRIPT = @"
// jXHR library
(function (global) {
    var SETTIMEOUT = global.setTimeout, // for better compression
        doc = global.document,
        callback_counter = 0;

    global.jXHR = function () {
        var script_url,
            script_loaded,
            jsonp_callback,
            scriptElem,
            publicAPI = null;

        function removeScript() { try { scriptElem.parentNode.removeChild(scriptElem); } catch (err) { } }

        function reset() {
            script_loaded = false;
            script_url = '';
            removeScript();
            scriptElem = null;
            fireReadyStateChange(0);
        }

        function ThrowError(msg) {
            try { publicAPI.onerror.call(publicAPI, msg, script_url); } catch (err) { throw new Error(msg); }
        }

        function handleScriptLoad() {
            if ((this.readyState && this.readyState !== 'complete' && this.readyState !== 'loaded') || script_loaded) { return; }
            this.onload = this.onreadystatechange = null; // prevent memory leak
            script_loaded = true;
            if (publicAPI.readyState !== 4) ThrowError('Script failed to load [' + script_url + '].');
            removeScript();
        }

        function fireReadyStateChange(rs, args) {
            args = args || [];
            publicAPI.readyState = rs;
            if (typeof publicAPI.onreadystatechange === 'function') publicAPI.onreadystatechange.apply(publicAPI, args);
        }

        publicAPI = {
            onerror: null,
            onreadystatechange: null,
            readyState: 0,
            open: function (method, url) {
                reset();
                internal_callback = 'cb' + (callback_counter++);
                (function (icb) {
                    global.jXHR[icb] = function () {
                        try { fireReadyStateChange.call(publicAPI, 4, arguments); }
                        catch (err) {
                            publicAPI.readyState = -1;
                            ThrowError('Script failed to run [' + script_url + '].');
                        }
                        global.jXHR[icb] = null;
                    };
                })(internal_callback);
                script_url = url.replace(/=\?/, '=jXHR.' + internal_callback);
                fireReadyStateChange(1);
            },
            send: function () {
                SETTIMEOUT(function () {
                    scriptElem = doc.createElement('script');
                    scriptElem.setAttribute('type', 'text/javascript');
                    scriptElem.onload = scriptElem.onreadystatechange = function () { handleScriptLoad.call(scriptElem); };
                    scriptElem.setAttribute('src', script_url);
                    doc.getElementsByTagName('head')[0].appendChild(scriptElem);
                }, 0);
                fireReadyStateChange(2);
            },
            setRequestHeader: function () { }, // noop
            getResponseHeader: function () { return ''; }, // basically noop
            getAllResponseHeaders: function () { return []; } // ditto
        };

        reset();

        return publicAPI;
    };
})(window);

// most of this library is borrowed from Titanium, Copyright 2008-2012 Appcelerator, Inc. under the Apache License Version 2 http://www.apache.org/licenses/LICENSE-2.0
//I haven't implemented all the features they have.
Mt = {};
Mt.appId = 'jsbridge';
Mt.pageToken = 'index';
Mt.App = {};
Mt.API = {};
Mt.App._listeners = {};
Mt.App._listener_id = 1;
Mt.App.id = Mt.appId;
Mt.App._xhr = jXHR;
Mt._broker = function (module, method, data) {
    var x1 = new Mt.App._xhr();
    x1.onerror = function (e) {
        console.log('XHR error:' + JSON.stringify(e));
    };
    var url = 'app://' + module + '/' + method + '?callback=?&data=' + encodeURIComponent(JSON.stringify(data)) + '&_=' + Math.random();
    x1.open('GET', url);
    x1.send();
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
            return '""""' + object.replace(/""""/g, '\\\\""""').replace(/\\n/g, '\\\\n').replace(/\\r/g, '\\\\r') + '""""'
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
        			
		private static List<EventListener> EventListeners = new List<EventListener>();

        public static void AddEventListener (Object Source, string EventName, Action<FireEventData> Event) {
			EventListeners.Add( new EventListener(Source, EventName, Event) );
		}

        public static void RemoveEventListener (Object Source, string EventName, Action<FireEventData> Event) {
			for(int xx = 0; xx < EventListeners.Count; xx++) {
				var ee = EventListeners[xx];
                if (Source == ee.Source
                    && string.Compare(EventName, ee.EventName, StringComparison.InvariantCultureIgnoreCase) == 0
					&& ee.Event == Event) {
					EventListeners.RemoveAt(xx);
					break;
				}
			}
		}

		public static void JsEventFired (FireEventData feData) {
			foreach(var ee in EventListeners.Where(oo => string.Compare(oo.EventName, feData.Name, StringComparison.InvariantCultureIgnoreCase) == 0)) {				
				ee.Event(feData);				
			}
		}
	}

	public class EventListener {
        public Object Source { get; set; }
		public string EventName { get; set; }
		public Action<FireEventData> Event { get; set; }

		public EventListener() {
		}

        public EventListener(Object Source, string EventName, Action<FireEventData> Event) {
            this.Source = Source;
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

