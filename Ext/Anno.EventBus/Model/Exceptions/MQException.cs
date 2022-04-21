
using System;
using System.Runtime.Serialization;

namespace Anno.EventBus.Model.Exceptions
{
    [Serializable]
    public class MQException : Exception
    {

        public MQException() : base("ExtendException Error.")
        {
            //记录日志
            Log.Log.Error(this);
        }

        public MQException(string errorMessage) : base(errorMessage) { }

        public MQException(string msgFormat, params object[] os) : base(string.Format(msgFormat, os)) { }

        protected MQException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public MQException(string message, Exception innerException) : base(message, innerException) { }

    }
}
