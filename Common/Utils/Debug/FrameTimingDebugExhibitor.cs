// #define IGNORE_MEMORY_DEBUG
using System;
using UnityEngine;
using UnityEngine.Profiling;

[DefaultExecutionOrder(EXECUTION_ORDER)]
public abstract class FrameTimingDebugExhibitor : MonoBehaviour
{
	public const int EXECUTION_ORDER = 20000;

	protected float timer;

	[SerializeField] protected float tickInterval = 0.0f;
	[ExtendedDrawer(true),SerializeReference] IEventBinderReference _deepToogle;
    FpsCounter _fps;

	protected abstract void SetLog( string log );
	protected abstract void OnEnableChange( bool enabled );

    void OnEnable()
	{
		FrameTimingDebug.Enable();
		_deepToogle?.Register( TryToggleDeep );
        _fps = new FpsCounter( .3333f );
	}

	void OnDisable()
	{
		FrameTimingDebug.Disable();
		_deepToogle?.Unregister( TryToggleDeep );
	}

	void TryToggleDeep()
	{
		FrameTimingDebug.ToogleDeep();
	}
	
	public static string ToMemoryString(long bytes,string colorHex)
	{
		const long KB = 1024;
		const long MB = KB * 1024;
		const long GB = MB * 1024;

		if (bytes >= GB)   return $"<color={colorHex}>{bytes / (double)GB:G3}</color>GB";
		if (bytes >= MB)   return $"<color={colorHex}>{bytes / (double)MB:G3}</color>MB";
		if (bytes >= KB)   return $"<color={colorHex}>{bytes / (double)KB:G3}</color>KB";
		return $"<color={colorHex}>{bytes}</color>B";
	}
	
	public static string ToMemoryString(long bytes)
	{
		const long KB = 1024;
		const long MB = KB * 1024;
		const long GB = MB * 1024;

		if (bytes >= GB)   return $"{bytes / (double)GB:G3}GB";
		if (bytes >= MB)   return $"{bytes / (double)MB:G3}MB";
		if (bytes >= KB)   return $"{bytes / (double)KB:G3}KB";
		return $"{bytes}B";
	}

	static readonly (float,Color)[] FPS_COLORS = { ( 27, Colors.Crimson ), ( 30, Colors.OrangeRed ), ( 60, Colors.Gold ), ( 120, Colors.LawnGreen ) };
	static readonly (float,Color)[] MEM_COLORS = { ( .3f, Colors.MediumSpringGreen ), ( .5f, Colors.Gold ), ( .8f, Colors.OrangeRed ), ( 1, Colors.Crimson ) };
	static readonly string HEADERS = 
#if IGNORE_MEMORY_DEBUG
		$"<color=#{Colors.BrightLime.ToHex()}>FPS</color>\t<color=#{Colors.PowderBlue.ToHex()}>FRAME TIME</color>";
#else
		$"<color=#{Colors.BrightLime.ToHex()}>FPS</color>\t     <color=#{Colors.Blurple.ToHex()}>MONO</color>\t\t     <color=#{Colors.SandyBrown.ToHex()}>USED</color>\t\t<color=#{Colors.Tomato.ToHex()}>RESERV.</color>\t<color=#{Colors.PowderBlue.ToHex()}>FRAME TIME</color>";
#endif

	static readonly string DEFAULT_FIRSTLINE_NUMBERS_COLOR = Colors.Khaki.ToHex();

	void LateUpdate()
	{
		_fps.Update();
		var log = FrameTimingDebug.GetLog();
		
		if (tickInterval > Mathf.Epsilon)
		{
			timer += Time.unscaledDeltaTime;
			if (timer > tickInterval)
				timer %= tickInterval;
			else
				return;
		}

		var fpsValue = _fps.CurrentFps;
		var fpsColor = Colors.Mix( fpsValue, FPS_COLORS );
#if IGNORE_MEMORY_DEBUG
		SetLog($"{HEADERS}\n<color=#{DEFAULT_FIRSTLINE_NUMBERS_COLOR}> {fpsValue.ToStringColored( fpsColor )}\t   {log}");
#else
		long memMono = Profiler.GetMonoUsedSizeLong();
		long memUsed = Profiler.GetTotalAllocatedMemoryLong();
		long memReserved = Profiler.GetTotalReservedMemoryLong();

		var monoPctg = memMono*100/memReserved;
		var usedPctg = memUsed*100/memReserved;

		var monoColor = "#"+Colors.Mix( monoPctg * .01f, MEM_COLORS ).ToHex();
		var usedColor = "#"+Colors.Mix( usedPctg * .01f, MEM_COLORS ).ToHex();

		var coloredMonPtg = monoPctg.ToString().Colorfy(monoColor);
		var coloredUsedPtg = usedPctg.ToString().Colorfy(usedColor);

		SetLog($"{HEADERS}\n<color=#{DEFAULT_FIRSTLINE_NUMBERS_COLOR}>{fpsValue.ToStringColored( fpsColor )}\t {ToMemoryString(memMono,monoColor)}({coloredMonPtg}%)\t{ToMemoryString(memUsed,usedColor)}({coloredUsedPtg}%)\t {ToMemoryString(memReserved)}</color>\t  {log}");
#endif
		FrameTimingDebug.ClearUnusedData();
	}
}
