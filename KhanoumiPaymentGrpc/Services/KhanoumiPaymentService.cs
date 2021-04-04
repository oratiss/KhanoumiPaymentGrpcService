using Grpc.Core;
using KhanoumiPaymentGrpc.Models.SamanBank;
using KhanoumiPyamentGrpc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static KhanoumiPaymentGrpc.Models.Constants.KhanoumiPaymentServiceConstants.ConstantsProvider;
using static KhanoumiPaymentGrpc.Models.Constants.SamanServiceConstants.SamanServiceConstantProvider;
using static KhanoumiPaymentGrpc.Models.Enumeration;


namespace KhanoumiPaymentGrpc.Services
{
    public class KhanoumiPaymentService : KhanoumiPayment.KhanoumiPaymentBase, IKhanoumiPaymentService
    {
        private readonly ILogger<KhanoumiPaymentService> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public KhanoumiPaymentService(ILogger<KhanoumiPaymentService> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public override Task<TokenResponse> GetToken(TokenRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                return Task.FromResult(new TokenResponse
                {
                    Status = 400,
                    Message = "Bad Request. request is null."
                });
            }

            if (!AuthenticationPairs.ContainsKey(request.MerchandId)
                || !AuthenticationPairs.ContainsValue(request.MerchantPassword)
                || AuthenticationPairs[request.MerchandId] != GrpcPassword)
            {
                return Task.FromResult(new TokenResponse
                {
                    Status = 401,
                    Message = "Authorization Failed."
                });
            }

            if (request.KhanoumiGateType.Equals(KhanoumiGateType.Saman))
            {
                var samanPaymentRequest = new SamanPaymentTokenRequest
                {
                    Action = "token",
                    Amount = request.Amount,
                    TerminalId = SamanTerminalId,
                    ResNum = request.OrderGuid,
                    CellNumber = Convert.ToInt64(request.Mobile),
                    RedirectUrl = request.CallBackUrl,
                };

                var result = GetTokenFromSamanPayment(samanPaymentRequest, context.CancellationToken).Result;

                return Task.FromResult(new TokenResponse
                {
                    Status = 200,
                    Message = "Succeeded",
                    Token = result.Token,
                    Bankurl = SamanRedirectUrl
                });
            }


            if (request.KhanoumiGateType.Equals(KhanoumiGateType.Saman))
            {
                var samanPaymentRequest = new SamanPaymentTokenRequest
                {
                    Action = "token",
                    Amount = request.Amount,
                    TerminalId = SamanTerminalId,
                    ResNum = request.OrderGuid,
                    CellNumber = Convert.ToInt64(request.Mobile),
                    RedirectUrl = request.CallBackUrl,
                };

                var result = GetTokenFromSamanPayment(samanPaymentRequest, context.CancellationToken).Result;

                return Task.FromResult(new TokenResponse
                {
                    Status = 200,
                    Message = "Succeeded",
                    Token = result.Token,
                    Bankurl = SamanRedirectUrl
                });
            }



            return Task.FromResult(new TokenResponse
            {
                Status = 400,
                Message = "Bad Request. The Parameter KhanoumiGateType is not specified."
            });
        }
        public async Task<SamanPaymentTokenResponse> GetTokenFromSamanPayment(SamanPaymentTokenRequest samanPaymentTokenRequest, CancellationToken cancellationToken)
        {
            var httpClient = _clientFactory.CreateClient("saman");
            var serializedSamanPaymentRequest = await Task.Run(() => JsonSerializer.Serialize(samanPaymentTokenRequest), cancellationToken);
            var httpContent = new StringContent(serializedSamanPaymentRequest, Encoding.UTF8, "application/json");
            var apiResponse = await httpClient.PostAsync(httpClient.BaseAddress + SamanTokenRelationalUrl, httpContent, cancellationToken);
            apiResponse.EnsureSuccessStatusCode();
            var responseContent = await apiResponse?.Content.ReadAsStringAsync();
            var result = await Task.Run(() => JsonSerializer.Deserialize<SamanPaymentTokenResponse>(responseContent), cancellationToken);
            return result;
        }
    }
}
