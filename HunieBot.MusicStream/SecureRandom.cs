using System;
using System.Security.Cryptography;

namespace HunieBot.MusicStream
{

    /// <summary>
    ///     An implementation of <see cref="Random"/> that is thread-safe and cryptographically secure.
    /// </summary>
    /// <remarks>
    ///     Courtesy of Edward Shryock
    /// </remarks>
    public class SecureRandom : Random
    {
        public SecureRandom() { } //empty to keep from passing through to base
        private byte[] Pool = new byte[2048];
        private int CurrentIndex = 2048;
        unsafe private ulong SampleImpl()
        {
            lock (Pool.SyncRoot)
            {
                if ((CurrentIndex + 8) > Pool.Length - 1)
                {
                    RngProvider.GetBytes(Pool);
                    CurrentIndex = 0;
                }
                else
                {
                    CurrentIndex += 8;
                }
                fixed (byte* ptr = &Pool[CurrentIndex])
                {
                    return *((UInt64*)ptr);
                }
            }
        }
        unsafe private int NextImpl()
        {
            lock (Pool.SyncRoot)
            {
                if ((CurrentIndex + 4) > Pool.Length - 1)
                {
                    RngProvider.GetBytes(Pool);
                    CurrentIndex = 0;
                }
                else
                {
                    CurrentIndex += 4;
                }
                fixed (byte* ptr = &Pool[CurrentIndex])
                {
                    return *((int*)ptr);
                }
            }
        }
        private static RNGCryptoServiceProvider RngProvider = new RNGCryptoServiceProvider();
        protected override double Sample()
        {
            var n1 = (double)SampleImpl();
            const double n2 = (double)ulong.MaxValue;
            return n1 / n2;
        }
        public override int Next()
        {
            return NextImpl();
        }
        public override void NextBytes(byte[] bytes)
        {
            RngProvider.GetBytes(bytes);
        }
        public override int Next(int fromInclusive, int toExclusive)
        {
            var range = (long)toExclusive - (long)fromInclusive;
            return (int)(fromInclusive + (Sample() * range));
        }
    }

}