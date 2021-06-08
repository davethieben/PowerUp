using System;
using System.Threading;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace PowerUp.Helpers
{
    public class TextBoxLoggerProvider : CallbackLoggerProvider
    {
        public TextBoxLoggerProvider(TextBox textBox, CancellationToken cancelToken) : base(AppendText(textBox, cancelToken))
        {
        }

        private static Action<string> AppendText(TextBox textBox, CancellationToken cancelToken)
        {
            return message =>
            {
                textBox.Dispatcher.Invoke(() =>
                {
                    textBox.AppendText(message);
                    textBox.ScrollToEnd();
                },
                System.Windows.Threading.DispatcherPriority.Normal,
                cancelToken);
            };
        }
    }

    public static class TextBoxLoggerExtensions
    {
        public static ILoggingBuilder AddTextBox(this ILoggingBuilder builder, TextBox textBox, CancellationToken cancelToken = default)
        {
            return builder.AddProvider(new TextBoxLoggerProvider(textBox, cancelToken));
        }
    }
}
