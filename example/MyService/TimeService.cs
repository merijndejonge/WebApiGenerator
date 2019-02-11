using System;
using Microsoft.Extensions.Configuration;
using OpenSoftware.WebApiClient;

namespace MyService
{
    public class TimeService
    {
        private readonly IStringUtils _stringUtils;

        public TimeService(IStringUtils stringUtils, IConfiguration config)
        {
            _stringUtils = stringUtils;
        }

        [FromGetMethod]
        public string Foo(string foobar)
        {
            return foobar;
        }
        [FromGetMethod]
        public string GetCurrentTime()
        { 
            return DateTime.Now.ToShortTimeString();
        }

        [FromPostMethod]
        public string GetCurrentTimeWithText([FromHttpHeader("MyHeader")]string text, [FromPayload("X")]string bla, [FromPayload("Y")]DateTime foo)
        {
            var result = _stringUtils.ToUpper(DateTime.Now.ToShortTimeString() + " " + text);
            return result;
        }
    }
}