namespace OroIdentityServer.Services.OroIdentityServer.Application.Shared;

public abstract record BaseResponse<T> 
{
    public T Data { get; set; }
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
