using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBP
{
    /// <summary>
    /// The PropertyFromServicesAttribute will expose the FromServices functionality of MVC to properties essentially
    /// allowing you to inject (or bind) to a property on a class in which injection is supported (like a controller).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class PropertyFromServicesAttribute : FromServicesAttribute
    {

    }
}