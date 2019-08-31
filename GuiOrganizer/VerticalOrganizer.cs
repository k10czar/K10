using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseOrganizer : MonoBehaviour { public abstract void UpdateOrganization(); }

[ExecuteInEditMode]
public class VerticalOrganizer : BaseOrganizer 
{
	[SerializeField] float _spacing = .1f;
	[SerializeField] EVerticalAlign _align = EVerticalAlign.Center;
	[SerializeField] bool _countInactive = true;
	[SerializeField] List<Transform> _ignoreds;

	void Update() { UpdateOrganization(); }

	public override void UpdateOrganization() { K10.Utils.Unity.Algorithm.VerticalOrganizer( transform, _spacing * transform.localScale.y, _align, _countInactive, _ignoreds ); }
}