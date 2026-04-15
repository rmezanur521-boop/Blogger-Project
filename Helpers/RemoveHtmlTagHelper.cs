using System.Text.RegularExpressions;

namespace Blogger_Project.Helpers
{
    public static class RemoveHtmlTagHelper
    {
        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input,"<.*?>| $.*?;", string.Empty);
        }
    }
}
