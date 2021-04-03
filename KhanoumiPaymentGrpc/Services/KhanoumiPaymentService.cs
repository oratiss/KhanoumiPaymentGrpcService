using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using KhanoumiPaymentGrpc.Models;
using KhanoumiPyamentGrpc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        public override Task<PaymentResponse> Pay(PaymentRequest request, ServerCallContext context)
        {
            if (request == null)
            {
                return Task.FromResult(new PaymentResponse
                {
                    Status = 400,
                    Message = "Bad Request."
                });
            }

            if (!AuthenticationPairs.ContainsKey(request.MerchandId)
                || !AuthenticationPairs.ContainsValue(request.MerchantPassword)
                || AuthenticationPairs[request.MerchandId] != GrpcPassword)
            {
                return Task.FromResult(new PaymentResponse
                {
                    Status = 401,
                    Message = "Authorization Failed."
                });
            }



            if (request.KhanoumiGateType.Equals(KhanoumiGateType.Saman))
            {
                var samanPaymentRequest = new SamanPaymentRequest
                {
                    Action = "token",
                    Amount = request.Amount,
                    TerminalId = SamanTerminalId,
                    CellNumber = Convert.ToInt64(request.Mobile),
                    RedirectUrl = request.CallBackUrl,
                    ResNum = request.OrderGuid
                };

                var tokenResult = SendViaToken(samanPaymentRequest);


            }












            //return Task.FromResult(new PaymentResponse
            //{
            //    Authority = "Hello " + request.MerchandId
            //});

            return null;
        }
        public JsonResult SendViaToken(SamanPaymentRequest txn)
        {
            


            //string urlAddress = "https://sep.shaparak.ir/MobilePG/MobilePayment";
            //var restClient = new RestClient(urlAddress);
            //var request = new RestRequest(Method.POST)
            //{
            //    RequestFormat = DataFormat.Json,
            //    OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; }
            //};
            ////to request a token you should set Actio property as token.

            //request.AddBody(txn);
            //var sepResult = restClient.Execute(request);
            //var jsonSerializer = new JavaScriptSerializer();
            //var sepPgToken = (IDictionary<string, object>)
            //    jsonSerializer.DeserializeObject(sepResult.Content);
            //return Json(sepPgToken);
            return null;
        }
    }
}
