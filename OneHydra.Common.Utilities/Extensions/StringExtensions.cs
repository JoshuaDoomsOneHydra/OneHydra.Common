using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Text.RegularExpressions;
using NCrontab;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class StringExtensions
    {
        #region Fields

        private static readonly Regex EmailRegex = new Regex(@"^[\w-\.]{1,}\@([\da-zA-Z-]{1,}\.){1,}[\da-zA-Z-]{2,4}$");
        private static readonly Regex PoBoxRegex = new Regex(@"^p(ost)?[ .]*o(ffice)?[ .]*b(ox)?[ 0-9]*[^a-z ]*");
        private static readonly Regex StringOfSingleQuotes = new Regex("(')+");
        private static readonly NumberFormatInfo NumberFormat = CultureInfo.CurrentCulture.NumberFormat;
        private static readonly string CurrencyNumberPattern = string.Concat(@"(\d{1,", NumberFormat.CurrencyGroupSizes[0], "}", NumberFormat.CurrencyGroupSeparator, "?)*(", NumberFormat.CurrencyDecimalSeparator, @"\d*)?");
        private static readonly string[] PositivePatterns = {@"^{1}{0}$",@"^{0}{1}$", @"^{1} {0}$", @"^{0} {1}$"};
        private static readonly string[] NegativePatterns = {@"^\({0}{2}\)$", @"^{2}{1}{0}$", @"^{1}{2}{0}$", @"^{1}{0}{2}$", @"^\({0}{1}\)$", @"^{2}{0}{1}$", @"^{0}{2}{1}$", @"^{0}{1}{2}$", @"^{2}{0} {1}$", @"^{2}{1} {0}$", @"^{0} {1}{2}$", @"^{1} {0}{2}$", @"^{1} {2}{0}$", @"^{0}{2} {1}$",@"^\({1} {0}\)$", @"^\({0} {1}\)$" };
        private static readonly Regex CurrencyPositivePattern = new Regex(string.Format(PositivePatterns[NumberFormat.CurrencyPositivePattern], CurrencyNumberPattern, NumberFormat.CurrencySymbol));
        private static readonly Regex CurrencyNegativePattern = new Regex(string.Format(NegativePatterns[NumberFormat.CurrencyNegativePattern], CurrencyNumberPattern, NumberFormat.CurrencySymbol, NumberFormat.NegativeSign));

        #endregion Fields

        public static T ToEnum<T>(this string theString)
        {
            return (T)Enum.Parse(typeof(T), theString);
        }

        public static bool IsNumeric(this string theString)
        {
            var returnValue = false;
            long longParseTarget;
            decimal decParseTarget;
            if (long.TryParse(theString, out longParseTarget))
            {
                returnValue = true;
            }
            else if (decimal.TryParse(theString, out decParseTarget))
            {
                returnValue = true;
            }
            return returnValue;
        }

        public static bool IsEmail(this string theString)
        {
            var isEmail = false;
            if (!string.IsNullOrEmpty(theString))
            {
                isEmail = EmailRegex.IsMatch(theString);
            }
            return isEmail;
        }

        public static bool IsPoBox(this string theString)
        {
            return PoBoxRegex.IsMatch(theString);
        }

        public static bool IsInteger(this string theString)
        {
            int i;
            return int.TryParse(theString, out i);
        }

        public static bool IsPercent(this string theString)
        {
            return theString.TrimEnd().EndsWith(CultureInfo.CurrentCulture.NumberFormat.PercentSymbol);
        }

        public static bool IsCurrency(this string theString)
        {
            return CurrencyPositivePattern.IsMatch(theString) || CurrencyNegativePattern.IsMatch(theString);
        }

        public static bool IsValidCronExpression(this string theString)
        {
            return CrontabSchedule.TryParse(theString, success => true, failure=>false);
        }

        /// <summary>
        /// Parses the date and returns a nullable datetime.
        /// </summary>
        /// <param name="theString">The string.</param>
        /// <returns>Null if the parse wasn't successful otherwise, returns a nullable datetime.</returns>
        public static DateTime? ToDate(this string theString)
        {
            DateTime? returnValue = null;
            DateTime dateTimeParseTarget;
            if (DateTime.TryParse(theString, out dateTimeParseTarget))
            {
                returnValue = dateTimeParseTarget;
            }
            return returnValue;
        }

        /// <summary>
        /// Parses the date and returns a nullable datetime.
        /// </summary>
        /// <param name="theString">The string.</param>
        /// <returns>Null if the parse wasn't successful otherwise, returns a nullable datetime.</returns>
        public static int? ToInt(this string theString)
        {
            int? returnValue = null;
            int intParseTarget;
            if (int.TryParse(theString, out intParseTarget))
            {
                returnValue = intParseTarget;
            }
            return returnValue;
        }

        public static string RegexReplace(this string theString, string oldValueRegex, string newValue, RegexOptions options = RegexOptions.None)
        {
            var regex = new Regex(oldValueRegex, options);
            return regex.Replace(theString, newValue);
        }

        public static string ForSql(this string theString)
        {
            string returnValue;
            if (theString == null)
            {
                returnValue = "NULL";
            }
            else
            {
                returnValue = "'" + StringOfSingleQuotes.Replace(theString, "''") + "'";
            }
            return returnValue;
        }

        public static string GetLeftUpToMaxLength(this string theString, int maxLength)
        {
            return (string.IsNullOrEmpty(theString) || theString.Length <= maxLength ? theString : theString.Substring(0, maxLength));
        }

        public static void WriteToFile(this string theString, string directory, string fileName, bool localFile)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var filePathAndName = directory + (localFile ? "\\" : "//") + fileName;
            using (var stream = !File.Exists(filePathAndName) ? File.Create(filePathAndName) : File.Open(filePathAndName, FileMode.Create))
            {
                var wrtr = new StreamWriter(stream);
                wrtr.Write(theString);
                wrtr.Flush();
                wrtr.Close();
            }
        }

        public static Stream AsStream(this string theString)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(theString);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static T DeserializeSoap<T>(this string theString)
        {
            var soapFormatter = new SoapFormatter();
            return (T) soapFormatter.Deserialize(theString.AsStream());
        }

        public static byte[] GetUnicodeMd5HashBytes(this string value)
        {
            return new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(Encoding.Unicode.GetBytes(value));
        }
    }
}
