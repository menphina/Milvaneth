using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;

namespace Milvaneth.Server.Service.Implement
{
    public class SmtpVerifyMailService : IVerifyMailService
    {
        public readonly MailboxAddress Sender;

        public const string Subject = "密码丢失/找回 - Milvaneth 账户服务";

        public readonly Func<string, string, string> MessageTemplate = (username, code) =>
            $"{(string.IsNullOrWhiteSpace(username) ? "用户" : username)} 您好！\n\n验证码：{code}\n\nMilvaneth 账户服务";

        private string _server;
        private int _port;
        private bool _ssl;
        private string _username;
        private string _password;

        public SmtpVerifyMailService(IConfigurationRoot conf)
        {
            var section = conf.GetSection("MailConfig") ?? throw new InvalidOperationException("No email config");

            var name = section.GetValue("SenderName", "");
            var email = section.GetValue("SenderAddress", "");
            Sender = string.IsNullOrWhiteSpace(name) ? new MailboxAddress(email) : new MailboxAddress(name, email);

            _server = section.GetValue("Server", "smtp.sendgrid.net");
            _port = section.GetValue("Port", 587);
            _ssl = section.GetValue("UseSsl", false);
            _username = section.GetValue("Username", "apikey");
            _password = section.GetValue("Password", "");
        }

        public void SendCode(string email, string nickname, string code)
        {
            var message = new MimeMessage();
            message.From.Add(Sender);

            var to = new MailboxAddress(email);
            message.To.Add(to);

            message.Subject = Subject;
            message.ReplyTo.Add(Sender);

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = MessageTemplate(nickname, code).Replace("\n", "<br/>"),
                TextBody = MessageTemplate(nickname, code)
            };
            message.Body = bodyBuilder.ToMessageBody();

            var client = new SmtpClient();
            client.Connect(_server, _port, _ssl);
            client.Authenticate(_username, _password);
            client.Send(message);
            client.Disconnect(true);
            client.Dispose();
        }
    }
}
