namespace Desuwa;

internal sealed class ClipboardSuffixInjector : ISuffixInjector
{
    public void Inject(string suffix)
    {
        if (string.IsNullOrEmpty(suffix))
        {
            return;
        }

        IDataObject? originalClipboardData = null;
        var hadClipboardData = false;

        try
        {
            try
            {
                originalClipboardData = Clipboard.GetDataObject();
                hadClipboardData = originalClipboardData != null;
            }
            catch
            {
                // 被占用失败跳过
            }

            Clipboard.SetText(suffix, TextDataFormat.UnicodeText);
            SendKeys.SendWait("^v");
            Thread.Sleep(20);
        }
        finally
        {
            try
            {
                if (hadClipboardData && originalClipboardData != null)
                {
                    Clipboard.SetDataObject(originalClipboardData, true);
                }
                else
                {
                    Clipboard.Clear();
                }
            }
            catch
            {
                //ignore
            }
        }
    }
}