using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace AspNetCore.Security.CAS
{
    public interface ICasTicketValidator
    {
        Task<AuthenticationTicket> ValidateTicket(HttpContext context, AuthenticationProperties properties, AuthenticationScheme scheme, CasOptions options, string ticket, string service);
    }
}