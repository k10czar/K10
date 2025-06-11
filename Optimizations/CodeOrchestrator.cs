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
	SubCodeOrchestrator _orchestrator = new();

	static SubCodeOrchestrator _sceneOrchestrator;
	static CodeOrchestrator _eternalOrchestrator;

	public static bool SceneExists => _sceneOrchestrator != null;

	public static SubCodeOrchestrator Scene
	{
		get
		{
			if (_sceneOrchestrator == null)
			{
				var sceneRelay = GameObjectEventsRelay.SceneObject;
				var orc = new SubCodeOrchestrator();
				_sceneOrchestrator = orc;
				var eternal = Eternal;
				eternal.Add(orc);
				sceneRelay.OnDestroyEvent.Register(() =>
				{
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
	}

	void FixedUpdate()
	{
		_orchestrator.BakeFixedUpdateList();
		_orchestrator.PreFixedUpdate();
		_orchestrator.PostFixedUpdate();
	}
}
