using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Framework
{
    public class MethodCallResult
    {
        #region -> Type Interface <-

        public static MethodCallResult CreateFail(string FailReason)
        {
            return new MethodCallResult(Fail, FailReason);
        }
        public static MethodCallResult CreateException(Exception ex)
        {
            return new MethodCallResult(Exception, ex);
        }

        public static readonly MethodCallResult Exception = new MethodCallResult(2);
        public static readonly MethodCallResult Success = new MethodCallResult(1);
        public static readonly MethodCallResult Fail = new MethodCallResult(0);

        public static implicit operator bool(MethodCallResult Source)
        {
            return Source.Value == 1;
        }
        public static bool operator ==(MethodCallResult CallResult1, MethodCallResult CallResult2)
        {
            // If both are null, or both are same instance, return true.
            if (Object.ReferenceEquals(CallResult1, CallResult2)) return true;

            // If one is null, but not both, return false.
            if ((object)CallResult1 == null || (object)CallResult2 == null) return false;
            return CallResult1.Value == CallResult2.Value;
            //return (CallResult1 != null && CallResult2 != null && CallResult1.Value == CallResult2.Value) || (CallResult1 == null && CallResult2 == null);
        }
        public static bool operator !=(MethodCallResult CallResult1, MethodCallResult CallResult2)
        {
            return !(CallResult1 == CallResult2);
        }

        #endregion

        #region -> Nested Fields <-

        private readonly short _value;

        protected short Value
        {
            get { return _value; }
        }

        #endregion

        #region -> Interface <-

        public readonly object Parameter = null;

        public override string ToString()
        {
            /*StringBuilder builder = new StringBuilder("Result: " + Value.ToString());
            if (Parameter != null)
            {
                builder.AppendLine();
                builder.AppendLine(Parameter.ToString());
            }
            return builder.ToString();*/
            if (Parameter != null)
            {
                return Parameter.ToString();
            }
            else
            {
                if (!this) return "Unknown error.";
                else return "Success";
            }
        }
        public override bool Equals(object obj)
        {
            MethodCallResult other = obj as MethodCallResult;
            return other != null && other.Value == this.Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        #endregion

        #region -> Consructors <-

        protected MethodCallResult(MethodCallResult Value, object Parameter)
        {
            _value = Value.Value;
            this.Parameter = Parameter;
        }
        protected MethodCallResult(short Value)
        {
            _value = Value;
        }

        #endregion
    }
}
