namespace YumeArisu.Core.Utility
{
    public static class Hash
    {
        public static ulong Fnv1a64(byte[] data)
        {
            const ulong offset = 14695981039346656037;
            const ulong prime = 1099511628211;

            ulong hash = offset;

            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= prime;
            }

            return hash;
        }

        public static ulong Fnv1a64(ReadOnlySpan<byte> data)
        {
            const ulong offset = 14695981039346656037;
            const ulong prime = 1099511628211;

            ulong hash = offset;

            foreach (byte b in data)
            {
                hash ^= b;
                hash *= prime;
            }

            return hash;
        }
    }
}