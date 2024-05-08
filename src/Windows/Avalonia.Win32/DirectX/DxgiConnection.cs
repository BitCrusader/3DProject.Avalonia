using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Logging;
using Avalonia.OpenGL.Egl;
using Avalonia.Rendering;
using static Avalonia.Win32.Interop.UnmanagedMethods;
using static Avalonia.Win32.DirectX.DirectXUnmanagedMethods;
using MicroCom.Runtime;

namespace Avalonia.Win32.DirectX
{
    internal unsafe class DxgiConnection : IRenderTimer, IWindowsSurfaceFactory
    {
        public const uint ENUM_CURRENT_SETTINGS = unchecked((uint)(-1));

        public bool RunsInBackground => true;

        public event Action<TimeSpan>? Tick;
        private readonly object _syncLock;

        private Stopwatch? _stopwatch;
        private const string LogArea = "DXGI";

        public DxgiConnection(object syncLock)
        {
            _syncLock = syncLock;
        }
        
        public static bool TryCreateAndRegister()
        {
            try
            {
                TryCreateAndRegisterCore();
                return true;
            }
            catch (Exception ex)
            {
                Logger.TryGet(LogEventLevel.Error, LogArea)
                    ?.Log(null, "Unable to establish Dxgi: {0}", ex);
                return false;
            }
        }

        private void RunLoop()
        {
            _stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    lock (_syncLock)
                    {
                        Tick?.Invoke(_stopwatch.Elapsed);
                    }
                }
                catch (Exception ex)
                {
                    Logger.TryGet(LogEventLevel.Error, LogArea)
                                    ?.Log(this, $"Failed to wait for vblank, Exception: {ex.Message}, HRESULT = {ex.HResult}");
                }
            }
        }

        // Used the windows composition as a blueprint for this startup/creation 
        private static bool TryCreateAndRegisterCore()
        {
            var tcs = new TaskCompletionSource<bool>();
            var pumpLock = new object();
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    var connection = new DxgiConnection(pumpLock);

                    AvaloniaLocator.CurrentMutable.Bind<IWindowsSurfaceFactory>().ToConstant(connection);
                    AvaloniaLocator.CurrentMutable.Bind<IRenderTimer>().ToConstant(connection);
                    tcs.SetResult(true);
                    connection.RunLoop();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            thread.IsBackground = true;
            thread.SetApartmentState(System.Threading.ApartmentState.STA);
            thread.Start();
            // block until 
            return tcs.Task.Result;
        }

        public bool RequiresNoRedirectionBitmap => false;
        public object CreateSurface(EglGlPlatformSurface.IEglWindowGlPlatformSurfaceInfo info) => new DxgiSwapchainWindow(this, info);
    }
}
