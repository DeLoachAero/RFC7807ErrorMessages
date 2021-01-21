using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace DeLoachAero.WebApi
{
    /// <summary>
    /// Static utility methods to handle RFC7807 media types and media type formatters
    /// </summary>
    public static class RFC7807Media
    {
        /// <summary>
        /// RFC7807 media type for problem detail in XML
        /// </summary>
        public const string ProblemXmlMediaType = "application/problem+xml";

        /// <summary>
        /// RFC7807 media type for problem detail in JSON
        /// </summary>
        public const string ProblemJsonMediaType = "application/problem+json";

        /// <summary>
        /// Get the proper response media type for the incoming request media type
        /// </summary>
        public static string GetRFC7807ContentTypeForRequest(HttpRequestMessage request)
        {
            var acceptsAny = request.Headers.Accept.Any(x =>
                x.MediaType.Equals("*/*") ||
                x.MediaType.Equals("application/*"));

            var isxml = request.Headers.Accept.Any(x =>
                x.MediaType.Equals("application/xml") ||
                x.MediaType.Equals("text/xml"));

            if (acceptsAny)
            {
                return ProblemJsonMediaType;
            }

            if (isxml)
            {
                return ProblemXmlMediaType;
            }

            return ProblemJsonMediaType;
        }

        /// <summary>
        /// return the formatter instance from the HttpConfiguration to use for the 
        /// given RFC7807 media content type string
        /// </summary>
        public static MediaTypeFormatter GetMediaTypeFormatterForResponseType(
            HttpConfiguration configuration, 
            string respMediaType)
        {
            if (respMediaType.Equals(ProblemXmlMediaType) && configuration.Formatters.XmlFormatter != null)
            {
                return configuration.Formatters.XmlFormatter;
            }
            else
                return configuration.Formatters.JsonFormatter;
        }

    }
}
