namespace CSharpLatest.CSharp11Features;

internal class UTF8StringLiterals
{
    public void Method()
    {
        ReadOnlySpan<byte> AuthWithTrailingSpace = new byte[] { 0x41, 0x55, 0x54, 0x48, 0x20 };
        ReadOnlySpan<byte> AuthStringLiteral = "AUTH "u8;
        byte[] AuthStringLiteralAsByteArray = "AUTH "u8.ToArray();
    }
}
