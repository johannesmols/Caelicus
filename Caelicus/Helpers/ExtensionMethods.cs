using System.Text.RegularExpressions;

namespace BlazorApp.Helpers
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Pretty prints variable names
        /// Credit: https://stackoverflow.com/a/5796793/2102106
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string SplitCamelCase(this string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
    }
}
