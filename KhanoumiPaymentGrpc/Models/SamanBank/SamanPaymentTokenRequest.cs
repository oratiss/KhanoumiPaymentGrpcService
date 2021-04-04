namespace KhanoumiPaymentGrpc.Models.SamanBank
{
    public class SamanPaymentTokenRequest
    {
        public string Action { get; set; }
        public string TerminalId { get; set; }
        public string RedirectUrl { get; set; }
        public string ResNum { get; set; }
        public long Amount { get; set; }
        public long CellNumber { get; set; }
        public IBANInfo[] SettleMentIBANInfo { get; set; }
    }

    public class IBANInfo
    {
        public string IBAN { get; set; }
        public long Amount { get; set; }
        public string PurchaseID { get; set; }
    }
}
