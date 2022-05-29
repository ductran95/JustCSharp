using System;

namespace ShopeeManagement.API.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AllowNoTenantAttribute : Attribute
{
}