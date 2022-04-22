using api.Interfaces;
using api.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace api.Services
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<SmtpSettings> smtpSetting;

        public EmailService(IOptions<SmtpSettings> smtpSetting)
        {
            this.smtpSetting = smtpSetting;
        }

        public async Task SendAsync(string from, string to, string subject, string body)
        {
            var message = new MailMessage(from,
                    to,
                    subject,
                    body);

            using (var emailClient = new SmtpClient(smtpSetting.Value.Host, smtpSetting.Value.Port))
            {
                emailClient.Credentials = new NetworkCredential(
                    smtpSetting.Value.User,
                    smtpSetting.Value.Password);

                await emailClient.SendMailAsync(message);
            }
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            // Compose Email Message 
            var message = new MailMessage(smtpSetting.Value.User,
                    to,
                    subject,
                    body);

            using (var emailClient = new SmtpClient(smtpSetting.Value.Host, smtpSetting.Value.Port))
            {
                emailClient.Credentials = new NetworkCredential(
                    smtpSetting.Value.User,
                    smtpSetting.Value.Password);

                await emailClient.SendMailAsync(message);
            }
        }
    }
}
