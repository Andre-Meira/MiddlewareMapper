using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

public static class MiddlewareAttribute
{

    /// <summary>
    ///    using specific Middleware on the endpoint through Attribute
    /// </summary>
    /// <param name="builder"></param>
    /// <typeparam name="Middleware"></typeparam>
    /// <typeparam name="RefMiddlwareAttribute"></typeparam>
    /// <returns></returns>
    public static IApplicationBuilder MapMiddlewareForAttribute<Middleware,RefMiddlewareAttribute>(this IApplicationBuilder builder)
        where RefMiddlewareAttribute: Attribute 
    {        
        var endpoints = GetEndpointsForAttribute<RefMiddlewareAttribute>();                
        
        foreach (var item in endpoints)
        {
            builder.UseWhen(conxt => conxt.Request.Path.StartsWithSegments($"/{item.Path}"), config => {
                config.UseMiddleware<Middleware>();
            });        
        }      

        return builder;     
    }

    
    private static EndpointObject[] GetEndpointsForAttribute<RefAttribute>() where RefAttribute: Attribute
    {   
        Type obj = typeof(ControllerBase);         
        List<EndpointObject>?  endpoints = new List<EndpointObject>();                
        IEnumerable<Type> controllers = Assembly.GetExecutingAssembly().ExportedTypes.Where(x => x.BaseType.MetadataToken == obj.MetadataToken);

        foreach (var controller in controllers)
        {            
            IEnumerable<MethodInfo> methods = controller.GetMethods().Where(x => x.DeclaringType.MetadataToken == controller.MetadataToken);
            IEnumerable<RouteAttribute> routeAttributes = controller.GetCustomAttributes<RouteAttribute>();

            foreach (var methodInfo in methods)
            {
                EndpointObject endpoint = new EndpointObject();
                IEnumerable<HttpMethodAttribute> httpMethods = methodInfo.GetCustomAttributes<HttpMethodAttribute>();
                IEnumerable<RefAttribute> methodAttribute = methodInfo.GetCustomAttributes<RefAttribute>();

                if(httpMethods.Count() == 0) continue;                                
                if(methodAttribute.Count() == 0) continue;
                if(routeAttributes.Count() != 0)
                    endpoint.Path = string.Format("{0}/",routeAttributes.ToArray()[0].Template);

                endpoint.Attributes = methodInfo.GetCustomAttributes().ToArray()[0];                
                endpoint.Method = methodInfo;
                endpoint.Path += string.Format("{0}",httpMethods.ToArray()[0].Template);;
                endpoints.Add(endpoint);                    
            }
        }
        return endpoints.ToArray();            
    }

    private class EndpointObject 
    {   
        public string Path { get; set; } 
        public MethodInfo? Method { get; set; }   
        public Attribute? Attributes { get; set; }         
    }
}


