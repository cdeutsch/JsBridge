using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;

namespace cdeutsch
{
	public class XHRBridge : AppProtocolHandler
	{
		public XHRBridge ()
		{
		}
		
		
	}
	
	public class AppProtocolHandler : NSUrlProtocol {
		static bool inited = false;
		
		public static void RegisterSpecialProtocol() {
			if (!inited) {
				// TODO: make sure this is the correct syntax.
				NSUrlProtocol.RegisterClass( new Class(typeof(AppProtocolHandler)) );
				inited = true;
			}
		}
		
		public AppProtocolHandler() {
			Console.WriteLine("init");
		}
		
		public override void StartLoading ()
		{
			base.StartLoading ();
		}

		public override void StopLoading ()
		{
			base.StopLoading ();
		}

		public override bool CanInitWithRequest (NSUrlRequest request)
		{
			//return base.CanInitWithRequest (request);
			return true;
		}

		public override NSCachedUrlResponse CachedResponse {
			get {
				return base.CachedResponse;
			}
		}

		public override IntPtr ClassHandle {
			get {
				return base.ClassHandle;
			}
		}
	}
		
}

