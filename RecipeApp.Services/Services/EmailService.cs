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
    }
}
