using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;

namespace DeLoachAero.WebApi
{
    /// <summary>
    /// HttpRequestMessage extensions for creating RFC7807 problem detail responses
    /// </summary>
    /// <remarks>
    /// Note that the implicit conversion operator on RFC7807Exception means any method
    /// in this class that takes an RFC7807ProblemDetail will also accept an
    /// RFC7807Exception instance as well.
    /// </remarks>
    public static class RFC7807RequestMessageExtensions
    {
        /// <summary>
        /// Create an HttpResponseMessage given an RFC7807ProblemDetail instance
        /// </summary>
        public static HttpResponseMessage CreateRFC7807ProblemResponse(this HttpRequestMessage request, 
            RFC7807ProblemDetail detail)
        {
            var media = RFC7807Media.GetRFC7807ContentTypeForRequest(request);
            var formatter = RFC7807Media.GetMediaTypeFormatterForResponseType(
                request.GetConfiguration(), media);

            var exception = new RFC7807Exception(detail);
            return request.CreateResponse((HttpStatusCode) exception.ProblemDetail.Status, detail, formatter, media);
        }

        /// <summary>
        /// Create an IHttpActionResult given an RFC7807ProblemDetail instance
        /// </summary>
        public static IHttpActionResult CreateRFC7807ProblemActionResult(this HttpRequestMessage request,
            RFC7807ProblemDetail detail)
        {
            var media = RFC7807Media.GetRFC7807ContentTypeForRequest(request);
            var formatter = RFC7807Media.GetMediaTypeFormatterForResponseType(
                request.GetConfiguration(), media);

            var exception = new RFC7807Exception(detail);

            return new FormattedContentResult<RFC7807ProblemDetail>(
                (HttpStatusCode)exception.ProblemDetail.Status, 
                detail, formatter, new MediaTypeHeaderValue(media), request);
        }

    }
}