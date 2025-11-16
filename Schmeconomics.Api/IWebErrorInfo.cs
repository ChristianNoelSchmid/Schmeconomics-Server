using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Schmeconomics.Api;

public interface IWebErrorInfo
{
    int StatusCode { get; }
    string? ServerMessage => null;
    string ClientMessage => "An internal server error has occurred";
}
