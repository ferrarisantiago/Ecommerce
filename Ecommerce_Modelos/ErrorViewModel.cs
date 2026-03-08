namespace Ecommerce_Modelos
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int? StatusCode { get; set; }
        public string? FriendlyMessage { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
