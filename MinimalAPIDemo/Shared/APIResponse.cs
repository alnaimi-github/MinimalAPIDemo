using System.Net;

namespace MinimalAPIDemo.Shared
{
    public record APIResponse(
        bool IsSuccess, object Result=null!,
        HttpStatusCode StatusCode=default,
        List<string> ErrorMessages=null!);
}
