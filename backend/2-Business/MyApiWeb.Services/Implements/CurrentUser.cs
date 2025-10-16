using Microsoft.AspNetCore.Http;
using MyApiWeb.Models.Interfaces;
using System.Security.Claims;

namespace MyApiWeb.Services.Implements
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? Id
        {
            get
            {
                var id = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return id == null ? null : Guid.Parse(id);
            }
        }
    }
}