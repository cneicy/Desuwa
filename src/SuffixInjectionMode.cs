namespace Desuwa;

public enum SuffixInjectionMode
{
    ClipboardPaste,
    SimulatedTyping
}

internal static class SuffixInjectionModeExtensions
{
    public static string ToDisplayName(this SuffixInjectionMode mode)
    {
        return mode switch
        {
            SuffixInjectionMode.ClipboardPaste => "剪贴板粘贴",
            SuffixInjectionMode.SimulatedTyping => "逐字模拟输入",
            _ => "未知模式"
        };
    }

    public static string ToShortDisplayName(this SuffixInjectionMode mode)
    {
        return mode switch
        {
            SuffixInjectionMode.ClipboardPaste => "粘贴",
            SuffixInjectionMode.SimulatedTyping => "模拟",
            _ => "未知"
        };
    }
}