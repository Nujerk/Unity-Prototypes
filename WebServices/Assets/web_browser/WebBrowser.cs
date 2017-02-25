using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using Xilium.CefGlue;

public class WebBrowser : MonoBehaviour {

    public string startingPage;
    public RawImage textureHolder;

    protected WWW _currentPage;
    protected string _url;
    private BrowserClient _webclient;
    private CefWindowInfo _windowInfo;
    private CefBrowserSettings _browserSettings;

	void Start () {
        // Load CEF. This checks for the correct CEF version.
        CefRuntime.Load("D:\\repositories\\Unity-Library\\WebServices\\Assets\\Plugins");

        // Start the secondary CEF process.
        var cefMainArgs = new CefMainArgs(new string[0]);
        var cefApp = new BrowserApp();

        // This is where the code path diverges for child processes.
        if (CefRuntime.ExecuteProcess(cefMainArgs, cefApp, IntPtr.Zero) != -1) {
            Debug.Log("CefRuntime could not start the secondary process.");
        }

        // Settings for all of CEF (e.g. process management and control).
        var cfeSettings = new CefSettings {
            SingleProcess = false,
            MultiThreadedMessageLoop = true
        };

        // Start the browser process (a child process).
        CefRuntime.Initialize(cefMainArgs, cfeSettings, cefApp, IntPtr.Zero);

        // Instruct CEF to not render to a window at all.
        _windowInfo = CefWindowInfo.Create();
        _windowInfo.SetAsWindowless(IntPtr.Zero, false);

        // Settings for the browser window itself (e.g. should JavaScript be enabled?).
        _browserSettings = new CefBrowserSettings();

        // Initialize some the cust interactions with the browser process.
        // The browser window will be 1280 x 720 (pixels).
        _webclient = new BrowserClient(1280, 720);

        _url = startingPage;
        StartCoroutine(LoadPage());
	}
	
	void Update () {
		
	}

    IEnumerator LoadPage() {
        CefBrowserHost.CreateBrowser(_windowInfo, _webclient, _browserSettings, _url);
        yield return null;
    }

    internal class BrowserApp : CefApp { }
    public class BrowserClient : CefClient {
        private readonly BrowserLoadHandler _loadHandler;
        private readonly BrowserRenderHandler _renderHandler;

        private static System.Object sPixelLock;
        private static byte[] sPixelBuffer;

        private static CefBrowserHost sHost;

        protected override CefRenderHandler GetRenderHandler() {
            return _renderHandler;
        }

        protected override CefLoadHandler GetLoadHandler() {
            return _loadHandler;
        }

        public BrowserClient(int windowWidth, int windowHeight) {
            _renderHandler = new BrowserRenderHandler(windowWidth, windowHeight);
            _loadHandler = new BrowserLoadHandler();

            sPixelLock = new object();
            sPixelBuffer = new byte[windowWidth * windowHeight * 4];
        }

        public void UpdateTexture(Texture2D pTexture) {
            if (sHost != null) {
                lock (sPixelLock) {
                    pTexture.LoadRawTextureData(sPixelBuffer);
                    pTexture.Apply();
                }
            }
        }

        public void Shutdown() {
            if (sHost != null) {
                sHost.Dispose();
            }
        }

        public class BrowserLoadHandler : CefLoadHandler {
            protected override void OnLoadStart(CefBrowser browser, CefFrame frame) {
                // A single CefBrowser instance can handle multiple requests
                //   for a single URL if there are frames (i.e. <FRAME>, <IFRAME>).
                if (browser != null) {
                    sHost = browser.GetHost();
                }
                if (frame.IsMain) {
                    Console.WriteLine("START: {0}", browser.GetMainFrame().Url);
                }
            }

            protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode) {
                if (frame.IsMain) {
                    Console.WriteLine("END: {0}, {1}", browser.GetMainFrame().Url, httpStatusCode);
                }
            }
        }

        internal class BrowserRenderHandler : CefRenderHandler {
            private readonly int _windowHeight;
            private readonly int _windowWidth;

            public BrowserRenderHandler(int windowWidth, int windowHeight) {
                _windowWidth = windowWidth;
                _windowHeight = windowHeight;
            }

            protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect) {
                return GetViewRect(browser, ref rect);
            }

            protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY) {
                screenX = viewX;
                screenY = viewY;
                return true;
            }

            protected override bool GetViewRect(CefBrowser browser, ref CefRectangle rect) {
                rect.X = 0;
                rect.Y = 0;
                rect.Width = _windowWidth;
                rect.Height = _windowHeight;
                return true;
            }

            protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo) {
                return false;
            }

            protected override void OnPopupSize(CefBrowser browser, CefRectangle rect) {
            }

            protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, IntPtr buffer, int width, int height) {
                if (browser != null) {
                    lock (sPixelLock) {
                        Marshal.Copy(buffer, sPixelBuffer, 0, sPixelBuffer.Length);
                    }
                }
            }

            protected override void OnCursorChange(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo) {
            }

            protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y) {
            }
        }
    }
    


}
