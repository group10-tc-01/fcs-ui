namespace Fcs.UI.Application.Interfaces;

public interface ICacheService
{
    Task SaveAsync<T>(string key, T data);
    Task<T?> GetAsync<T>(string key) where T : class;
    Task ClearAsync();
}
