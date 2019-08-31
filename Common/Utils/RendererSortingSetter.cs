using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RendererSortingSetter : AttachedComponent<Renderer>
{
    public string _sortingLayer = "";
    string _lastSettedSortingLayer = "";

    public int _sortingOrder = 0;
    int _lastSettedSortingOrder = 0;    

    void Awake()
    {
        Update();
    }

	void Update()
    {
        if( _lastSettedSortingOrder != _sortingOrder )
        {
            Attached.sortingOrder = _sortingOrder;
            _lastSettedSortingOrder = _sortingOrder;
        }

        if( _lastSettedSortingLayer != _sortingLayer )
        {
            Attached.sortingLayerName = _sortingLayer;
            _lastSettedSortingLayer = _sortingLayer;
        }
	}
}
