#if UNMANAGED

namespace SevenZip
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception class for ArchiveExtractCallback.
    /// </summary>
    [Serializable]
    public class ExtractionFailedException : SevenZipException
    {
        /// <summary>
        /// Exception default message which is displayed if no extra information is specified
        /// </summary>
        public const string DEFAULT_MESSAGE = "Could not extract files!";

        public OperationResult Result { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the ExtractionFailedException class
        /// </summary>
        public ExtractionFailedException(OperationResult result) : this(DEFAULT_MESSAGE, result)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ExtractionFailedException class
        /// </summary>
        /// <param name="message">Additional detailed message</param>
        public ExtractionFailedException(string message, OperationResult result) : base(message)
        {
            Result = result;
        }

        public ExtractionFailedException(OperationResult result, bool passwordRequested) : this(DEFAULT_MESSAGE, result, passwordRequested)
        {
        }

        public ExtractionFailedException(string message, OperationResult result, bool passwordRequested) : base(message)
        {
            if (result == OperationResult.DataError)
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

#endif
