using Silky.Core.Exceptions;

namespace Silky.Transaction.Tcc.Exceptions
{
    public class TccTransactionException :SilkyException
    {
        public TccTransactionException(string message) : base(message, StatusCode.BusinessError)
        {

        }
    }
}