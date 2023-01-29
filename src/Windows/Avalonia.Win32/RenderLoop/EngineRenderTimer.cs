using System;
using Avalonia.Rendering;
using Avalonia.Threading;

namespace Avalonia.Win32;

public class EngineRenderTimer : IRenderTimer
{
    public bool RunsInBackground => false;

    public event Action<TimeSpan> Tick = delegate {};

    public EngineRenderTimer()
    {
		Start();
    }

    private void Start()
    {
		// Run render on UI thread.
		DispatcherTimer.Run(RaiseTick, TimeSpan.Zero, DispatcherPriority.Render);
    }

    private bool RaiseTick()
    {
        Tick(TimeSpan.FromMilliseconds(Environment.TickCount));
		return true;
    }
}
