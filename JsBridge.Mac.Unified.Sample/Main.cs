using System;

using AppKit;

namespace JsBridge.Mac.Unified.Sample
{
    static class MainClass
    {
        static void Main (string[] args)
        {
            NSApplication.Init ();
            NSApplication.Main (args);
        }
    }
}
