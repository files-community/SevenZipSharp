using System;
using System.Collections.Generic;
using System.Text;

namespace SevenZip
{
    public class SevenZipOpenFailedException : SevenZipException
    {
        /// <summary>
        /// Exception default message which is displayed if no extra information is specified
        /// </summary>
        public static string DefaultMessage =
            $"Invalid archive: open/read error! Is it encrypted and a wrong password was provided?{Environment.NewLine}" +
            "If your archive is an exotic one, it is possible that SevenZipSharp has no signature for " +
            "its format and thus decided it is TAR by mistake.";

        public OperationResult Result { get; protected set; }

        public SevenZipOpenFailedException(OperationResult result) : this(DefaultMessage, result)
        {
        }

        public SevenZipOpenFailedException(string message, OperationResult result) : base(message)
        {
            Result = result;
        }

        public SevenZipOpenFailedException(OperationResult result, bool passwordRequested) : this(DefaultMessage, result, passwordRequested)
        {
        }

        public SevenZipOpenFailedException(string message, OperationResult result, bool passwordRequested) : base(message)
        {
            if (result == OperationResult.UnsupportedMethod)
            {
                if (passwordRequested)
                {
                    result = OperationResult.WrongPassword;
                }
            }
            Result = result;
        }
    }
}
