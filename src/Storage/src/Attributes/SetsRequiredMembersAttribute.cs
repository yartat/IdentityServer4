#if NET7_0_OR_GREATER
#else

namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor)]
public class SetsRequiredMembersAttribute : Attribute
{
}
#endif