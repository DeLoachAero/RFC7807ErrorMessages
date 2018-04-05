# RFC7807ErrorMessages
A set of classes and extensions to allow for easier creation and use of RFC 7807 problem messages from Web API 2.x.

## What it is
In the world of REST services, there has been historically no useful standard
for returning error or problem information to a caller until recently,
with the proposal of RFC 7807.
[Click here to read the text of RFC 7807](https://tools.ietf.org/html/rfc7807)

As this is the closest thing we have to an actual standard for errors
beyond simple HTTP status codes, you are more likely to create
inter-operable web services by using the RFC format for your error
detail information (which incorporates HTTP status codes as well, so
you get the best of both worlds.)

This library implements a number of classes , filters and extensions to allow
developers using ASP.NET Web API 2.x to easily create web response
messages in the RFC7807 format. 

## Problem Detail
The core data transfer object in the library is RFC7807ProblemDetail,
which holds the various fields from the RFC document and defines
the data contract to meet the requirements of the RFC.

Example creating a problem detail instance using the example
from the RFC:

```C#
    var detail = new RFC7807ProblemDetail
    {
       Type = new Uri("https://example.com/probs/out-of-credit"),
       Title = "You do not have enough credit.",
       Status = 403,
       Detail = "Your current balance is 30, but that costs 50.",
       Instance = new Uri("/account/12345/msgs/abc", UriKind.Relative)
    }
```

## Content Negotiation
The RFC specifies two specific content media types that should be
set on the http response depending on the Accept request header:

* application/problem+xml   for XML format, and
* application/problem+json  for JSON format.

The RFC7807Media static class has static methods to determine
the appropriate media type and Web API media type formatter 
based on the request.  In this implementation, XML is only 
returned when the caller explicitly asks for XML in the Accept
header; JSON is returned in all other cases.

## Supports both HttpResponseMessage and IHttpActionResult
The library is agnostic on your position in the debate over
what return types one should use for Web API action methods.
It supports all three styles of programming.

### If You Believe In Using Exceptions
Some programmers prefer to define their methods to return
object types, and explicitly throw Exceptions to indicate
error conditions.

In Web API, these programmers often use Exception Filters to
catch the exception and create a final HttpResponseMessage
to return to the caller.

The library includes an RFC7807ExceptionFilter class, and an
RFC7807Exception class as well, to support this style of coding.

Register the filter either globally in WebApiConfig.cs:

```C#
    config.Filters.Add(new RFC7807ExceptionFilterAttribute());
```
...or locally per-controller or per-action method:

```C#
    [RFC7807ExceptionFilter]
    public void MyActionMethod() ...
```

In your action methods, either throw any exception you want and 
it will extract the problem detail from the exception:

```C#
    throw new InvalidOperationException("You do not have enough credit.");
```

or even better, throw an RFC7807Exception instance instead, which allows 
you to explicitly control the problem detail:

```C#
    throw new RFC7807Exception(new RFC7807ProblemDetail
    {
        Type = new Uri("https://example.com/probs/out-of-credit"),
        Title = "You do not have enough credit.",
        Status = 403,
        Detail = "Your current balance is 30, but that costs 50.",
        Instance = new Uri("/account/12345/msgs/abc", UriKind.Relative)
    });
```

The exception filter will do content negotiation and output the 
appropriate JSON or XML that meets the RFC standard.

### If You Believe in Using HttpResponseMessages
Some programmers prefer to return HttpResponseMessages, which allow
for more complete control over the response including HTTP headers.

In order to get the proper content negotiation, you should use the
HttpRequestMessage extension included in the library:

```C#
    var response = Request.CreateRFC7807ProblemResponse(
	new RFC7807ProblemDetail
    {
        Type = new Uri("https://example.com/probs/out-of-credit"),
        Title = "You do not have enough credit.",
        Status = 403,
        Detail = "Your current balance is 30, but that costs 50.",
        Instance = new Uri("/account/12345/msgs/abc", UriKind.Relative)
    });
    // do any other processing of the HttpResponseMessage desired,
    // then...
    return response;
```

This is exactly analogous to the existing Request.CreateResponse()
and Request.CreateErrorResponse() methods included in Web API.

### If You Believe in Using IHttpActionResult
Some programmers prefer the flexibility of IHttpActionResult return
values.  Programmers using this style of coding in Web API usually prefer
to avoid throwing Exceptions, and instead return the appropriate
valid response or error response as an IHttpActionResult.

In order to get the proper content negotiation, you should use the
HttpRequestMessage extension included in the library:

```C#
    var response = Request.CreateRFC7807ProblemActionResult(
	new RFC7807ProblemDetail
    {
        Type = new Uri("https://example.com/probs/out-of-credit"),
        Title = "You do not have enough credit.",
        Status = 403,
        Detail = "Your current balance is 30, but that costs 50.",
        Instance = new Uri("/account/12345/msgs/abc", UriKind.Relative)
    });
    // do any other processing of the IHttpActionResult desired,
    // then...
    return response;
```

This is exactly analogous to the existing ApiController methods
like Ok(...), BadRequest(...), etc.


### If You Like Every Style
No problem. Use them all, you will have no issues registering the
Exception Filter and throwing exceptions in some cases, building
up HttpResponseMessages in others, and return IHttpActionResults 
here and there too.  Going overboard has maintenance consequences,
though, so you should consider this option carefully ;)

## Global Unhandled Exception Handler
The above scenarios revolve around the use of intentional exceptions and error 
results where the code wants explicit control over the error details
returned.  What about unintentional exceptions that aren't trapped? 
Returning non-RFC-compliant stock Web Api errors would confuse
callers expecting all errors to be in the RFC 7807 form.

The library includes a global exception handler that you can 
register to ensure unhandled exceptions still return results
in RFC 7807 format.  To register it, in your WebApiConfig.cs:

```C#
config.Services.Replace(typeof(IExceptionHandler), new RFC7807GlobalExceptionHandler());
```

Note the use of .Replace instead of .Add; there can only be one 
global exception handler registered in Web Api.

## Example Output
The library, and any of the examples above, produce the same sort of
output shown in the RFC.  The above examples return this in Web API:

	HTTP/1.1 403 Forbidden
	Cache-Control: no-cache
	Pragma: no-cache
	Content-Type: application/problem+json; charset=utf-8
	Expires: -1
	Server: Microsoft-IIS/10.0
	X-AspNet-Version: 4.0.30319
	X-Powered-By: ASP.NET
	Date: Mon, 26 Mar 2018 01:39:48 GMT
	Content-Length: 199

	{"type":"https://example.com/probs/out-of-credit","title":"You do not have enough credit.","status":403,"detail":"Your current balance is 30, but that costs 50.","instance":"/account/12345/msgs/abc"}

## License
MIT License, use as you see fit.


 
