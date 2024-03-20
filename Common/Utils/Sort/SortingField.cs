using System;

public class SortingField<T>
{
    ComparisonSequence<T> _ordering;
    private readonly string[] _names;
    private readonly string[] _cachedNames;
    private readonly Comparison<T>[] _comps;
    private readonly int[] _sortComps;
    private Comparison<T> _cachedLambda;
    private bool _isDirty;

    // public ComparisonSequence<T> Ordering => _ordering;
    // public string[] Names => _names;
    public string[] CachedNames
    {
        get
        {
            UpdateIfDirty();
            return _cachedNames;
        }
    }
    // public int[] Sorting => _sortComps;
    // public Comparison<T>[] Comparisons => _comps;

    public  Comparison<T> ComparisonOrder => _cachedLambda ?? ( _cachedLambda = _ordering.Comparison );

    public SortingField( params ( Comparison<T> comparison,string name )[] definition )
    {
        _comps = new Comparison<T>[definition.Length];
        _names = new string[definition.Length];
        _sortComps = new int[definition.Length];
        _cachedNames = new string[definition.Length];
        for( int i = 0; i < definition.Length; i++ ) 
        {
            _comps[i] = definition[i].comparison;
            var name = definition[i].name;
            _names[i] = name;
        }
        _ordering = new ComparisonSequence<T>( _comps );
        UpdateCache();
    }

    public bool UpdateIfDirty()
    {
        if( !_isDirty ) return false;
        _isDirty = false;
        UpdateCache();
        return true;
    }

    public void UpdateCache()
    {
        for( int i = 0; i < _sortComps.Length; i++ ) _sortComps[_ordering.IndexOf( _comps[i] )] = i;
        for( int i = 0; i < _sortComps.Length; i++ ) 
        {
            var sortId = _sortComps[i];
            var reverted = _ordering.Reverted( i );
            _cachedNames[i] = $"{_names[sortId]}{(reverted?"↑":"↓")}";
        }
    }

    public void Toggle( int index )
    {
        _ordering.Toggle( index );
        _isDirty = true;
    }

    // public void LayoutAsButtonList()
    // {
    //     UpdateIfDirty();
    //     EditorGUILayout.BeginHorizontal( GUI.skin.box );
    //     GuiUtils.Label.ExactSizeLayout( "Sort:" );
    //     for( int i = 0; i < _sortComps.Length; i++ ) 
    //     {
    //         var sortId = _sortComps[i];
    //         var reverted = _ordering.Reverted( i );
    //         if( GuiUtils.Button.ExactSizeLayout( _cachedNames[i] ) ) 
    //         {
    //             _ordering.Toggle( _comps[sortId] );
    //             _isDirty = true;
    //         }
    //     }
    //     EditorGUILayout.EndHorizontal();
    // }

    // public void LayoutAsDropdown( params GUILayoutOption[] opts )
    // {
    //     UpdateIfDirty();
    //     EditorGUILayout.BeginHorizontal( GUI.skin.box, opts );
    //     GuiUtils.Label.ExactSizeLayout( "Sort:" );
    //     EditorGUI.BeginChangeCheck();
    //     var id = EditorGUILayout.Popup( 0, _cachedNames );
    //     var changed = EditorGUI.EndChangeCheck();
    //     if( GuiUtils.Button.ExactSizeLayout( "↕" ) || id != 0 || changed ) 
    //     {
    //         _ordering.Toggle( id );
    //         _isDirty = true;
    //     }
    //     EditorGUILayout.EndHorizontal();
    // }
}
