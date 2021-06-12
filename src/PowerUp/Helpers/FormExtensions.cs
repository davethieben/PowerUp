using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Essentials;

namespace PowerUp
{
    public static class FormExtensions
    {

        public static Task DispatchAsync<T>(this T target, Action<T> action)
            where T : DispatcherObject
        {
            target.IsRequired();
            action.IsRequired();
            return target.Dispatcher.InvokeAsync(() => action.Invoke(target)).Task;
        }

        public static void Dispatch<T>(this T target, Action<T> action)
            where T : DispatcherObject
        {
            target.IsRequired();
            action.IsRequired();
            target.Dispatcher.Invoke(() => action.Invoke(target));
        }


    }
}
