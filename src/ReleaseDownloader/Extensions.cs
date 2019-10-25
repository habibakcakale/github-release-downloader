namespace ReleaseDownloader
{
    using System.Linq;

    public static class Extensions
    {
        public static void Deconstruct<T>(this T[] array, out T first, out T[] rest)
        {
            first = array.Length > 0 ? array[0] : default(T);
            rest = array.Skip(1).ToArray();
        }

        public static void Deconstruct<T>(this T[] array, out T first, out T second, out T[] rest)
            => (first, (second, rest)) = array;

        public static void Deconstruct<T>(this T[] array, out T first, out T second, out T third, out T[] rest)
            => (first, second, (third, rest)) = array;
    }
}