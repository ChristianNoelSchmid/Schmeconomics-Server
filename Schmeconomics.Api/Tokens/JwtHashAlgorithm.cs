using System.Runtime.CompilerServices;
using Microsoft.IdentityModel.Tokens;

namespace Schmeconomics.Api.Tokens;

public enum JwtHashAlgorithm
{
    HmacSha256,
    HmacSha384,
    HmacSha512,
}

public static class JwtHashAlgorithmExtensions
{
    public static string ToSecurityAlgorithmString(this JwtHashAlgorithm alg) => alg switch
    {
        JwtHashAlgorithm.HmacSha256        => SecurityAlgorithms.HmacSha256,
        JwtHashAlgorithm.HmacSha384        => SecurityAlgorithms.HmacSha384,
        /* JwtHashAlgorithm.HmacSha512*/ _ => SecurityAlgorithms.HmacSha512,
    };
}