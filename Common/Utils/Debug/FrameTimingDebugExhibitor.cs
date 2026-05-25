// #define IGNORE_MEMORY_DEBUG
// #define IGNORE_GFX_DEBUG
using System;
using UnityEngine;
using UnityEngine.Profiling;

[DefaultExecutionOrder(EXECUTION_ORDER)]
public abstract class FrameTimingDebugExhibitor : MonoBehaviour
{
	public const int EXECUTION_ORDER = 20000;

	protected float timer;

	[SerializeField] protected float tickInterval = 0.333333f;
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

	public static string FillString( string text, int totalLength )
	{
		var padding = totalLength - text.Length;
		if( padding <= 0 ) return text;
		var left = padding / 2;
		var right = padding - left;
		return new string( ' ', left ) + text + new string( ' ', right );
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

	static readonly Colors.ColorThreshold[] FPS_COLORS = { ( 27, Colors.Crimson ), ( 30, Colors.OrangeRed ), ( 60, Colors.Gold ), ( 120, Colors.LawnGreen ) };
	static readonly Colors.ColorThreshold[] MEM_COLORS = { ( .3f, Colors.MediumSpringGreen ), ( .5f, Colors.Gold ), ( .8f, Colors.OrangeRed ), ( 1, Colors.Crimson ) };
	
	static readonly string HEADERS = $"<color=#{Colors.BrightLime.ToHex()}>FPS</color>\t" +
#if !IGNORE_MEMORY_DEBUG
		$"     <color=#{Colors.Blurple.ToHex()}>MONO</color>\t\t     <color=#{Colors.SandyBrown.ToHex()}>USED</color>\t\t<color=#{Colors.Tomato.ToHex()}>RESERV.</color>\t" +
#endif
#if !IGNORE_GFX_DEBUG
		$"       <color=#{Colors.SpringGreen.ToHex()}>API</color>\t\t<color=#{Colors.HotPink.ToHex()}>RESOLUTION</color>\t<color=#{Colors.MediumPurple.ToHex()}>SCALE</color>\t" +
#endif
		$"<color=#{Colors.PowderBlue.ToHex()}>FRAME TIME</color>";

	static readonly string HEADERS_WITH_SCALE = $"<color=#{Colors.BrightLime.ToHex()}>FPS</color>\t" +
#if !IGNORE_MEMORY_DEBUG
		$"     <color=#{Colors.Blurple.ToHex()}>MONO</color>\t\t     <color=#{Colors.SandyBrown.ToHex()}>USED</color>\t\t<color=#{Colors.Tomato.ToHex()}>RESERV.</color>\t" +
#endif
#if !IGNORE_GFX_DEBUG
		$"       <color=#{Colors.SpringGreen.ToHex()}>API</color>\t\t<color=#{Colors.HotPink.ToHex()}>RESOLUTION</color>\t" +
#endif
		$"<color=#{Colors.PowderBlue.ToHex()}>FRAME TIME</color>";

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

#if !IGNORE_GFX_DEBUG
		var gfxScale = ScalableBufferManager.widthScaleFactor;
		var scaleApproxOne = MathAdapter.Approximately( gfxScale, 1f );
		// scaleApproxOne = false;
		var api = SystemInfo.graphicsDeviceType;
		// api = (UnityEngine.Rendering.GraphicsDeviceType)( ( Time.frameCount / 30 ) % 30 );
		var apiStr = api.Pretty().FillSides( 12, ' ' );
		var apiStrColored = apiStr.Colorfy( Colors.TeaGreen );
		var w = Screen.width;
		var h = Screen.height;
		var digitCount = K10.Math.Log10( w ) + K10.Math.Log10( h ) + 2;
		var resStr = $"{w}x{h}".Colorfy( Colors.LightPink ) + ( digitCount < 7 ? "\t" : string.Empty );
		gfxScale = .8f;
		var scalePct = Mathf.RoundToInt( gfxScale * 100 );
		var scalePrefix = ( !scaleApproxOne && scalePct < 100 ) ? "  " : " ";
		var scaleStr = scaleApproxOne ? string.Empty : $"{scalePrefix}{scalePct}%\t".Colorfy( Colors.Plum );
		var gfxStr = $"  {apiStrColored}\t   {resStr}\t{scaleStr}";
		var headers = scaleApproxOne ? HEADERS_WITH_SCALE : HEADERS;
#else
		var gfxStr = string.Empty;
		var headers = HEADERS;
#endif

#if IGNORE_MEMORY_DEBUG
		SetLog($"{headers}\n<color=#{DEFAULT_FIRSTLINE_NUMBERS_COLOR}> {fpsValue.ToString().FillSides(4).Colorfy( fpsColor )}\t   {gfxStr}   {log}");
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

		SetLog($"{headers}\n<color=#{DEFAULT_FIRSTLINE_NUMBERS_COLOR}>{fpsValue.ToString().FillSides(4).Colorfy( fpsColor )}\t {ToMemoryString(memMono,monoColor)}({coloredMonPtg}%)\t{ToMemoryString(memUsed,usedColor)}({coloredUsedPtg}%)\t {ToMemoryString(memReserved)}</color>\t{gfxStr}   {log}");
#endif
		FrameTimingDebug.ClearUnusedData();
	}
}
