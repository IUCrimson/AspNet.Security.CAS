using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCore.Security.CAS
{
    public interface ICasTicketValidator
    {
        Task<AuthenticateResult> ValidateTicket(HttpContext context, HttpClient httpClient, AuthenticationProperties properties, string ticket, string service);
    }
}