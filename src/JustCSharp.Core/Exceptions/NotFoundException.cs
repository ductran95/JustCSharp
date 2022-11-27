using System;
using System.Collections.Generic;
using JustCSharp.Data;
using JustCSharp.Data.Constants;
using JustCSharp.Data.Resources;

namespace JustCSharp.Core.Exceptions
{
    public class NotFoundException: DomainException
    {
        public string DataType { get; private set; }
        public object DataId { get; private set; }

        public NotFoundException(object dataId, string dataType = "",  string message = "", Exception? innerException = null) : base(message, innerException)
        {
            DataId = dataId;
            DataType = dataType;
        }

        public override HttpException WrapException()
        {
            var errors = new List<Error>
            {
                new Error(JustCSharpErrorCodes.NotFound, string.Format(JustCSharpErrorMessages.NotFound, DataType, DataId))
            };
            return new HttpException(404, errors, Message, this);
        }
    }
}