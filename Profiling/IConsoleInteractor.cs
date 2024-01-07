public interface IConsoleInteractor
{
    int ActionsCount { get; }
    bool HasBack { get; }
    bool IsDirty { get; }

    string Text { get; }

    void DoAction( int pretendedValue );
    void Back();

    string GetActionDebugName( int pretendedValue );
    string GetActionDebugNameColored( int pretendedValue );
}
