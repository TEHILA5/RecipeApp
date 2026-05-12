using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using RecipeApp.Common.DTOs;
using RecipeApp.Services.Interfaces;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace RecipeApp.Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendNewsletterAsync(string toEmail, string toName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = "🍰 Sweet&Treat Newsletter — Welcome to the Sweet Side!";

            var builder = new BodyBuilder
            {
                HtmlBody = BuildNewsletterHtml(toName)
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string BuildNewsletterHtml(string name) => $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #fdf2f8; margin: 0; padding: 0; }}
    .container {{ max-width: 600px; margin: 0 auto; background: white; border-radius: 20px; overflow: hidden; }}
    .header {{ background: linear-gradient(135deg, #d4547a, #e8799a); padding: 48px 32px; text-align: center; color: white; }}
    .header h1 {{ font-size: 2.2rem; margin: 0 0 8px; }}
    .header p {{ opacity: 0.9; margin: 0; font-size: 1rem; }}
    .body {{ padding: 40px 32px; }}
    .greeting {{ font-size: 1.1rem; color: #374151; margin-bottom: 24px; }}
    .section {{ margin-bottom: 32px; }}
    .section h2 {{ color: #d4547a; font-size: 1.2rem; margin-bottom: 12px; border-bottom: 2px solid #fce7f3; padding-bottom: 8px; }}
    .recipe-card {{ background: #fdf2f8; border-radius: 12px; padding: 16px; margin-bottom: 12px; }}
    .recipe-card h3 {{ color: #1f2937; margin: 0 0 6px; font-size: 1rem; }}
    .recipe-card p {{ color: #6b7280; margin: 0; font-size: 0.88rem; }}
    .tip-box {{ background: #fff7ed; border-left: 4px solid #f59e0b; border-radius: 8px; padding: 16px; margin-bottom: 12px; }}
    .cta {{ text-align: center; margin: 32px 0; }}
    .cta a {{ background: linear-gradient(135deg, #d4547a, #e8799a); color: white; padding: 14px 36px; border-radius: 999px; text-decoration: none; font-weight: 700; font-size: 1rem; }}
    .footer {{ background: #fdf2f8; padding: 24px 32px; text-align: center; color: #9ca3af; font-size: 0.82rem; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <div style='font-size:3rem;margin-bottom:12px'>🍰</div>
      <h1>Sweet&amp;Treat</h1>
      <p>Your monthly dose of dessert inspiration</p>
    </div>
    <div class='body'>
      <p class='greeting'>Hi {name}! 👋<br>Welcome to the Sweet&amp;Treat newsletter — we're so happy you're here.</p>
 
      <div class='section'>
        <h2>🌟 Featured This Month</h2>
        <div class='recipe-card'>
          <h3>🎂 Classic Chocolate Layer Cake</h3>
          <p>Rich, moist layers with silky ganache frosting. The ultimate celebration cake.</p>
        </div>
        <div class='recipe-card'>
          <h3>🍪 Brown Butter Chocolate Chip Cookies</h3>
          <p>Nutty, chewy, and crispy at the edges. One bowl, 30 minutes.</p>
        </div>
        <div class='recipe-card'>
          <h3>🍋 No-Bake Lemon Cheesecake</h3>
          <p>Light, zesty, and effortlessly elegant. Perfect for summer.</p>
        </div>
      </div>
 
      <div class='section'>
        <h2>👩‍🍳 Baking Tip of the Month</h2>
        <div class='tip-box'>
          <strong>Brown your butter!</strong> Cooking butter until golden and nutty adds incredible depth to cookies, cakes, and frostings. Just melt it in a light-colored pan and watch for the milk solids to turn amber.
        </div>
      </div>
 
      <div class='section'>
        <h2>🔍 Smart Search is Here!</h2>
        <p style='color:#6b7280;line-height:1.6'>
          Try our new <strong style='color:#d4547a'>Smart Search</strong> — just describe what you're craving in plain English and we'll find the perfect recipe. Try: <em>light festive chocolate cake</em> or <em>quick no-bake summer dessert</em>.
        </p>
      </div>
 
      <div class='cta'>
        <a href = 'http://localhost:5173/recipes' > Explore All Recipes →</a>
          </div>
        </div>
        <div class='footer'>
          © 2026 Sweet&amp;Treat — Made with 💕<br>
          You're receiving this because you subscribed at sweetandtreat.com
        </div>
      </div>
    </body>
    </html>";

        public async Task SendContactToAdminAsync(ContactMessageDto dto)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(dto.Name, dto.Email));
            message.To.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.Subject = $"[Sweet&Treat Contact] {dto.Category} — {dto.Urgency} | {dto.Name}";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
        <div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:24px'>
          <h2 style='color:#d4547a'>📬 New Contact Message</h2>
          <table style='width:100%;border-collapse:collapse'>
            <tr><td style='padding:8px;font-weight:bold;color:#374151'>Name:</td><td style='padding:8px'>{dto.Name}</td></tr>
            <tr style='background:#fdf2f8'><td style='padding:8px;font-weight:bold;color:#374151'>Email:</td><td style='padding:8px'><a href='mailto:{dto.Email}'>{dto.Email}</a></td></tr>
            <tr><td style='padding:8px;font-weight:bold;color:#374151'>Category:</td><td style='padding:8px'>{dto.Category}</td></tr>
            <tr style='background:#fdf2f8'><td style='padding:8px;font-weight:bold;color:#374151'>Recipe:</td><td style='padding:8px'>{dto.RecipeName ?? "—"}</td></tr>
            <tr><td style='padding:8px;font-weight:bold;color:#374151'>Urgency:</td><td style='padding:8px'>{dto.Urgency}</td></tr>
          </table>
          <div style='margin-top:20px;padding:16px;background:#fdf2f8;border-radius:12px'>
            <strong style='color:#374151'>Message:</strong>
            <p style='color:#4b5563;margin-top:8px'>{dto.Message}</p>
          </div>
          <p style='color:#9ca3af;font-size:0.8rem;margin-top:24px'>Sent via Sweet&amp;Treat Contact Form</p>
        </div>"
            };

            message.Body = builder.ToMessageBody();
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        public async Task SendReplyToUserAsync(string toEmail, string toName, string subject, string replyContent)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = $"Re: {subject} — Sweet&Treat";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
        <!DOCTYPE html><html><head><meta charset='utf-8'></head>
        <body style='font-family:Arial,sans-serif;background:#fdf2f8;margin:0;padding:0'>
          <div style='max-width:600px;margin:0 auto;background:white;border-radius:20px;overflow:hidden'>
            <div style='background:linear-gradient(135deg,#d4547a,#e8799a);padding:40px 32px;text-align:center;color:white'>
              <div style='font-size:2.5rem;margin-bottom:8px'>🍰</div>
              <h1 style='margin:0 0 6px;font-size:1.6rem'>Sweet&amp;Treat</h1>
              <p style='margin:0;opacity:0.9'>We've replied to your message</p>
            </div>
            <div style='padding:40px 32px'>
              <p style='color:#374151;font-size:1rem'>Hi {toName}! 👋</p>
              <p style='color:#374151'>Thank you for reaching out. Here's our response to your message:</p>
              <div style='background:#fdf2f8;border-left:4px solid #d4547a;border-radius:8px;padding:20px;margin:24px 0'>
                <p style='color:#1f2937;line-height:1.7;margin:0'>{replyContent}</p>
              </div>
              <p style='color:#6b7280;font-size:0.9rem'>If you have any further questions, feel free to contact us again.</p>
              <p style='color:#6b7280;font-size:0.9rem'>With love, 💕<br><strong style='color:#d4547a'>The Sweet&amp;Treat Team</strong></p>
            </div>
            <div style='background:#fdf2f8;padding:20px 32px;text-align:center;color:#9ca3af;font-size:0.8rem'>
              © 2026 Sweet&amp;Treat — Made with 💕
            </div>
          </div>
        </body></html>"
            };

            message.Body = builder.ToMessageBody();
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
