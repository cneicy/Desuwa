using System.Text;

namespace Desuwa;

internal sealed class SimulatedTypingSuffixInjector : ISuffixInjector
{
    public void Inject(string suffix)
    {
        if (string.IsNullOrEmpty(suffix))
        {
            return;
        }

        SendKeys.SendWait(EscapeForSendKeys(suffix));
    }

    private static string EscapeForSendKeys(string text)
    {
        var builder = new StringBuilder(text.Length);

        foreach (var character in text)
        {
            builder.Append(character switch
            {
                '+' => "{+}",
                '^' => "{^}",
                '%' => "{%}",
                '~' => "{~}",
                '(' => "{(}",
                ')' => "{)}",
                '[' => "{[}",
                ']' => "{]}",
                '{' => "{{}",
                '}' => "{}}",
                '\r' => string.Empty,
                '\n' => "{ENTER}",
                _ => character.ToString()
            });
        }

        return builder.ToString();
    }
}