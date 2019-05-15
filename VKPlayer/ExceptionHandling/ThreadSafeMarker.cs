using System.Threading;

namespace VKPlayer.ExceptionHandling
{
    public sealed class ThreadSafeMarker
    {
        private int _value;

        public bool TrySet()
        {
            return Interlocked.CompareExchange(ref _value, 1, 0) == 0;
        }

        public bool IsSet => _value == 1;

        public void Reset()
        {
            Interlocked.Exchange(ref _value, 0);
        }
    }
}
