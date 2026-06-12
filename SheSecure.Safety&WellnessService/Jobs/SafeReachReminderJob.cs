using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SheSecure.Safety_WellnessService.Data;
using System.Text;
using System.Text.Json;

namespace SheSecure.Safety_WellnessService.Jobs
{
    public class SafeReachReminderJob
    {
        private readonly WellnessDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public SafeReachReminderJob(
            WellnessDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Job 1 — Runs at ExpectedArrivalTime
        public async Task SendReminderAsync(int safeReachId)
        {
            var check = await _context.SafeReachChecks
                .FindAsync(safeReachId);

            if (check == null || check.Status != "Pending")
                return;

            await SendNotificationAsync(
                employeeId: check.EmployeeId,
                title: "Safe Arrival Reminder",
                message: "Please confirm that you have reached your destination safely.",
                type: "Safety"
            );
        }

        // Job 2 — Runs 30 min after ExpectedArrivalTime
        public async Task CheckAndEscalateAsync(int safeReachId)
        {
            var check = await _context.SafeReachChecks
                .FindAsync(safeReachId);

            if (check == null || check.IsAcknowledged)
                return;

            check.Status = "Escalated";
            _context.SafeReachChecks.Update(check);
            await _context.SaveChangesAsync();

            var inAppMessage =
                $"Employee ID {check.EmployeeId} failed to confirm safe arrival. Please review immediately.";

            // In-app notifications
            await SendNotificationAsync(
                employeeId: 2,
                title: "SafeReach Escalation - HR Action Required",
                message: inAppMessage,
                type: "Emergency"
            );

            await SendNotificationAsync(
                employeeId: 3,
                title: "SafeReach Escalation - Security Alert",
                message: inAppMessage,
                type: "Emergency"
            );

            await SendNotificationAsync(
                employeeId: 1,
                title: "SafeReach Escalation - Admin Notice",
                message: inAppMessage,
                type: "Emergency"
            );

            // Email notifications
            var subject = $"[SheSecure] SafeReach Escalation - Employee {check.EmployeeId}";
            var body = BuildEmailBody(check.EmployeeId, check.ExpectedArrivalTime);

            var hrEmail = _configuration["Escalation:HrEmail"] ?? "";
            var securityEmail = _configuration["Escalation:SecurityEmail"] ?? "";
            var adminEmail = _configuration["Escalation:AdminEmail"] ?? "";

            await SendEmailAsync(hrEmail, "HR Team", subject, body);
            await SendEmailAsync(securityEmail, "Security Team", subject, body);
            await SendEmailAsync(adminEmail, "Admin", subject, body);
        }

        // ── Private helpers ───────────────────────────────────────────────

        private async Task SendNotificationAsync(
            int employeeId, string title,
            string message, string type)
        {
            try
            {
                var client = _httpClientFactory
                    .CreateClient("NotificationService");

                var payload = JsonSerializer.Serialize(new
                {
                    employeeId = employeeId.ToString(),
                    title,
                    message,
                    type
                });

                var content = new StringContent(
                    payload, Encoding.UTF8, "application/json");

                await client.PostAsync("api/Notification/create", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[SafeReachReminderJob] In-app notification failed: {ex.Message}");
            }
        }

        private async Task SendEmailAsync(
            string toEmail, string toName,
            string subject, string htmlBody)
        {
            try
            {
                var fromEmail = _configuration["Email:FromEmail"] ?? "";
                var fromName = _configuration["Email:FromName"] ?? "SheSecure";
                var appPassword = _configuration["Email:AppPassword"] ?? "";
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");

                if (string.IsNullOrWhiteSpace(appPassword)
                    || string.IsNullOrWhiteSpace(toEmail))
                    return;

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromEmail));
                email.To.Add(new MailboxAddress(toName, toEmail));
                email.Subject = subject;

                email.Body = new TextPart("html")
                {
                    Text = htmlBody
                };

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(
                    smtpHost, smtpPort,
                    SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(fromEmail, appPassword);

                await smtp.SendAsync(email);

                await smtp.DisconnectAsync(true);

                Console.WriteLine(
                    $"[SafeReachReminderJob] Email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[SafeReachReminderJob] Email to {toEmail} failed: {ex.Message}");
            }
        }

        private string BuildEmailBody(
            int employeeId, DateTime expectedArrivalTime)
        {
            var istZone = TimeZoneInfo.FindSystemTimeZoneById(
                "India Standard Time");

            var utcTime = expectedArrivalTime.Kind == DateTimeKind.Utc
                ? expectedArrivalTime
                : expectedArrivalTime.ToUniversalTime();

            var expectedIst = TimeZoneInfo
                .ConvertTimeFromUtc(utcTime, istZone);

            return $@"
<html>
<body style='font-family:Arial,sans-serif;color:#333;'>
  <div style='background:#c0392b;padding:16px;border-radius:6px;'>
    <h2 style='color:white;margin:0;'>SheSecure - SafeReach Escalation Alert</h2>
  </div>
  <div style='padding:20px;'>
    <p>This is an automated escalation alert from the SheSecure platform.</p>
    <table style='border-collapse:collapse;width:100%;'>
      <tr>
        <td style='padding:8px;font-weight:bold;width:200px;'>Employee ID</td>
        <td style='padding:8px;'>{employeeId}</td>
      </tr>
      <tr style='background:#f9f9f9;'>
        <td style='padding:8px;font-weight:bold;'>Expected Arrival (IST)</td>
        <td style='padding:8px;'>{expectedIst:dd MMM yyyy, hh:mm tt}</td>
      </tr>
      <tr>
        <td style='padding:8px;font-weight:bold;'>Escalated At (UTC)</td>
        <td style='padding:8px;'>{DateTime.UtcNow:dd MMM yyyy, HH:mm}</td>
      </tr>
      <tr style='background:#f9f9f9;'>
        <td style='padding:8px;font-weight:bold;'>Status</td>
        <td style='padding:8px;color:#c0392b;font-weight:bold;'>Escalated</td>
      </tr>
    </table>
    <p style='margin-top:20px;'>
      The employee has not confirmed safe arrival 30 minutes past their expected time.
      Please take immediate action.
    </p>
    <p style='color:#888;font-size:12px;'>
      This is an automated message from SheSecure. Do not reply to this email.
    </p>
  </div>
</body>
</html>";
        }
    }
}