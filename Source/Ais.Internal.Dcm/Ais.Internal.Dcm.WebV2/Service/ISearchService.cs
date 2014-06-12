using Ais.Internal.Dcm.Web.Models;

namespace Ais.Internal.Dcm.Web.Service
{
    public interface ISearchService
    {
        SearchData SearchMedia(string searchParam, int rowsToSkip, int rowsToRetrieve, SearchType searchType);
        void InsertMediaHistory(SearchResultViewModel model);
    }

    public enum SearchType
    {
        FreeText,
        TagSearch
    }
}
