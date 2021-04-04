namespace KhanoumiPaymentGrpc.Models.SamanBank
{
    public class SamanPaymentTokenResponse
    {
        public int Status { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorDesc { get; set; }
        public string Token { get; set; }
    }
}
