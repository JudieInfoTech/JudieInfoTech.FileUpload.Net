using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Config;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace System.Config
{
  public class HttpClientConfig
  {
    public String BaseAddress { get; set; }

    public String UserId { get; set; }
    public String UserPassword { get; set; }

  }
}

namespace System.Net.Http
{
  public class HttpRestClient : HttpClient
  {
    public HttpRestClient(IOptions<HttpClientConfig> config) : base()
    {
      BaseAddress = new Uri(config.Value.BaseAddress);

      DefaultRequestHeaders.Accept.Clear();
      DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
  }
}