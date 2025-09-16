using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public interface IOrchestratedUpdate
{
	void PreUpdate();
	void PostUpdate();
}

public interface IOrchestratedLateUpdate
{
	void PreLateUpdate();
	void PostLateUpdate();
}

public interface IOrchestratedFixedUpdate
{
	void PreFixedUpdate();
	void PostFixedUpdate();
}

public class CodeOrchestrator : MonoBehaviour
{
	[SerializeField] SubCodeOrchestrator _orchestrator = new();
#if UNITY_EDITOR
	[SerializeField] List<string> _debugList = new();
#endif

	static SubCodeOrchestrator _sceneOrchestrator;
	static CodeOrchestrator _eternalOrchestrator;

	public static bool FromSceneExists => _sceneOrchestrator != null;

	public static SubCodeOrchestrator Scene
	{
		get
		{
			if (_sceneOrchestrator == null)
			{
				var orc = new SubCodeOrchestrator();
				_sceneOrchestrator = orc;
				var eternal = Eternal;
				eternal.Add(orc);
				var sceneRelay = GameObjectEventsRelay.SceneObject;
				Debug.Log( $"Created Scene CodeOrchestrator".Colorfy( Colors.Violet ) );
				sceneRelay.OnDestroyEvent.Register(() =>
				{
					Debug.Log( $"Destroyed Scene CodeOrchestrator".Colorfy( Colors.Tomato ) );
					eternal.Remove(orc);
					if (_sceneOrchestrator == orc) _sceneOrchestrator = null;
					orc.Dispose();
				});
			}
			return _sceneOrchestrator;
		}
	}

	public static CodeOrchestrator Eternal
	{
		get
		{
			if (_eternalOrchestrator == null)
			{
				Debug.Log( $"Created Eternal CodeOrchestrator".Colorfy( Colors.Orange ) );
				GameObject obj = new GameObject($"EternalCodeOrchestration");
				_eternalOrchestrator = obj.AddComponent<CodeOrchestrator>();
				DontDestroyOnLoad(obj);
			}
			return _eternalOrchestrator;
		}
	}

	public void Add(object obj)
	{
		_orchestrator.Add(obj);
	}

	public void Remove(object obj)
	{
		_orchestrator.Remove(obj);
	}

	void OnDestroy()
	{
		_orchestrator.Dispose();
	}

	void Update()
	{
		_orchestrator.BakeUpdateList();
		_orchestrator.PreUpdate();
		_orchestrator.PostUpdate();
	}

	void LateUpdate()
	{
		_orchestrator.BakeLateUpdateList();
		_orchestrator.PreLateUpdate();
		_orchestrator.PostLateUpdate();
#if UNITY_EDITOR
		if (UnityEditor.Selection.activeGameObject == gameObject)
		{
			_debugList.Clear();
			_orchestrator.DebugList(_debugList);
		}
#endif
		}

	void FixedUpdate()
	{
		_orchestrator.BakeFixedUpdateList();
		_orchestrator.PreFixedUpdate();
		_orchestrator.PostFixedUpdate();
	}
}
