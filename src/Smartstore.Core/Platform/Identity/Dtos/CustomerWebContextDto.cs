using System.Runtime.CompilerServices;
using Smartstore.Core.Identity;
using Smartstore.Extensions;

namespace Smartstore.Core.Platform.Identity.Dtos;

public class CustomerWebContextDto
{
    public int Id { get; set; }
    public Guid CustomerGuid { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public bool Active { get; set; }
    private string LanguageIdString { get; set; }
    public int? LanguageId => LanguageIdString.ToSafeInt();

    public IList<CustomerRoleDto> Roles => RolesJson.DeserializeStringToList<CustomerRoleDto>();
    public string RolesJson { get; set; }
}

public class CustomerRoleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SystemName { get; set; }
}

public static class CustomerWebContextDtoExtensions
{
    /// <summary>
    /// Gets a value indicating whether customer is in a certain customer role.
    /// </summary>
    /// <param name="customer">Customer.</param>
    /// <param name="roleSystemName">Customer role system name.</param>
    public static bool IsInRole(this CustomerWebContextDto customer, string roleSystemName)
    {
        Guard.NotNull(customer, nameof(customer));
        Guard.NotEmpty(roleSystemName, nameof(roleSystemName));

        return customer.Roles.Any(v => v.SystemName.EqualsNoCase(roleSystemName));
    }

    /// <summary>
    /// Gets a value indicating whether customer is registered (navigation properties CustomerRoleMappings then CustomerRole are required).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRegistered(this CustomerWebContextDto customer)
    {
        return IsInRole(customer, SystemCustomerRoleNames.Registered);
    }

    /// <summary>
    /// Gets a value indicating whether customer is administrator (navigation properties CustomerRoleMappings then CustomerRole are required).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAdmin(this CustomerWebContextDto customer)
    {
        return IsInRole(customer, SystemCustomerRoleNames.Administrators);
    }
}