public interface IEditorAssetValidationProcess
{
#if UNITY_EDITOR
	bool EDITOR_ExecuteAssetValidationProcess();
#endif
}
