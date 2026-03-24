using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class ScreenVisibilityCheckSystem : IOrchestratedUpdate, IDisposable
{
	static ScreenVisibilityCheckSystem _instance;
	
	private List<IScreenVisibilitySetter> _elements = new();
	private ListSetCollection<int> _toRemove = new();

	[SerializeField] Camera _camera;
	int _checkedElementsOnThisFrame;

	int _frameOffset;
	bool _waitingJobResult = false;
	bool _isDirty = false;
	bool _isRegistered = false;

	const int MAX_ELEMENTS_PER_BATCH = 32;
    bool _cameraWasNull = false;

	NativeArray<float3> _positions;
	NativeArray<float3> _vpos;
	NativeArray<byte> _result;

	JobHandle _jobHandle;

	private void Alloc()
	{
		_positions = new NativeArray<float3>(MAX_ELEMENTS_PER_BATCH, Allocator.Persistent);
		_vpos = new NativeArray<float3>(MAX_ELEMENTS_PER_BATCH, Allocator.Persistent);
		_result = new NativeArray<byte>(MAX_ELEMENTS_PER_BATCH, Allocator.Persistent);
	}

	public void Dispose()
	{
		_positions.Dispose();
		_vpos.Dispose();
		_result.Dispose();

		//Since it will stop check for visibility, all will be visible
		var elementsCount = _elements.Count;
		for (int i = 0; i < elementsCount; i++)
		{
			var element = _elements[i];
			if( element == null ) continue;
			if( element is UnityEngine.Object uObj && uObj == null ) continue; 
			element.SetVisibility(true);
		}

		_elements.Clear();
		_toRemove.Clear();
	}

    public static void Add(IScreenVisibilitySetter element)
    {
		if( _instance == null ) 
		{
			_instance = new();
			_instance.Alloc();
		}
		if( !_instance._isRegistered )
		{
			CodeOrchestrator.Eternal.Add( _instance );
			_instance._isRegistered = true;
		}
		_instance._elements.Add( element );
    }

    public static void Remove(IScreenVisibilitySetter element)
    {
		if( _instance == null ) return;
		var id = _instance._elements.IndexOf( element );
		if( id == -1 ) return;
		if( _instance._waitingJobResult )
		{
			_instance._toRemove.Add( id );
			_instance._isDirty = true;
		} 
		else
		{
			_instance._elements.RemoveAt( id );
			if( _instance._elements.Count == 0 )
			{
				CodeOrchestrator.Eternal.Remove( _instance );
				_instance._isRegistered = false;
			}
		}
    }

    public void PreUpdate()
	{
		_waitingJobResult = true;
#if CODE_METRICS
		const string CODE_TAG = "ScreenVisibilityCheckSystem.PreUpdate";
		FrameTimingDebug.LogStart( CODE_TAG );
#endif
		if (_camera == null) _camera = Camera.main;
		if (_camera == null)
		{
			_jobHandle = default;
			return;
		}
		if (!_camera.isActiveAndEnabled) _camera = Camera.main;
		var camera = _camera;

		if( camera == null ) 
		{
			if( !_cameraWasNull ) Debug.LogError( $"{"ScreenVisibilityCheckSystem".Colorfy( Colors.Console.TypeName )} {"CANNOT".Colorfy( Colors.Console.Danger )} find camera to calculate, so is {"NOT".Colorfy( Colors.Console.Danger )} running" );
			_cameraWasNull = true;
			return;
		}
		if (_cameraWasNull)
		{
			Debug.Log( $"{"ScreenVisibilityCheckSystem".Colorfy( Colors.Console.TypeName )} found camera to calculate and is running again" );
			_cameraWasNull = false;
		}

		var mat = camera.projectionMatrix * camera.worldToCameraMatrix;

		var eCount = _elements.Count;
		_checkedElementsOnThisFrame = Mathf.Min(eCount - _frameOffset, MAX_ELEMENTS_PER_BATCH );

		// Has a limited buget of elements to check on each frame that is MAX_ELEMENTS_PER_BATCH
		// So each frame verify 

		for (int i = 0; i < _checkedElementsOnThisFrame; i++)
		{
			var e = _elements[_frameOffset + i];
			_positions[i] = e.Position;
		}

		var job = new Job()
		{
			worldToScreenTransform = mat,
			positions = _positions,
			visible = _result,
			nearClip = camera.nearClipPlane,
			farClip = camera.farClipPlane,
			vPos = _vpos,
		};

		_jobHandle = job.Schedule(_checkedElementsOnThisFrame, 1);
#if CODE_METRICS
		FrameTimingDebug.LogEnd( CODE_TAG );
#endif
	}

    public void PostUpdate()
    {
#if CODE_METRICS
		const string CODE_TAG = "ScreenVisibilityCheckSystem.PostUpdate";
		FrameTimingDebug.LogStart( CODE_TAG );
		const string JOB_CODE_TAG = "ScreenVisibilityCheckSystem.PostUpdate.jobHandle.Complete";
		FrameTimingDebug.LogStart( JOB_CODE_TAG );
#endif
		_jobHandle.Complete();
#if CODE_METRICS
		FrameTimingDebug.LogEnd( JOB_CODE_TAG );
#endif

		if( _isDirty ) CheckResultBackDirty();
		else CheckResultBackClean();

		_waitingJobResult = false;

#if CODE_METRICS
		FrameTimingDebug.LogEnd( CODE_TAG );
#endif
    }

	void CheckResultBackClean()
	{
		for (int i = 0; i < _checkedElementsOnThisFrame; i++)
		{
			var realId = _frameOffset + i;
			_elements[realId].SetVisibility(_result[i] != byte.MinValue);
		}
		var eCount = _elements.Count;
		_frameOffset = ( eCount > 0 ) ? ( ( _frameOffset + _checkedElementsOnThisFrame ) % eCount ) : 0;
	}

	void CheckResultBackDirty()
	{
		var realElementsCheckedBack = 0;
		for (int i = 0; i < _checkedElementsOnThisFrame; i++)
		{
			var realId = _frameOffset + i;
			if( _toRemove.Contains( realId ) ) continue;
			realElementsCheckedBack++;
			_elements[realId].SetVisibility(_result[i] != byte.MinValue);
		}

		RemoveDelayedElementsToRemove();

		var eCount = _elements.Count;
		_frameOffset += realElementsCheckedBack;
		if( _frameOffset >= eCount ) _frameOffset = 0;

		if( eCount == 0 )
		{
			CodeOrchestrator.Eternal.Remove( _instance );
			_instance._isRegistered = false;
		}
	}

	void RemoveDelayedElementsToRemove()
	{
		var toRemoveCount = _toRemove.Count;
		_toRemove.Sort();

		for (int i = toRemoveCount - 1; i >= 0; i--)
		{
			var idToRemove = _toRemove[i];
			_elements.RemoveAt( idToRemove );
		}

		_toRemove.Clear();
		_isDirty = false;
	}

    [BurstCompile]
	struct Job : IJobParallelFor
	{
		const float TOLERANCE = 0.5f;
		const float MIN_TOLERANCE = -MAX_TOLERANCE;
		const float MAX_TOLERANCE = 1 + TOLERANCE;

		[ReadOnly] public NativeArray<float3> positions;
		[ReadOnly] public float4x4 worldToScreenTransform;
		[ReadOnly] public float nearClip;
		[ReadOnly] public float farClip;
		public NativeArray<float3> vPos;
		public NativeArray<byte> visible;

		public void Execute(int i)
		{
			var wPos = new float4(positions[i], 1);
			wPos.y = wPos.y + .5f; // Hack to reach center of characters and only charge at oher threads
			var temp = math.mul(worldToScreenTransform, wPos);
			var vPos = new float3(temp.x / temp.w, temp.y / temp.w, temp.z);
			this.vPos[i] = vPos;
			visible[i] = (vPos.x > MIN_TOLERANCE &&
							vPos.x < MAX_TOLERANCE &&
							vPos.y > MIN_TOLERANCE &&
							vPos.y < MAX_TOLERANCE) ?
							byte.MaxValue : byte.MinValue;
		}
	}
}