using System;

namespace CircuitMVVMBase
{
    public class Notification
    {
        public enum ErrorType { info, warning, error, exception }

        public ErrorType Error { get; internal set; }
        public string Message { get; internal set; }

        public Notification(string message, ErrorType errortype = ErrorType.info)
        {
            Message = message;
            Error = errortype;
        }

        public Notification(Exception ex)
        {
            Message = ex.Message;
            Error = ErrorType.exception;
        }

    }
}
