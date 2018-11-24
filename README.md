# WebApiGenerator

## Description
WebApiGenerator is a tool that dynamically generates and executes an Asp.Net Core WebApi from a C# class. Its purpose to make it easy to develop and maintain a (large) collection of micro services written in Asp.Net Core / C# and to keep them consistent. This is accomplished by
1) Removing the need to develop and maintain the boilerplate code that is normally necessary to get a functional WebApi, 
2) Enabling easy code sharing of configuration and startup code of different WebApis.
The effect of WebApiGenerator is that you can focus on your business logic, that your business logic doesn't get obfuscated with boilerplate WebApi code which, as a result, keeps your business logic testable.

## Usage

```
webapi -- Asp.Net Core on-the-fly Web service generator.

Usage:
--dry          Print generated controller (C#) code to standard output, don't
               actually start the service itself.
--service      Assembly containing service implementation (required).
--startup      Assembly containing Asp.Net Core startup class. If absent, a
               default is used
--urls         Semi-colon separated list of urls to listen to. E.g.,
               http://0.0.0.0:2700
--help (/?)    Shows this usage information.
```
##  WebApiGenerator
With WebApiGenerator you create your business logic as normal methods in C# classes. The data that comes in during a web request is represented as normal parameters in your C# methods, but you use attributes to specify how that data should flow into your method. For example, the following method is defined in the class `ValuesService`:
```csharp
    [FromPostMethod]
    public string DoSomething([FromHttpHeader("MyHeader")]string text, [FromPayload("X")]string propX, [FromPayload("Y")]DateTime propY
    )
    {
        var result = _stringUtils.ToUpper("Hello " + propX + " " + propY + " " + text);
        return result;
    }
```
The method `DoSomething` is a normal method without any dependencies on Asp.Net. This means the code can easily be unit tested and it doesn't have to deal with payload classes, HTTP headers, etc. The attributes define where the data should come from when the method is called from within a webservice.
* `FromHttpHeader` Indicates that the data comes from the http header with the name `MyHeader`. How it is retrieved and converted to a string (or other data type) is the responsibility of the generator.
* `FromPayLoad` indicates that the data comes from a payload object (typically a JSon object) with a property `X`. How the payload class looks like is the responsibility of the generator.
The method further specifies that it is to be used from an HTTP POST method.

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
        [Microsoft.AspNetCore.Mvc.HttpPostAttribute]
        public Microsoft.AspNetCore.Mvc.IActionResult DoSomething(DoSomethingPayload payload)
        {
            var value0 = GetFromHttpHeader<System.String>("MyHeader");
            if (string.IsNullOrEmpty(value0))
            {
                return BadRequest();
            }
            try
            {
                var result = _service.DoSomething(value0, payload.X, payload.Y);

                return Ok(result);
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
```
This example shows how the original service is injected as a constructor dependency in the generated WebApi controller. The service method `DoSomething` itself is wrapped in an HTTP POST method call. The arguments of the service method are retrieved from an HTTP header and the generated payload class `DoSomethingPayload` with properties `X` and `Y`.
## Running a service
Assuming the above method is contained in the assembly `myservice.dll`, the service can dynamically be compiled into a running web service as follows:
```csharp
dotnet webapi.dll --service myservice.dll --urls http://localhost:2700
```

Yo can now use a tool like postman to create a proper HTTP POST request and access the web service at the specified address http://localhost:2700.

## Service dependencies
Generated WebApi controller classes and the service classes from which they are generated are injected in the Asp.Net Core framework. Therefore, they can benefit from the Dependency Injection facilities of Asp.Net Core. in the example above, this was already demonstrated since the generated controller class has a constructor dependency on the wrapped service class `ValuesService`.

If the service class, itself, has dependencies on additonal services which are to be  injected through the service's constructor, they can be declared outside the normal Asp.Net Core registration mechanism in the Startup's `ConfigureServices` method.

To that end, create a class in the assembly that countains your service class and have it implement `IServiceMethadata`:
```c#
public interface IServiceMetadata
{
    void ConfigureServices(IServiceCollection services);
}
```
This interface has just a single method that is used for registering additional servics. If such a class exists in your assembly, WebApiGenerator automatically finds and instantiates it, and it will then invoke the `ConfigureServices` method.
### Service setup
The WebApiGenerator cannot and does not make many assumptions about how your services should run. For basic services, the default service startup class that is used will be sufficient. However, if your service has special requirements in order to function properly, you have to define the corresponding startup class yourself. This is the case, for example, if a database connections needs to be created, or of your service uses authentication. The good thing is that this startup class can easily be shared between multiple services. This helps to keep your services consistent.

You can specify your startup class with the `--startup` switch. This switch expects the path to an assembly containing a normal Asp.Net Core startup class (i.e., a class with a `Configure`, and (optional)  `ConfigureServices` methods).
If such a class exists, it will replace the default startup class that otherwise will be used.

## More info
Source code of `WebApiGenerator` is available at [GitHub](https://github.com/merijndejonge/WebApiGenerator). 

`WebApiGenerator` is distributed under the [Apache 2.0 License](https://github.com/merijndejonge/WebApiGenerator/blob/master/LICENSE).
