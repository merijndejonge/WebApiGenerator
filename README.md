# WebApiGenerator

## Description
WebApiGenerator is a tool that dynamically generates and executes an Asp.Net Core WebApi from a C# class. Its purpos to make it easier to develop and maintain a (large) collection of micro services written in Asp.Net Core / C# and to keep them consistent. This is accomplished by
1) Removing the need to develop and maintain the boilerplate code that is normally necessary to get a funcional WebApi, 
2) Enabling easy code sharing of configuration and startup code of different WebApis.
The effect of WebApiGenerator is that you can focus on your business logic and that this code gets better testable.

##  WebApiGenerator
With WebApigenerator you create your business logic as normal methods in C# classes. The data that comes in durin a web request is represented as noral parameters in your C# methods, but you use attributes to specify how that data should flow into your method. For example
```csharp
    [FromPostMethod]
    public string DoSomething([FromHttpHeader("MyHeader")]string text, [FromPayload("X")]string propX, [FromPayload("Y")]DateTime propY
    )
    {
        var result = _stringUtils.ToUpper("Hello " + propX + " " + propY + " " + text);
        return result;
    }
```
The method `DoSomething` is a normal method without any dependencies on Asp.Net. This means the code can easily be unit tested and it doesn't have to deal with payload classes, http headers, etc. The attributes define where the data should come from when the method is called from with a webservice.
* `FromHttpHeader` Indicates that the data comes form the http header with ame `MyHeader`. How it is retrieved and converted to a string (or other data type) is the responsibility of the generator.
* `FromPayLoad` indicates that the data comes from a payload object (typically a JSon object) with a property `X`. How the payload class looks like is the responsigiblity of the generator.
The method further specifies that it is to be used from a HTTP [post method.

The generated code for this method looks as follows:
```csharp
namespace OpenSoftware.WebControllers.ValuesService
{
    public class DoSomethingPayload
    {
        public System.String X
        {
            get;
            set;
        }

        public System.DateTime Y
        {
            get;
            set;
        }
    }

    public class ValuesController : OpenSoftware.WebApiGenerator.ControllerBase.ServiceControllerBase
    {
        private readonly MyService.ValuesService _service;
        public ValuesController(MyService.ValuesService service)
        {
            _service = service;
        }

        [Microsoft.AspNetCore.Mvc.HttpGetAttribute]
        public Microsoft.AspNetCore.Mvc.IActionResult Get()
        {
            var result = _service.Get();
            return Ok(result);
        }

        [Microsoft.AspNetCore.Mvc.HttpPostAttribute]
        public Microsoft.AspNetCore.Mvc.IActionResult DoSomething(DoSomethingPayload payload)
        {
            var value0 = GetFromHttpHeader<System.String>("MyHeader");
            if (string.IsNullOrEmpty(value0))
            {
                return BadRequest();
            }

            var result = _service.DoSomething(value0, payload.X, payload.Y);
            return Ok(result);
        }
    }
}
```
## Running a service
Assuming the above method iscontained in the assembly `myservice.dll`, the serevice can dynamically be compiled into a running web service as follows:
```csharp
dotnet webapi.dll --service myservice.dll --urls http://localhost:2700
```

Yo can now use a tool like postman to create a proper HTTP post request and access webservice.

## More info
Source code of `WebApiGenerator` is available at [GitHub](https://github.com/merijndejonge/WebApiGenerator). 

`WebApiGenerator.Dgml` is distributed under the [Apache 2.0 License](https://github.com/merijndejonge/WebApiGenerator/blob/master/LICENSE).
