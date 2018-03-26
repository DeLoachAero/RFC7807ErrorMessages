using System.Web.Http.Filters;


namespace DeLoachAero.WebApi
{
    /// <summary>
    /// Web API 2.x exception filter to output valid RFC7807 data to the caller for an exception
    /// </summary>
    public class RFC7807ExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            var ex = context.Exception as RFC7807Exception;
            if (ex == null)
                ex = new RFC7807Exception(context.Exception, context.Request.RequestUri);

            context.Response = context.Request.CreateRFC7807ProblemResponse(ex.ProblemDetail);
        }
    }
}