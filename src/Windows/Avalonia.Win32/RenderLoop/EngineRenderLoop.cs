using System;
using System.Collections.Generic;
using Avalonia.Rendering;

namespace Avalonia.Win32;

public class EngineRenderLoop : IRenderLoop
{
    private List<IRenderLoopTask> tasks = new List<IRenderLoopTask>();

    public EngineRenderLoop()
    {
		AvaloniaLocator.Current.GetService<IRenderTimer>().Tick += TimerTick;
    }

    public bool RunsInBackground => false;

    public void Add(IRenderLoopTask task) => tasks.Add(task);
    public void Remove(IRenderLoopTask task) => tasks.Remove(task);

    private void TimerTick(TimeSpan time)
    {
		// Update tasks.
		foreach (var task in tasks)
		{
			if (task.NeedsUpdate)
			{
				task.Update(time);
			}
		}

		// Render tasks.
		foreach (var task in tasks)
		{
			task.Render();
		}
    }
}
