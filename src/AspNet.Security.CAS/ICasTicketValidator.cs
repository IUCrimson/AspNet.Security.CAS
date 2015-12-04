using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;

namespace AspNet.Security.CAS
{
    public interface ICasTicketValidator
    {
        Task<AuthenticateResult> ValidateTicket(HttpContext context, HttpClient httpClient, AuthenticationProperties properties, string ticket, string service);
    }
}
