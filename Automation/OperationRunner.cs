using UnityEngine;

namespace Automation
{
	public class OperationRunner : MonoBehaviour
	{
		[SerializeField] bool _log = true;
		[ExtendedDrawer, SerializeReference] IOperation _operation;

		public void Start()
		{
			_operation.ExecuteOn( this, _log );
		}
	}
}
