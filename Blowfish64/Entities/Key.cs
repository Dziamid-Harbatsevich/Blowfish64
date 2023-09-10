namespace Blowfish64.Entities;

public class _Key
{
    internal const int MAX_CHAR_KEY_LENGTH = 28;

    public int KeyLength { get; set; }
    public string KeyValue;

    internal static int FilterKeyLength(int length)
    {
        return length > MAX_CHAR_KEY_LENGTH || length < 2 ? MAX_CHAR_KEY_LENGTH : length;
    }
}
