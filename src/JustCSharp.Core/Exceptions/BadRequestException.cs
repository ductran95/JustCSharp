using System;
using System.Collections.Generic;
using System.Linq;
using JustCSharp.Data;
using JustCSharp.Data.Constants;

namespace JustCSharp.Core.Exceptions
{
    public class BadRequestException: DomainException
    {
        public IEnumerable<Error> Errors { get; private set; }

        public BadRequestException(string message = "", Exception innerException = null) : base(message, innerException)
        {
        }

        // public BadRequestException(IEnumerable<ValidationFailure> validationFailures, string message = "", Exception innerException = null) : this(message, innerException)
        // {
        //     Errors = validationFailures?.Select(x=>ToError(x));
        // }
        //
        // public BadRequestException(ValidationFailure validationFailure, string message = "", Exception innerException = null) : this(message, innerException)
        // {
        //     Errors = new List<Error>()
        //     {
        //         ToError(validationFailure)
        //     };
        // }

        public BadRequestException(IEnumerable<Error> errors, string message = "", Exception innerException = null) : this(message, innerException)
        {
            Errors = errors;
        }

        public BadRequestException(Error error, string message = "", Exception innerException = null) : this(message, innerException)
        {
            Errors = new List<Error>()
            {
                error
            };
        }

        public override HttpException WrapException()
        {
            if (Errors == null || !Errors.Any())
            {
                Errors = new List<Error>()
                {
                    new Error(JustCSharpErrorCodes.BadRequest, Message)
                };
            }

            return new HttpException(400, Errors, Message, this);
        }
    }
}