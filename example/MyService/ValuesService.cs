using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenSoftware.WebApiClient;

namespace MyService
{
    public class ValuesService
    {
        private readonly IStringUtils _stringUtils;

        public ValuesService(IStringUtils stringUtils, IConfiguration config)
        {
            _stringUtils = stringUtils;
        }
        [FromGetMethod]
        public string Get()
        {
            return "Hi there!";
        }
        [FromPostMethod]
        public async Task<string> DoDomething()
        {
            return await Task.FromResult("Hello");
        }
        [FromPostMethod]
        public string DoSomething([FromHttpHeader("MyHeader")]string text, [FromPayload("X")]string propX, [FromPayload("Y")]DateTime propY
        )
        {
            var result = _stringUtils.ToUpper("Hello " + propX + " " + propY + " " + text);
            return result;
        }
    }
}