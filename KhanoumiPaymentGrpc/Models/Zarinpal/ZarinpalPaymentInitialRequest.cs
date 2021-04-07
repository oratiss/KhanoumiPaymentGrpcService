namespace KhanoumiPaymentGrpc.Models.Zarinpal
{
    public class ZarinpalPaymentInitialRequest
    {
        public string MerchantID { get; set; }
        public string Amount { get; set; }
        public string CallBackUrl { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Description { get; set; }
        public bool UseSandBox { get; set; }
    }
}
