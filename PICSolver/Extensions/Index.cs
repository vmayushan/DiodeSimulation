using System.Runtime.CompilerServices;

namespace PICSolver.Extensions
{
    public static class Index
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Re(int i, int j, int n, int m)
        {
            return 2 * n * j + 2 * i;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Im(int i, int j, int n, int m)
        {
            return 2 * n * j + 2 * i + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int At(int i, int j, int n, int m)
        {
            return n * j + i;
        }
    }
}
