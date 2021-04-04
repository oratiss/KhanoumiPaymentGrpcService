using Grpc.Core;
using KhanoumiPyamentGrpc;
using System.Threading.Tasks;

namespace KhanoumiPaymentGrpc.Services
{
    public interface IKhanoumiPaymentService
    {
        Task<TokenResponse> GetToken(TokenRequest request, ServerCallContext context);
    }
}
