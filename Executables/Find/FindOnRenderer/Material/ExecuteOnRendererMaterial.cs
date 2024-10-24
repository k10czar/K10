using UnityEngine;
using System.Collections.Generic;

public interface IFinder<T, K>
{
    IEnumerator<K> Find(T t);
}

public class FindMaterialById : IFinder<Renderer,Material>
{
    [SerializeField] int materialId = 0;

    public IEnumerator<Material> Find(Renderer r)
    {
        if( materialId >= 0 && materialId < r.materials.Length )
            yield return r.materials[materialId];
    }
}

public abstract class SelectWithPredicate<T,K> : IFinder<T,K>
{
    [SerializeField] int maxCount = -1;
    
    System.Func<K,bool> predicateInstance;
    public System.Func<K, bool> PredicateInstance
    {
        get
        {
            if( predicateInstance == null ) predicateInstance = Predicate;
            return predicateInstance;
        }
    }

    public abstract IEnumerable<K> GetEnumeration( T t );

    protected abstract bool Predicate( K k );
    public IEnumerator<K> Find( T t ) 
    {
        var predicate = PredicateInstance;
        var count = 0;
        if( maxCount != 0 )
        {
            foreach( var m in GetEnumeration( t ) )
            {
                if( predicate( m ) ) 
                {
                    count++;
                    yield return m;
                    if( count == maxCount ) break;
                }
            }
        }
    }
}

public abstract class RendererMaterialSelection : SelectWithPredicate<Renderer,Material>
{
    public override IEnumerable<Material> GetEnumeration( Renderer r ) => r.materials;
}

public class SelectMaterialByName : RendererMaterialSelection
{
    [SerializeField] string name = "Standard";
    protected override bool Predicate(Material m) => m.name == name;
}

public class SelectMaterialByShader : RendererMaterialSelection
{
    [SerializeField] Shader shader;
    protected override bool Predicate(Material m) => m.shader == shader;
}

public class ExecuteOnRendererMaterial : ITriggerable<Renderer>
{
    [SerializeReference,ExtendedDrawer] IFinder<Renderer,Material> finder;
    [SerializeReference,ExtendedDrawer] ITriggerable<Material>[] execute;

    public void Trigger( Renderer r )
    {
        if( r == null ) return;
        var materials = finder.Find( r );
        if( materials == null ) return;
        while( materials.Current != null )
        {
            execute.TriggerAll( materials.Current );
            materials.MoveNext();
        }
    }
}
