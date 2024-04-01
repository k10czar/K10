using System.Collections.Generic;
using UnityEngine;

namespace Automation
{
	public class OperationRunner : KomposedDebugableMonoBehavior
	{
		[SerializeField] bool _log = true;
		[ExtendedDrawer, SerializeReference] IOperation _operation;

		public void Start()
		{
			_operation.ExecuteOn( this, _log );
		}
		
    
		protected override IEnumerable<object> GetKomposedDebugableObjects()
		{
			yield return _operation;
		}
	}
}
