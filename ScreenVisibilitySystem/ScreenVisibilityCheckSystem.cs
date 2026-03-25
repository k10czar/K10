using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public sealed class ScreenVisibilityCheckSystem : IOrchestratedUpdate, IDisposable
{
	const int MAX_ELEMENTS_START = 100;
	const int MAX_ELEMENTS_NEW_ALLOC = 50;
	static ScreenVisibilityCheckSystem _instance;
	
	private List<IScreenVisibilitySetter> _elements = new();
	private ListSetCollection<int> _toRemove = new();

	[SerializeField] Camera _camera;

	int _checkedElementsOnThisFrame;

	bool _waitingJobResult = false;
	bool _isDirty = false;
	bool _isRegistered = false;

	int _maxElements = 0;
    bool _cameraWasNull = false;

	NativeArray<float3> _positions;
	NativeArray<float3> _vpos;
	NativeArray<byte> _result;

	JobHandle _jobHandle;

	private void RequestSize( int requestedSize )
	{
		if( requestedSize <= _maxElements ) return;

		if( _positions.IsCreated ) _positions.Dispose();
		if( _vpos.IsCreated ) _vpos.Dispose();
		if( _result.IsCreated ) _result.Dispose();

		var sizeWas = _maxElements;
		
		if( _maxElements < MAX_ELEMENTS_START ) _maxElements = MAX_ELEMENTS_START;
		while( requestedSize > _maxElements ) _maxElements += MAX_ELEMENTS_NEW_ALLOC;

		Debug.Log( $"***** 🤏 was {sizeWas} < {requestedSize} then will be {_maxElements}" );

		_positions = new NativeArray<float3>(_maxElements, Allocator.Persistent);
		_vpos = new NativeArray<float3>(_maxElements, Allocator.Persistent);
		_result = new NativeArray<byte>(_maxElements, Allocator.Persistent);
	}

	public void Dispose()
	{
		if( _positions.IsCreated ) _positions.Dispose();
		if( _vpos.IsCreated ) _vpos.Dispose();
		if( _result.IsCreated ) _result.Dispose();

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
			_instance = new();

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
		_checkedElementsOnThisFrame = _elements.Count;
		if( _checkedElementsOnThisFrame == 0 ) return;

		_waitingJobResult = true;
		RequestSize( _checkedElementsOnThisFrame );

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

		// Has a limited buget of elements to check on each frame that is MAX_ELEMENTS_PER_BATCH
		// So each frame verify 

		for (int i = 0; i < _checkedElementsOnThisFrame; i++)
		{
			var e = _elements[i];
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
		if( _checkedElementsOnThisFrame == 0 ) return;

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

#if CODE_METRICS
		FrameTimingDebug.LogEnd( CODE_TAG );
#endif

		_waitingJobResult = false;
    }

	void CheckResultBackClean()
	{
		for (int i = 0; i < _checkedElementsOnThisFrame; i++)
			_elements[i].SetVisibility(_result[i] != byte.MinValue);
	}

	void CheckResultBackDirty()
	{
		var realElementsCheckedBack = 0;
		for (int i = 0; i < _checkedElementsOnThisFrame; i++)
		{
			if( _toRemove.Contains( i ) ) continue;
			realElementsCheckedBack++;
			_elements[i].SetVisibility(_result[i] != byte.MinValue);
		}

		RemoveDelayedElementsToRemove();
		var eCount = _elements.Count;

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