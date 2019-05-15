using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace VKPlayer.UiHelpers
{
    public static class UiInvoker
    {
        public static void Invoke(Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
        {
            if (Application.Current != null &&
                Application.Current.Dispatcher != null &&
                Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                if (Application.Current == null) //Danger! For unit test purposes only
                {
                    action();
                    return;
                }
                try
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Invoke(action, dispatcherPriority);
                    }
                }
                catch (TaskCanceledException)
                {
                    //Ignore
                }
            }
        }

        public static TResult Invoke<TResult>(Func<TResult> func)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                return func();
            }
            else
            {
                return Application.Current.Dispatcher.Invoke(func);
            }
        }

        public static void InvokeAsync(Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Application.Current.Dispatcher.InvokeAsync(action, dispatcherPriority);
            }
        }

        public static async void BeginInvoke(Action action, DispatcherPriority dispatcherPriority = DispatcherPriority.Normal)
        {
            if (Application.Current != null &&
                Application.Current.Dispatcher != null &&
                Application.Current.Dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                try
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null)
                    {
                        await Application.Current.Dispatcher.BeginInvoke(dispatcherPriority, action);
                    }
                }
                catch (TaskCanceledException)
                {
                    //Ignore
                }
            }
        }
    }
}
