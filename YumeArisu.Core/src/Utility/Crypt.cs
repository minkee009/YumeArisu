namespace YumeArisu.Core.Utility;

public static class Crypt
{
    public static void Xor(byte[] data, int offset, int length, ulong seed)
    {
        ulong state = seed;

        for (int i = 0; i < length; i++)
        {
            state = state * 1099511628211UL + 1;

            data[offset + i] ^= (byte)(state >> 56);
        }
    }

    public static void Xor(Span<byte> data, ulong seed)
    {
        ulong state = seed;

        for (int i = 0; i < data.Length; i++)
        {
            state = state * 1099511628211UL + 1;
            data[i] ^= (byte)(state >> 56);
        }
    }
}