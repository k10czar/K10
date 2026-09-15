using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IWeighted
{
	float Weight { get; }
}

public static class WeightedExtensions
{
	public static float TotalWeight<T>(this IEnumerable<T> list) where T : IWeighted
	{
		float w = 0;
		foreach( var item in list ) w += item.Weight;
		return w;
	}
	
	public static int RandomID<T>(this IReadOnlyList<T> list) where T : IWeighted => list.RandomID( K10Random.Value );
	public static int RandomID<T>(this IReadOnlyList<T> list, float rng01) where T : IWeighted
	{
		var total = TotalWeight(list);
		var rnd = rng01 * total;

		var count = list.Count;
		for (int i = 0; i < count; i++)
		{
			var element = list[i];
			rnd -= element.Weight;
			if (rnd <= 0) return i;
		}

		return -1;
	}

	public static T Random<T>(this IReadOnlyList<T> list) where T : IWeighted => list.Random( K10Random.Value );
	public static T Random<T>(this IReadOnlyList<T> list, float rng01) where T : IWeighted
	{
		var id = RandomID(list,rng01);
		if (id < 0) return default;
		return list[id];
	}
}

[System.Serializable]
public abstract class Weighted : IWeighted
{
	[SerializeField] protected float _weight = 1;
	public float Weight { get { return _weight; } }

    public void SetWeight(float weight) { _weight = weight; }

	public static float Total( IList list ) 
	{
		float total = 0;

		for( int i = 0; i < list.Count; i++ ) total += ( (Weighted)list[ i ] )._weight;
		return total;
	}

	public static string ToString( IList list )
	{
		var total = TotalWeight( list );
		var sb = new System.Text.StringBuilder();

		sb.Append( "{" );

		for( int i = 0; i < list.Count; i++ )
		{
			var element = (Weighted)list[i];
			
			sb.Append( "[" + element.ToString() + "," + ( element._weight * 100 / total ) + "%]" );
			if( i + 1 < list.Count ) sb.Append( ", " );
		}
		sb.Append( "}" );

		return sb.ToString();
	}

	public static int RandomID( IList list )
	{
		var total = TotalWeight( list );
		var rnd = K10Random.Value * total;

		for( int i = 0; i < list.Count; i++ )
		{
			var element = (Weighted)list[i];
			rnd -= element._weight;
			if( rnd <= 0 )
				return i;
		}

		return -1;
	}

	public static float TotalWeight( IList list )
	{
		float w = 0;
		for( int i = 0; i < list.Count; i++ )
		{
			var element = (Weighted)list[i];
			w += element._weight;
		}
		return w;
	}

	public static readonly AscendingComparer Ascending = new AscendingComparer();
	public static readonly DescendingComparer Descending = new DescendingComparer();

	public sealed class AscendingComparer : IComparer<Weighted> 
	{
		public int Compare( Weighted x, Weighted y ) 
		{
			return x.Weight.CompareTo( y.Weight );
		}
	}

	public sealed class DescendingComparer : IComparer<Weighted> 
	{
		public int Compare( Weighted x, Weighted y ) 
		{
			return y.Weight.CompareTo( x.Weight );
		}
	}
}

[System.Serializable]
public class Weighted<T> : Weighted
{
	[SerializeField] protected T _t;

	public T Value { get { return _t; } }

	public static T Random( IList list ) 
	{
		var id = RandomID( list );

		if( id == -1 ) return default( T );
		return ((Weighted<T>)list[id])._t;
	}

	public Weighted() { _t = default(T); }
	public Weighted( T t ) { _t = t; }
	public Weighted( T t, float weight ) { _t = t; _weight = weight; }

	public override string ToString() { return _t.ToString(); }
}

[System.Serializable]
public class WeightedColor : Weighted<Color>
{
	public static Color Random( List<WeightedColor> list )
	{
		var id = RandomID( list );
		
		if( id == -1 ) return Color.white;
		return list[id]._t;
	}
	
	public static int RandomID( List<WeightedColor> list )
	{
		var total = TotalWeight( list );
		var rnd = K10Random.Value * total;
		
		for( int i = 0; i < list.Count; i++ )
		{
			var element = list[i];
			rnd -= element._weight;
			if( rnd <= 0 )
				return i;
		}
		
		return -1;
	}
	
	static float TotalWeight( List<WeightedColor> list )
	{
		float w = 0;
		for( int i = 0; i < list.Count; i++ )
		{
			var element = list[i];
			w += element._weight;
		}
		return w;
	}
}

[System.Serializable] public class WeightedGameObject : Weighted<GameObject>
{
	public static GameObject Random( List<WeightedGameObject> list )
	{
		var id = RandomID( list );
		
		if( id == -1 ) return null;//new GameObject( "WeightedGameObject" );
		return list[id]._t;
	}
	
	public static int RandomID( List<WeightedGameObject> list )
	{
		var total = TotalWeight( list );
		var rnd = K10Random.Value * total;
		
		for( int i = 0; i < list.Count; i++ )
		{
			var element = list[i];
			rnd -= element._weight;
			if( rnd <= 0 )
				return i;
		}
		
		return -1;
	}
	
	static float TotalWeight( List<WeightedGameObject> list )
	{
		float w = 0;
		for( int i = 0; i < list.Count; i++ )
		{
			var element = list[i];
			w += element._weight;
		}
		return w;
	}
}

