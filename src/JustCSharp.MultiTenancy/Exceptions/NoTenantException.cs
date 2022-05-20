using System;
using System.Collections.Generic;
using JustCSharp.Core.Exceptions;
using JustCSharp.Data;
using JustCSharp.Data.Constants;
using JustCSharp.Data.Resources;

namespace JustCSharp.MultiTenancy.Exceptions
{
    public class NoTenantException: DomainException
    {
        public NoTenantException(string message = "", Exception innerException = null) : base(message, innerException)
        {
        }

        public override HttpException WrapException()
        {
            var errors = new List<Error>
            {
                new Error(JustCSharpErrorCodes.NoTenant, string.IsNullOrEmpty(Message) ? JustCSharpErrorMessages.NoTenant : Message)
            };
            return new HttpException(400, errors, Message, this);
        }
    }
}