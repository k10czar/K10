#if CODE_METRICS
#define LOG_ALL_METRICS
#define LOG_REPORT_ON_SUSPEND
#define LOG_REPORT_PARTIAL
#endif
#if LOG_ALL_METRICS || LOG_REPORT_ON_SUSPEND || LOG_REPORT_PARTIAL
#define ENABLED
#endif
public interface IDebugName
{
	string DebugName { get; }
}
