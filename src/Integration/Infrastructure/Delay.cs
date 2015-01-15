using System;

namespace Vertica.Integration.Infrastructure
{
    public struct Delay
    {
        private readonly int _seconds;

        private Delay(int seconds)
        {
            _seconds = seconds;
        }

        public static explicit operator Delay(uint seconds)
        {
            return new Delay((int)seconds);
        }

        public static implicit operator int(Delay delay)
        {
            return delay._seconds;
        }

        public static Delay operator *(Delay x, Delay y)
        {
            return new Delay(x._seconds*y._seconds);
        }

        public static Delay operator +(Delay x, Delay y)
        {
            return new Delay(x._seconds + y._seconds);
        }

        public static readonly Delay OneSecond = (Delay) 1;
        public static readonly Delay OneMinute = ((Delay) 60) * OneSecond;
        public static readonly Delay OneHour = ((Delay) 60) * OneMinute;

        public static Delay Custom(uint seconds)
        {
            return (Delay) seconds;
        }

        public override string ToString()
        {
            return String.Format("{0} second{1}", _seconds, _seconds == 1 ? String.Empty : "s");
        }
    }
}