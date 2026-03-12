namespace cafe.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPayRequestModel model);
        VnPayResponseModel PaymentExecute(IQueryCollection collections);
    }

    public class VnPayRequestModel
    {
        public int OrderId { get; set; }
        public string FullName { get; set; } = "";
        public string Description { get; set; } = "";
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class VnPayResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string OrderDescription { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string PaymentId { get; set; } = "";
        public string TransactionId { get; set; } = "";
        public string Token { get; set; } = "";
        public string VnPayResponseCode { get; set; } = "";
    }

    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContext context, VnPayRequestModel model)
        {
            var vnpay = new VnPayLibrary();
            var vnp_TmnCode = _config["VnPay:TmnCode"];
            var vnp_HashSecret = _config["VnPay:HashSecret"];
            var vnp_Url = _config["VnPay:BaseUrl"];
            var vnp_ReturnUrl = _config["VnPay:ReturnUrl"];

            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode!);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString()); // Số tiền nhân 100
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang:" + model.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl!);
            vnpay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());

            return vnpay.CreateRequestUrl(vnp_Url!, vnp_HashSecret!);
        }

        public VnPayResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value!);
                }
            }

            var vnp_orderId = vnpay.GetResponseData("vnp_TxnRef");
            var vnp_TransactionId = vnpay.GetResponseData("vnp_TransactionNo");
            var vnp_SecureHash = collections.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash!, _config["VnPay:HashSecret"]!);

            if (!checkSignature)
            {
                return new VnPayResponseModel { Success = false };
            }

            return new VnPayResponseModel
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = vnp_OrderInfo,
                OrderId = vnp_orderId,
                TransactionId = vnp_TransactionId,
                Token = vnp_SecureHash!,
                VnPayResponseCode = vnp_ResponseCode
            };
        }
    }
}
