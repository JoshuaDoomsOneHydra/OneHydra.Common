using System;
using System.Text;
using System.Web.Services.Protocols;

namespace OneHydra.Common.Utilities.Extensions
{
    public static class ExceptionExtensions
    {
        public static string FullExceptionString(this Exception ex)
        {
            var exceptionStringBuilder = new StringBuilder();
            while (ex != null)
            {
                if (ex is SoapException)
                {
                    var detail = ex.CastTo<SoapException>().Detail;
                    if (detail != null)
                    {
                        var detailString = detail.InnerText;
                        if (!string.IsNullOrEmpty(detailString))
                        {
                            exceptionStringBuilder.AppendLine(detailString);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    exceptionStringBuilder.AppendLine(ex.Message);
                }
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    exceptionStringBuilder.AppendLine(ex.StackTrace);
                }

                ex = ex.InnerException;
            }
            return exceptionStringBuilder.ToString();
        }
    }
}
