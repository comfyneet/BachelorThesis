using System.Text;
using System.Text.RegularExpressions;

namespace RiceDoctor.Shared
{
    public static class StringExtensions
    {
        public static string ToUpperFirstLetter(this string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            var array = s.ToCharArray();
            array[0] = char.ToUpper(array[0]);
            return new string(array);
        }

        //public static string RemoveAccents(this string s)
        //{
        //    string[] signs =
        //    {
        //        "aAeEoOuUiIdDyY",
        //        "áàạảãâấầậẩẫăắằặẳẵ",
        //        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
        //        "éèẹẻẽêếềệểễ",
        //        "ÉÈẸẺẼÊẾỀỆỂỄ",
        //        "óòọỏõôốồộổỗơớờợởỡ",
        //        "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
        //        "úùụủũưứừựửữ",
        //        "ÚÙỤỦŨƯỨỪỰỬỮ",
        //        "íìịỉĩ",
        //        "ÍÌỊỈĨ",
        //        "đ",
        //        "Đ",
        //        "ýỳỵỷỹ",
        //        "ÝỲỴỶỸ"
        //    };

        //    for (var i = 1; i < signs.Length; i++)
        //        for (var j = 0; j < signs[i].Length; j++)
        //            s = s.Replace(signs[i][j], signs[0][i - 1]);

        //    return s;
        //}

        /// <summary>
        ///     Remove all non alphanumeric characters except dash, underscore and space characters (new lines are parsed as
        ///     spaces).
        ///     Then trim and remove duplicate spaces.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string RemoveNonWordChars(this string s)
        {
            var str = s.RemoveDuplicateSpaces();
            var builder = new StringBuilder();
            foreach (var c in str)
                if (char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == ' ')
                    builder.Append(c);

            return builder.ToString().Trim().RemoveDuplicateSpaces();
        }

        public static string RemoveDuplicateSpaces(this string s)
        {
            return Regex.Replace(s, @"\s+", " ");
        }
    }
}