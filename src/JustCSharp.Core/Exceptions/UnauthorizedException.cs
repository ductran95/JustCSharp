using System;
using System.Collections.Generic;
using JustCSharp.Data;
using JustCSharp.Data.Constants;

namespace JustCSharp.Core.Exceptions
{
    public class UnauthorizedException: DomainException
    {
        public UnauthorizedException(string message = "", Exception innerException = null) : base(message, innerException)
        {
        }

        public override HttpException WrapException()
        {
            var errors = new List<Error>
            {
                new Error(JustCSharpErrorCodes.Unauthorized, string.IsNullOrEmpty(Message) ? JustCSharpErrorMessages.Unauthorized : Message)
            };
            return new HttpException(403, errors, Message, this);
        }
    }
}