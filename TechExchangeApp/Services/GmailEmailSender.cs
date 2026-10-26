using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TechExchangeApp.Configuration;
using TechExchangeApp.Interfaces;

namespace TechExchangeApp.Services
{
    public class GmailEmailSender : IEmailSender
    {
        private readonly ISystemParameterService _params;
        private readonly ILogger<GmailEmailSender> _logger;

        public GmailEmailSender(ISystemParameterService @params, ILogger<GmailEmailSender> logger)
        {
            _params = @params;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body, bool isHtml = true)
        {
            var host        = await _params.GetAsync(ParameterKeys.SmtpHost)        ?? "smtp.gmail.com";
            var port        = await _params.GetIntAsync(ParameterKeys.SmtpPort, 587);
            var email       = await _params.GetRequiredAsync(ParameterKeys.SmtpEmail);
            var password    = await _params.GetRequiredAsync(ParameterKeys.SmtpPassword);
            var displayName = await _params.GetAsync(ParameterKeys.SmtpDisplayName) ?? "TechPort";
            var maxRetry    = await _params.GetIntAsync(ParameterKeys.SmtpMaxRetry, 3);
            var retryDelay  = await _params.GetIntAsync(ParameterKeys.SmtpRetryDelaySeconds, 5);
            var skipCert    = await _params.GetBoolAsync(ParameterKeys.SmtpSkipCertCheck, false);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(displayName, email));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml) builder.HtmlBody = body;
            else        builder.TextBody = body;
            message.Body = builder.ToMessageBody();

            Exception? lastEx = null;
            for (int attempt = 1; attempt <= maxRetry; attempt++)
            {
                try
                {
                    using var client = new SmtpClient();
                    if (skipCert)
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    // Cổng 465 dùng SSL ngay khi kết nối, các cổng khác dùng STARTTLS
                    await client.ConnectAsync(host, port, port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(email, password);
                    var response = await client.SendAsync(message);
                    await client.DisconnectAsync(true);

                    _logger.LogInformation(
                        "[Email] Sent to {To} | Subject: {Subject} | Attempt: {Attempt}",
                        to, subject, attempt);
                    return; // success
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _logger.LogWarning(
                        "[Email] Attempt {Attempt}/{Max} failed for {To}: {Error}",
                        attempt, maxRetry, to, ex.Message);

                    if (attempt < maxRetry)
                        await Task.Delay(TimeSpan.FromSeconds(retryDelay));
                }
            }

            throw new Exception($"Email failed after {maxRetry} attempts: {lastEx?.Message}", lastEx);
        }
    }
}
