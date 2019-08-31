using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class HorizontalOrganizer : BaseOrganizer
{
	[SerializeField] TextAlignment _align = TextAlignment.Center;
	[SerializeField] float _spacing = .1f;
	[SerializeField] bool _countInactive = true;
	[SerializeField] List<Transform> _ignoreds;

	void Update() { UpdateOrganization(); }

	public override void UpdateOrganization() { K10.Utils.Unity.Algorithm.HorizontalOrganizer( transform, _spacing, _align, _countInactive, _ignoreds ); }
}