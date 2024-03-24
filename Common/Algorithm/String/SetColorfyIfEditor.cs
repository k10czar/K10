using UnityEngine;

public class SetColorfyIfEditor : IStringProcessor
{
    [SerializeField] Color _color = Color.white;
    
    public string Execute(string msg) => msg.Colorfy( _color );
}

public class StringPassThrough : IStringProcessor
{
    public string Execute(string msg) => msg;
}


// public class LogValue<T> : ITriggerValue<T>
// {
//     [SerializeField] string _messageSuffix = "New Value: ";
//     [SerializeField] ELogType _type = ELogType.Basic;
//     [SerializeField] IStringProcessor _suffixPostProcessing;
//     [SerializeField] IStringProcessor _valuePostProcessing;

//     public bool IsValid => true;
    
//     public void Trigger( T value )
//     {
//         var suffix = _suffixPostProcessing.TryExecute( _messageSuffix );
//         var valueStr = _valuePostProcessing.TryExecute(value.ToString());
//         var msg = $"{suffix}{valueStr}";
//         msg.Log( _type );
//     }
// }