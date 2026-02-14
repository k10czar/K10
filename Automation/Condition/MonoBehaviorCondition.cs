using UnityEngine;

namespace K10.Conditions
{
    public abstract class MonoBehaviorCondition<T> : ICondition where T : MonoBehaviour
    {        
        public virtual bool NotFoundCheck => true;
        public virtual FindObjectsInactive SearchParam => FindObjectsInactive.Include;

        T mb;

        public bool Check()
        {
            if( mb == null ) mb = Object.FindAnyObjectByType<T>( SearchParam );
            if( mb == null ) return NotFoundCheck;
            return CheckOn( mb );
        }

        public abstract bool CheckOn( T t );

        public override string ToString() => $"{GetType()}:{(Check()?"True":"False")}";
    }
}