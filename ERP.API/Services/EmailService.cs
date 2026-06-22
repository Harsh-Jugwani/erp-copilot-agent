using ERP.API.Configurations;
using ERP.API.Models;
using ERP.API.Utilities;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;

namespace ERP.API.Services
{
    public interface IEmailService
    {
        Task<bool> SendApprovalEmailAsync(ApprovalRequest request,string approverEmail);
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly SmtpConfig _smtp;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public EmailService(
            IOptions<SmtpConfig> smtpOptions,
            ILogger<EmailService> logger,
            IWebHostEnvironment environment,
            IHttpContextAccessor httpContextAccessor )
        {
            _smtp = smtpOptions?.Value ?? throw new ArgumentNullException(nameof(smtpOptions));
            _logger = logger;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> SendApprovalEmailAsync(ApprovalRequest request, string approverEmail)
        {
            // Validate expiry
            if (DateTime.UtcNow > request.ExpiryDate)
            {
                _logger.LogWarning("Approval token expired before sending email to {To}", approverEmail);
                throw new InvalidOperationException("Approval request has already expired.");
            }
            var url = _httpContextAccessor.HttpContext?.Request.Host;
            var baseUrl = $"https://{url}";
            var approveLink = $"{baseUrl}/api/InvoiceApproval/process-token?token={request.ApprovalToken}&approve=true";
            var rejectLink = $"{baseUrl}/api/InvoiceApproval/process-token?token={request.ApprovalToken}&approve=false";

            var placeholders = new Dictionary<string, string>
            {
                { "ApproverName", approverEmail ?? "Approver" },
                { "InvoiceNumber", request.Invoice.InvoiceNumber },
                { "VendorName", request.Invoice.VendorName },
                { "Amount", request.Invoice.Amount.ToString("F2") },
                { "InvoiceDate", request.Invoice.InvoiceDate.ToString("yyyy-MM-dd") },
                { "RequestedBy", request.RequestedBy },
                { "Remarks", request.Remarks ?? "No remarks" },
                { "ApprovalApproveLink", approveLink },
                { "ApprovalRejectLink", rejectLink },
                { "ExpiryDate", request.ExpiryDate.ToString("yyyy-MM-dd HH:mm:ss") },
                { "CompanyName", "ERP System" }
            };

            var emailBody = await GetApprovalRequestEmailAsync(placeholders);
            return await SendEmailAsync(approverEmail, "Invoice Approval Request", emailBody);
        }
        private async Task<string> GetApprovalRequestEmailAsync(Dictionary<string, string> placeholders)
        {
            return await LoadAndPopulateTemplateAsync("ApprovalRequestEmail.html", placeholders);
        }

        private async Task<string> LoadAndPopulateTemplateAsync(string templateName, Dictionary<string, string> placeholders)
        {
            try
            {
                var templatePath = Path.Combine(_environment.ContentRootPath, "Templates", templateName);

                if (!File.Exists(templatePath))
                {
                    _logger.LogWarning("Email template not found at {TemplatePath}", templatePath);
                    throw new FileNotFoundException($"Email template '{templateName}' not found.");
                }

                var content = await File.ReadAllTextAsync(templatePath);

                // Replace placeholders
                foreach (var placeholder in placeholders)
                {
                    content = content.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value ?? string.Empty);
                }

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email template {TemplateName}", templateName);
                throw;
            }
        }
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(to) || string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(body))
            {
                throw new InvalidOperationException("Email parameters (to, subject, body) are required.");
            }

            if (string.IsNullOrWhiteSpace(_smtp.Host) || _smtp.Port == 0 ||
                string.IsNullOrWhiteSpace(_smtp.UserName) || string.IsNullOrWhiteSpace(_smtp.Password))
            {
                throw new InvalidOperationException("SMTP is not configured correctly.");
            }
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(ApplicationConstants.AdminAccount.Email);
                    mail.To.Add(to);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(_smtp.Host, _smtp.Port))
                    {
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password);
                        smtp.EnableSsl = true;
                        await smtp.SendMailAsync(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To} with subject '{Subject}'", to, subject);
                throw;
            }
        }
    }
}