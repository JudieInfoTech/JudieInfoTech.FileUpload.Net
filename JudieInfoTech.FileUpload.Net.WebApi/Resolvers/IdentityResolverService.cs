using Microsoft.AspNetCore.Http;

namespace System.Security.Principal
{
  public class IdentityResolverService : IIdentity
  {
    private readonly IHttpContextAccessor Context;

    public IdentityResolverService(IHttpContextAccessor context)
    {
      Context = context;
    }

    public String AuthenticationType
    {
      get { return Context.HttpContext.User?.Identity.AuthenticationType; }
    }

    public Boolean IsAuthenticated
    {
      get { return Context.HttpContext.User?.Identity.IsAuthenticated ?? false; }
    }

    public String Name
    {
      get { return Context.HttpContext.User?.Identity.Name; }
    }

  }

}