using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web.Http.ModelBinding;

namespace DeLoachAero.WebApi
{
    /// <summary>
    /// Exception class that holds RFC7807 problem detail information
    /// </summary>
    [Serializable]
    public class RFC7807Exception : Exception
    {
        /// <summary>
        /// Generic error message for model state validation errors when no
        /// other explicit error message is present in the model state dictionary.
        /// </summary>
        public const string ModelErrorOccurred = "An error has occurred.";

        /// <summary>
        /// Set the base URI for any calculated Type URIs; used
        /// when an RFC7807Exception is created based on a System.Exception
        /// so no problem detail is present, and the problem type URI must
        /// be calculated from the source exception type name.
        /// </summary>
        /// <example>
        /// <code>
        /// RFC7807Exception.TypeUriAuthority = "https://example.com/probs/";
        /// </code>
        /// </example>
        public static string TypeUriAuthority = "urn:";

        /// <summary>
        /// Holds the problem detail associated with this exception
        /// </summary>
        public RFC7807ProblemDetail ProblemDetail;

        /// <summary>
        /// Constructor needing only an HttpStatusCode. 
        /// </summary>
        /// <remarks>
        /// The instance returned meets the requirements but not necessarily the intent 
        /// of RFC7807 so you should really only use this version when you plan to flesh
        /// out the <see cref="ProblemDetail"/> in subsequent code, unless the
        /// http status code alone really is completely sufficient to describe the problem.
        /// </remarks>
        public RFC7807Exception(HttpStatusCode statusCode)
        {
            ProblemDetail = new RFC7807ProblemDetail(statusCode);
        }

        /// <summary>
        /// Constructor taking an RFC7807ProblemDetail instance.  
        /// </summary>
        /// <remarks>
        /// This is the best general constructor to use, as it gives you explicit control over
        /// the problem info directly in RFC7807 form.
        /// </remarks>
        public RFC7807Exception(RFC7807ProblemDetail problem) 
        {
            ProblemDetail = problem;
            if (problem.Status <= 0)
                problem.Status = (int) HttpStatusCode.InternalServerError;
        }

        /// <summary>
        /// Constructor for converting model validation errors into an RFC7807Exception
        /// </summary>
        /// <param name="modelState">ModelStateDictionary instance from model validation</param>
        /// <param name="typeUri">optional type URI for the problem detail type</param>
        /// <param name="instanceUri">optional instance URI for the ProblemDetail</param>
        /// <example>
        /// <code>
        /// if (!ModelState.IsValid)
        /// {
        ///     throw new RFC7807Exception(ModelState);
        /// }
        /// </code>
        /// </example>
        public RFC7807Exception(ModelStateDictionary modelState, Uri typeUri = null, Uri instanceUri = null)
        {
            ProblemDetail = new RFC7807ProblemDetail
            {
                Status = (int)HttpStatusCode.BadRequest,
                Type = typeUri ?? new Uri(TypeUriAuthority + typeof(System.ComponentModel.DataAnnotations.ValidationException).FullName),
                Instance = instanceUri
            };

            var extensions = new Dictionary<string, dynamic>();

            foreach (var keyValPair in modelState)
            {
                var errors = keyValPair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    var errorMessages = errors.Select(error =>
                    {
                        return String.IsNullOrEmpty(error.ErrorMessage) ? ModelErrorOccurred : error.ErrorMessage;
                    }).ToArray();

                    extensions.Add(keyValPair.Key, errorMessages);
                }
            }

            ProblemDetail.Extensions = new Dictionary<string, dynamic> { { "modelState", extensions} };
        }

        /// <summary>
        /// Constructor taking any Exception instance (including derived subclasses),
        /// and optionally an instance URI (perhaps the request URI) that will be 
        /// stored to the Instance field, and retrieves
        /// RFC7807 problem detail from the exception data.  
        /// </summary>
        /// <remarks>
        /// The resulting instance will have an http status code of either 500,
        /// or 501 for a NotImplementedException. 
        /// </remarks>
        public RFC7807Exception(Exception ex, Uri instanceUri = null)
        {
            ProblemDetail = new RFC7807ProblemDetail
            {
                Status = (int) HttpStatusCode.InternalServerError,
                Type = new Uri(TypeUriAuthority + ex.GetType().FullName),
                Detail = ex.Message,
                Instance = instanceUri
            };
            
            // 501 errors can be determined directly from the exception type
            if (ex is NotImplementedException)
                ProblemDetail.Status = (int)HttpStatusCode.NotImplemented;
        }

        /// <summary>
        /// Override the default Exception.Message to narrowly scope the
        /// message down to the Detail or Title values
        /// </summary>
        public override string Message => ProblemDetail.Detail ?? ProblemDetail.Title;

        /// <summary>
        /// Implicit cast operator to get an RFCProblemDetail from an RFC7807Exception
        /// </summary>
        public static implicit operator RFC7807ProblemDetail(RFC7807Exception ex)
        {
            return ex.ProblemDetail;
        }

        #region ISerializable required methods
        /// <summary>
        /// Required method for ISerializble, ensures ProblemDetail is serialized
        /// </summary>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("ProblemDetail", ProblemDetail);
            base.GetObjectData(info, context);
        }
        #endregion

    }
}