namespace TechExchangeApp.Configuration
{
    public class SiteBrandingOptions
    {
        public const string SectionName = "SiteBranding";

        public string ShortName { get; set; } = "TechExchange";
        public string OrganizationName { get; set; } = "TechExchange";
        public string Tagline { get; set; } = "";
        public string ProvinceName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
        public string WorkingHours { get; set; } = "";
    }
}
