using System.Reflection;
using Realms;

namespace CacheHandlerPlugin.Realm.Extensions
{
    internal static class ReflectionExtensions
    {
        public static string GetMappedOrOriginalName(this MemberInfo member) => member?.GetCustomAttribute<MapToAttribute>()?.Mapping ?? member?.Name;

    }
}
