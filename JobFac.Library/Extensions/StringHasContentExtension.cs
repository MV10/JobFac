namespace System
{
    public static class StringHasContentExtension
    {
        public static bool HasContent(this string s)
            => !string.IsNullOrWhiteSpace(s);
    }
}
