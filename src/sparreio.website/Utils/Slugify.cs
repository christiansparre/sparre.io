using System.Text.RegularExpressions;

namespace sparreio.website.Utils
{
    public class Slugify
    {
        public static string GenerateSlug(string phrase, int length = 40)
        {
            string str = RemoveAccent(phrase).ToLower();
            // invalid chars           
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            // convert multiple spaces into one space   
            str = Regex.Replace(str, @"\s+", " ").Trim();
            // cut and trim 
            str = str.Substring(0, str.Length <= length ? str.Length : length).Trim();
            str = Regex.Replace(str, @"\s", "-"); // hyphens   
            return str;
        }

        public static string RemoveAccent(string txt)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(txt);
            return System.Text.Encoding.ASCII.GetString(bytes);
        }
    }
}