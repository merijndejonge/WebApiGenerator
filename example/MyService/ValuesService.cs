using System;
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
        public string Post([FromHttpHeader("Foo")]string header, [FromPayload("X")]string bla, [FromPayload("Y")]DateTime foo)
        {
            var result = _stringUtils.ToUpper("Hello " + bla + " " + foo + " " + header);
            return result;
        }
    }
}