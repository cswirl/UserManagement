using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string from, string to, string subject, string body);
        Task SendAsync(string to, string subject, string body);
    }
}
