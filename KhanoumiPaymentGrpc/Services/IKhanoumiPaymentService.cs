using Grpc.Core;
using KhanoumiPyamentGrpc;
using System.Threading.Tasks;

namespace KhanoumiPaymentGrpc.Services
{
    public interface IKhanoumiPaymentService
    {
        Task<PaymentResponse> Pay(PaymentRequest request, ServerCallContext context);
    }
}
