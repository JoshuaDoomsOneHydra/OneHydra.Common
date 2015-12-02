using System;
using System.Text;
using System.Text.RegularExpressions;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class StringCanonicalisationExtensions
    {
        const string IgnoreCategoriesPattern = @"[\p{Lt}\p{Lm}\p{Nl}\p{No}\p{Sc}\p{Sm}\p{So}\p{Sk}\p{Cc}\p{Cn}\p{Cf}\p{Co}\p{Cs}\p{Me}\p{Mn}\p{Mc}\p{Ps}\p{Pe}\p{Pf}\p{Pi}\p{Pd}\p{Pc}\p{Zl}\p{Zp}\p{Lo}\p{Po}-[&]]*";
        const string SpaceCharacterPattern = @"[.\-,+\n\r\f/_#]";
        const string IgnoreCharactersPattern = @"[`~!@#~$%^*()+/<>?;:""{}\\|}\[\]=]*";

        public static string RemoveMulitpleQuestionMarks(this string theString)
        {
            var result = Regex.Replace(theString, @"\?{2,}", "?");
            return result == "?" ? string.Empty : result;
        }

        public static string RemoveSpaceCharacters(this string theString)
        {
            return Regex.Replace(theString, SpaceCharacterPattern, " ");
        }

        public static string RemoveIgnoredCategories(this string theString)
        {
            return Regex.Replace(theString, IgnoreCategoriesPattern, "");
        }

        public static string RemoveIgnoredCharacters(this string theString)
        {
            return Regex.Replace(theString, IgnoreCharactersPattern, "");
        }

        public static string RemoveMulitpleWhitespaces(this string theString)
        {
            return Regex.Replace(theString, @"\s{2,}", " ");
        }

        public static string TrimToLowerCase(this string theString)
        {
            return theString.Trim().ToLower();
        }

        public static string NormaliseUnicode(this string theString)
        {
            return !theString.IsNormalized(NormalizationForm.FormKC) ? theString.Normalize(NormalizationForm.FormKC) : theString;
        }

        public static string ToKeywordCanonicalForm(this string theString)
        {
            string returnString;
            if (string.IsNullOrEmpty(theString))
            {
                returnString = theString;
            }
            else
            {
                returnString = theString.NormaliseUnicode();
                returnString = returnString.RemoveSpaceCharacters();
                returnString = returnString.RemoveIgnoredCharacters();
                returnString = returnString.RemoveIgnoredCategories();
                returnString = returnString.RemoveMulitpleWhitespaces();
                returnString = returnString.RemoveMulitpleQuestionMarks();
                returnString = returnString.TrimToLowerCase();
            }
            return returnString.GetLeftUpToMaxLength(150);
        }

        public static bool TryCanonicaliseUrl(this string url, out string canon, bool confirmHost = false)
        {
            var isValid = false;
            Uri outputUri;
            canon = string.Empty;
            if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out outputUri))
            {
                if (!confirmHost || !string.IsNullOrEmpty(outputUri.Host))
                {
                    canon = string.Format("{0}{1}{2}{3}", outputUri.Scheme, Uri.SchemeDelimiter, outputUri.Host, outputUri.PathAndQuery);
                    if (outputUri.Fragment != string.Empty)
                    {
                        canon = string.Format("{0}{1}", canon, outputUri.Fragment);
                    }
                    isValid = true;
                }
            }
            return isValid;
        }

        public static string TruncateLongUrl(this string canonicalisedUrl)
        {
            return (canonicalisedUrl.Length >= 2100 ? canonicalisedUrl.Substring(0, 2096) + "..." : canonicalisedUrl);
        }
    }
}
