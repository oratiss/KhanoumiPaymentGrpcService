using Grpc.Core;
using KhanoumiPaymentGrpc.Models.SamanBank;
using KhanoumiPyamentGrpc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KhanoumiPaymentGrpc.Models.Constants.KhanoumiPaymentServiceConstants;
using KhanoumiPaymentGrpc.Models.Zarinpal;
using static KhanoumiPaymentGrpc.Models.Constants.KhanoumiPaymentServiceConstants.ConstantsProvider;
using static KhanoumiPaymentGrpc.Models.Constants.SamanServiceConstants.SamanServiceConstantProvider;
using static KhanoumiPaymentGrpc.Models.Constants.ZarinpalServiceConstants.ZarinpalServiceConstantProvider;
using static KhanoumiPaymentGrpc.Models.Enumeration;


namespace KhanoumiPaymentGrpc.Services
{
    public class KhanoumiPaymentService : KhanoumiPayment.KhanoumiPaymentBase, IKhanoumiPaymentService
    {
        private readonly ILogger<KhanoumiPaymentService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        private ConstantsProvider _constantsProvider;

        public KhanoumiPaymentService(ILogger<KhanoumiPaymentService> logger, IHttpClientFactory clientFactory,
            ConstantsProvider constantsProvider)
        {
            _logger = logger;
            _clientFactory = clientFactory;
            _constantsProvider = constantsProvider;
        }

        public override Task<TokenResponse> GetToken(TokenRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                return Task.FromResult(new TokenResponse
                {
                    GrpcStatus = 400,
                    GrpcMessage = "Bad Request. request is null."
                });
            }

            if (!_constantsProvider.AuthenticationPairs.ContainsKey(request.GrpcMerchandId)
                || !_constantsProvider.AuthenticationPairs.ContainsValue(request.GrpcMerchantPassword)
                || _constantsProvider.AuthenticationPairs[request.GrpcMerchandId] != GrpcPassword)
            {
                return Task.FromResult(new TokenResponse
                {
                    GrpcStatus = 401,
                    GrpcMessage = "Authorization Failed."
                });
            }

