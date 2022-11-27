using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JustCSharp.AspNetCore.OpenTelemetry.Extensions;

public static class ActionContextExtensions
{
    public static string GetActivityName(this ActionExecutingContext context)
    {
        return GetActivityName(context.Controller, context.ActionDescriptor);
    }

    public static string GetActivityName(this ActionExecutedContext context)
    {
        return GetActivityName(context.Controller, context.ActionDescriptor);
    }

    private static string GetActivityName(object controller, ActionDescriptor action)
    {
        var controllerName = controller.GetType().Name;
        var actionName = (action as ControllerActionDescriptor)?.ActionName;
        return $"{controllerName}.{actionName}";
    }
}