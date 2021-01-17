using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface IArticlesService
    {
        Task<FullArticle> GetFullArticleAsync(string articleUrl);
    }
}