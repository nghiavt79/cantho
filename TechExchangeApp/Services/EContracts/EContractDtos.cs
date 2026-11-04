using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechExchangeApp.Services.EContracts
{
    /// <summary>Giá trị theo đặc tả eContract, mục 8.1.</summary>
    public static class EContractConstants
    {
        public const string ProviderName = "ECONTRACT";

        public const int ContractDraft = 0;
        public const int ContractProcessing = 20;
        public const int ContractSigned = 30;
        public const int ContractRejected = 31;
        public const int ContractCancelled = 32;
        public const int ContractLiquidated = 40;

        public const int RoleSigner = 3;
        public const int SignTypeUsbToken = 2;
        public const int FieldTypeDigitalSign = 3;
        public const int DocumentTypeFinally = 2;
    }

    // ── Tạo hợp đồng: giữ đúng key trên wire ─────────────────────────────────

    public class EContractCreateRequest
    {
        [JsonPropertyName("contract")]
        public EContractInfo Contract { get; set; } = new();

        [JsonPropertyName("participants")]
        public List<EContractParticipant> Participants { get; set; } = new();

        [JsonPropertyName("fields")]
        public List<EContractFieldRequest> Fields { get; set; } = new();
    }

    public class EContractInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("contract_no")]
        public string? ContractNo { get; set; }

        [JsonPropertyName("sign_time")]
        [JsonConverter(typeof(EContractUtcDateConverter))]
        public DateTime SignTime { get; set; }

        [JsonPropertyName("contract_expire_time")]
        [JsonConverter(typeof(EContractNullableUtcDateConverter))]
        public DateTime? ContractExpireTime { get; set; }

        [JsonPropertyName("type_id")]
        public int? TypeId { get; set; }

        [JsonPropertyName("is_template")]
        public bool IsTemplate { get; set; } = false;
    }

    public class EContractParticipant
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        /// <summary>Theo mẫu: 1 = tổ chức của tài khoản tạo, 3 = đối tác cá nhân. Chốt ở Giai đoạn 0.</summary>
        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("ordering")]
        public int Ordering { get; set; } = 1;

        [JsonPropertyName("recipients")]
        public List<EContractRecipientRequest> Recipients { get; set; } = new();
    }

    public class EContractRecipientRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("role")]
        public int Role { get; set; } = EContractConstants.RoleSigner;

        [JsonPropertyName("ordering")]
        public int Ordering { get; set; } = 1;

        [JsonPropertyName("locale")]
        public string Locale { get; set; } = "vi";

        [JsonPropertyName("sign_type")]
        public List<EContractSignType> SignType { get; set; } = new();

        [JsonPropertyName("card_id")]
        public string? CardId { get; set; }

        [JsonPropertyName("login_by")]
        public string LoginBy { get; set; } = "email";
    }

    public class EContractSignType
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
    }

    public class EContractFieldRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("email_or_phone")]
        public string EmailOrPhone { get; set; } = "";

        [JsonPropertyName("type")]
        public int Type { get; set; } = EContractConstants.FieldTypeDigitalSign;

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("coordinate_x")]
        public double CoordinateX { get; set; }

        [JsonPropertyName("coordinate_y")]
        public double CoordinateY { get; set; }

        [JsonPropertyName("width")]
        public double Width { get; set; }

        [JsonPropertyName("height")]
        public double Height { get; set; }

        [JsonPropertyName("required")]
        public int Required { get; set; } = 1;
    }

    /// <summary>Đặc tả yêu cầu định dạng yyyy-MM-ddTHH:mm:ssZ.</summary>
    public class EContractUtcDateConverter : JsonConverter<DateTime>
    {
        internal const string Format = "yyyy-MM-dd'T'HH:mm:ss'Z'";

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetDateTime();

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToUniversalTime().ToString(Format, CultureInfo.InvariantCulture));
    }

    public class EContractNullableUtcDateConverter : JsonConverter<DateTime?>
    {
        public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType == JsonTokenType.Null ? null : reader.GetDateTime();

        public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
        {
            if (value == null) { writer.WriteNullValue(); return; }
            writer.WriteStringValue(value.Value.ToUniversalTime().ToString(EContractUtcDateConverter.Format, CultureInfo.InvariantCulture));
        }
    }

    // ── Response: đặc tả ghi là projection, chỉ map trường Sàn dùng ───────────

    public class EContractDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("contract_no")]
        public string? ContractNo { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    public class EContractPage
    {
        [JsonPropertyName("total_pages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("total_elements")]
        public long TotalElements { get; set; }

        [JsonPropertyName("entities")]
        public List<EContractDto> Entities { get; set; } = new();
    }

    public class EContractDocumentDto
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("filename")]
        public string? FileName { get; set; }

        [JsonPropertyName("path")]
        public string? Path { get; set; }

        [JsonPropertyName("contract_id")]
        public long? ContractId { get; set; }
    }

    // ── Lỗi ──────────────────────────────────────────────────────────────────

    public enum EContractFailureKind
    {
        /// <summary>Thiếu hoặc sai cấu hình phía Sàn; chưa gọi API nghiệp vụ.</summary>
        Configuration,
        /// <summary>Không đăng nhập được; chưa gọi API nghiệp vụ.</summary>
        Authentication,
        /// <summary>eContract từ chối rõ ràng; thao tác không có hiệu lực.</summary>
        Rejected,
        /// <summary>Timeout, 5xx hoặc response không đọc được; thao tác ghi có thể đã có hiệu lực, phải đối soát trước khi làm lại.</summary>
        Unknown
    }

    public class EContractException : Exception
    {
        public EContractFailureKind Kind { get; }
        public int? HttpStatus { get; }

        public EContractException(EContractFailureKind kind, string message, int? httpStatus = null, Exception? inner = null)
            : base(message, inner)
        {
            Kind = kind;
            HttpStatus = httpStatus;
        }
    }
}
