using System;
using System.Collections.Generic;
using K10.Localization;
using TMPro;
using UnityEngine;

public abstract class EnumDropdown<T> : MonoBehaviour, ILocalizable where T : struct, IComparable, IFormattable, IConvertible
{
	[SerializeField] TMP_Dropdown _dropdown;

	protected abstract IValueState<T> DataSource { get; }

	protected void Start()
	{
		Relocalize();
		_dropdown.onValueChanged.AddListener( OnDropdownChanged );
		DataSource.Synchronize( this.UntilLifeTime( (System.Action<T>)UpdateView ) );
	}

	void UpdateView( T val )
	{
		var id = GetId( val );
		if( _dropdown.options.Count > id && _dropdown.value != id ) _dropdown.value = id;
	}

#if UNITY_EDITOR
	void OldValidation()
	{
		this.RequestSibling( ref _dropdown );
	}
#endif

	public void Relocalize()
	{
		var initialVal = DataSource.Value;
		RenewOptions( _dropdown );
		_dropdown.value = GetId( initialVal );
	}

	public static void RenewOptions( TMP_Dropdown dropdown, string prefix = "" )
	{
		var opts = new List<TMP_Dropdown.OptionData>();
		foreach( T t in System.Enum.GetValues( typeof( T ) ) )
		{
			var opt = new TMP_Dropdown.OptionData();
			opt.text = RosettaStone.Interpret( prefix + t.ToString() );
			opts.Add( opt );
		}
		dropdown.ClearOptions();
		dropdown.AddOptions( opts );
	}

	public void OnDropdownChanged( int newId )
	{
		var newT = this[newId];
		DataSource.Setter( newT );
	}

	public int GetId( T val ) { int id = 0; foreach( T t in System.Enum.GetValues( typeof( T ) ) ) { if( t.Equals( val ) ) return id; id++; } return -1; }
	public T this[int id] { get { foreach( T t in System.Enum.GetValues( typeof( T ) ) ) { if( id <= 0 ) return t; id--; } return default( T ); } }
}