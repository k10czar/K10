using UnityEngine;

namespace Automation
{
	public class OperationReferenceRunner : MonoBehaviour
	{
		[SerializeField] bool _log = true;
		[SerializeField] OperationObject _operation;

		public void Start()
		{
			_operation.ExecuteOn(this, _log);
		}
	}
}
