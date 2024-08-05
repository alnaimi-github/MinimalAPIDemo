using System.Net;

namespace MinimalAPIDemo.Shared
{
    public record APIResponse(
        bool IsSuccess=true, object Result=null!,
        HttpStatusCode StatusCode=default,
        List<string> ErrorMessages=null!);
}
