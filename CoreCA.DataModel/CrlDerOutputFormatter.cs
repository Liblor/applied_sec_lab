using Microsoft.AspNetCore.Mvc.Formatters;
using Org.BouncyCastle.X509;
using System.Threading.Tasks;

namespace CoreCA.DataModel
{
    public class CrlDerOutputFormatter : OutputFormatter
    {
        public CrlDerOutputFormatter()
        {
            SupportedMediaTypes.Add(Constants.CrlMimeType);
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context.Object is X509Crl crl)
            {
                await context.HttpContext.Response.Body.WriteAsync(crl.GetEncoded());
            } else if (context.Object is byte[] crlBytes)
            {
                await context.HttpContext.Response.Body.WriteAsync(crlBytes);
            }
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return context.ObjectType == typeof(X509Crl) || context.ObjectType == typeof(byte[]);
        }
    }
}