            if (request.KhanoumiGateType.Equals((int)KhanoumiGateType.Saman))
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
                    GrpcStatus = 200,
                    GrpcMessage = "Token received from Bank Successfully.",
                    Status = result.Status,
                    ErrorCode = result.ErrorCode,
                    Message = result.ErrorDesc,
                    Token = result.Token,
                    BankUrl = SamanRedirectUrl
                });
            }


            if (request.KhanoumiGateType.Equals((int)KhanoumiGateType.Zarinpal))
            {
                var zarinPalPaymentInitialRequest = new ZarinpalPaymentInitialRequest()
                {
                    MerchantID = ZarinpalMerchandId,
                    Amount = request.Amount.ToString(),
                    CallBackUrl = request.CallBackUrl,
                    Mobile = request.Mobile,
                    Email = request.Email,
                    Description = request.Description,
                    UseSandBox = request.UseSandBox
                };

                var result = GetTokenFromZarinpalPayment(zarinPalPaymentInitialRequest, context.CancellationToken).Result;

                return Task.FromResult(new TokenResponse
                {
                    GrpcStatus = 200,
                    GrpcMessage = "First Step completed successfully.",
                    Status = result.Status,
                    Authority = result.Authority,
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

        public async Task<ZarinpalPaymentInitialResponse> GetTokenFromZarinpalPayment(ZarinpalPaymentInitialRequest zarinpalPaymentInitialRequest, CancellationToken cancellationToken)
        {
            var zarinpalUrl = $"https://{(zarinpalPaymentInitialRequest.UseSandBox ? "sandbox" : "www")}.zarinpal.com/pg/rest/WebGate/PaymentRequest.json";

            var _values = new Dictionary<string, string>
            {
                { "MerchantID", zarinpalPaymentInitialRequest.MerchantID }, //Change This To work, some thing like this : xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
                { "Amount", zarinpalPaymentInitialRequest.Amount }, //Toman
                { "CallbackURL", zarinpalPaymentInitialRequest.CallBackUrl },
                { "Mobile", zarinpalPaymentInitialRequest.Mobile },
                { "Email", zarinpalPaymentInitialRequest.Email },
                { "Description", zarinpalPaymentInitialRequest.Description }
            };

            var httpClient = _clientFactory.CreateClient();
            var serializedZarinpalTokenRequest = await Task.Run(() => JsonSerializer.Serialize(_values), cancellationToken);
            var httpContent = new StringContent(serializedZarinpalTokenRequest, Encoding.UTF8, "application/json");
            var apiResponse = await httpClient.PostAsync(zarinpalUrl, httpContent, cancellationToken);
            apiResponse.EnsureSuccessStatusCode();
            var responseContent = await apiResponse?.Content.ReadAsStringAsync();
            var result = await Task.Run(() => JsonSerializer.Deserialize<ZarinpalPaymentInitialResponse>(responseContent), cancellationToken);
            return result;
        }

        public override Task<PaymentResponse> Pay(PaymentRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                return Task.FromResult(new PaymentResponse
                {
                    GrpcStatus = 400,
                    GrpcMessage = "Bad Request. request is null."
                });
            }

            if (!_constantsProvider.AuthenticationPairs.ContainsKey(request.GrpcMerchandId)
                || !_constantsProvider.AuthenticationPairs.ContainsValue(request.GrpcMerchantPassword)
                || _constantsProvider.AuthenticationPairs[request.GrpcMerchandId] != GrpcPassword)
            {
                return Task.FromResult(new PaymentResponse
                {
                    GrpcStatus = 401,
                    GrpcMessage = "Authorization Failed."
                });
            }


            //Todo
            //if (request.KhanoumiGateType.Equals((int)KhanoumiGateType.Zarinpal))
            //{
            //    var samanPaymentFinalRequest = new SamanPaymentFinalRequest()
            //    {

            //    };

            //    var result = PayBySmaanPayment(samanPaymentFinalRequest, context.CancellationToken).Result;

            //    return Task.FromResult(new PaymentResponse()
            //    {

            //    });
            //}


            if (request.KhanoumiGateType.Equals((int)KhanoumiGateType.Zarinpal))
            {
                var zarinpalPaymentFinalRequest = new ZarinpalPaymentFinalRequest()
                {
                    ZarinpalMerchantId = ZarinpalMerchandId,
                    Amount = request.Amount,
                    Authority = request.Authority,
                    
                };

                var result = PayByZarinpalPayment(zarinpalPaymentFinalRequest, context.CancellationToken).Result;

                return Task.FromResult(new PaymentResponse
                {
                    GrpcStatus = 200,
                    GrpcMessage = "Results received from bank successfully.",
                    Status = result.Status,
                    RefId = result.RefID
                });
            }


            return Task.FromResult(new PaymentResponse
            {
                GrpcStatus = 400,
                GrpcMessage = "Bad Request. The Parameter KhanoumiGateType is not specified."
            });
        }


        public async Task<ZarinpalPaymentFinalResponse> PayByZarinpalPayment(ZarinpalPaymentFinalRequest zarinpalPaymentFinalRequest, CancellationToken cancellationToken)
        {
            var zarinpalUrl = $"https://{(zarinpalPaymentFinalRequest.UseSandBox ? "sandbox" : "www")}.zarinpal.com/pg/rest/WebGate/PaymentRequest.json";

            var _values = new Dictionary<string, string>
            {
                { "MerchantID", zarinpalPaymentFinalRequest.ZarinpalMerchantId }, 
                { "Authority", zarinpalPaymentFinalRequest.Authority },
                { "Amount", zarinpalPaymentFinalRequest.Amount.ToString() } //Toman

            };

            var httpClient = _clientFactory.CreateClient();
            var serializedZarinpalPaymentRequest = await Task.Run(() => JsonSerializer.Serialize(_values), cancellationToken);
            var httpContent = new StringContent(serializedZarinpalPaymentRequest, Encoding.UTF8, "application/json");
            var apiResponse = await httpClient.PostAsync(zarinpalUrl, httpContent, cancellationToken);
            apiResponse.EnsureSuccessStatusCode();
            var responseContent = await apiResponse?.Content.ReadAsStringAsync();
            var result = await Task.Run(() => JsonSerializer.Deserialize<ZarinpalPaymentFinalResponse>(responseContent), cancellationToken);
            return result;
        }



    }
}
