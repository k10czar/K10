

public interface ICallOnInspectEdit
{
#if UNITY_EDITOR
		void EDITOR_OnInspectEdit();
#endif
}