using Radio.Player.Models;

namespace Radio.Player.Services.Contracts;

public interface IArticlesService
{
    Task<FullArticle> GetFullArticleAsync(string articleUrl, CancellationToken cancellationToken = default);
}