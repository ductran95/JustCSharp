using System;
using System.Collections.Generic;
using JustCSharp.Data;
using JustCSharp.Data.Constants;

namespace JustCSharp.Core.Exceptions
{
    public class NotFoundException: DomainException
    {
        public object DataId { get; private set; }

        public NotFoundException(object dataId, string message = "", Exception innerException = null) : base(message, innerException)
        {
            this.DataId = dataId;
        }

        public override HttpException WrapException()
        {
            var errors = new List<Error>
            {
                new Error(JustCSharpErrorCodes.NotFound, string.Format(JustCSharpErrorMessages.DataNotFound, DataId))
            };
            return new HttpException(404, errors, Message, this);
        }
    }
}