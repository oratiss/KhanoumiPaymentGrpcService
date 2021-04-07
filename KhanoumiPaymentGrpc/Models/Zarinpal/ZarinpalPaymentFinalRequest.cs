using System;

namespace KhanoumiPaymentGrpc.Models.Zarinpal
{
    public class ZarinpalPaymentFinalRequest
    {
        public string Authority { get; set; }
        public long Amount { get; set; }
        public string ZarinpalMerchantId { get; set; }
        public bool UseSandBox { get; set; }
    }
}
