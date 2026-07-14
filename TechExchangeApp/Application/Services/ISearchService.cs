using TechExchangeApp.Application.DTOs;

namespace TechExchangeApp.Application.Services
{
    /// <summary>
    /// Search service interface for normal and AI search
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Perform normal FullText search
        /// </summary>
        Task<SearchResult> SearchNormalAsync(
            string keyword, 
            SearchOptions options, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform AI-enhanced semantic search
        /// </summary>
        Task<SearchResult> SearchAIAsync(
            string keyword, 
            SearchOptions options, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Perform AI-enhanced semantic search with results grouped by company/organization
        /// </summary>
        Task<List<ViewModel.AISearchResultGroup>> SearchAIGroupedAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get autocomplete suggestions
        /// </summary>
        Task<List<SearchSuggestion>> GetSuggestionsAsync(
            string prefix, 
            string? languageId = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get trending searches
        /// </summary>
        Task<List<TrendingSearch>> GetTrendingSearchesAsync(
            int days = 7, 
            int topN = 10, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get result counts grouped by TypeName for filter tabs
        /// </summary>
        Task<Dictionary<string, int>> GetCountsByTypeAsync(
            string keyword,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Search with TypeName filter using inline SQL (bypasses SP).
        /// </summary>
        Task<SearchResult> SearchByTypeAsync(
            string keyword,
            SearchOptions options,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Full-text search where each entry in <paramref name="phrases"/> is OR-combined as its
        /// own quoted CONTAINSTABLE phrase, so a multi-word entry like "chuyên gia" must appear
        /// as an adjacent phrase. Unlike SearchByTypeAsync (which ANDs individual words — "chuyên"
        /// AND "gia" — and can match unrelated documents that merely contain both syllables
        /// somewhere), this keeps compound Vietnamese terms intact. Used by the AI chatbox.
        /// </summary>
        Task<SearchResult> SearchByPhrasesAsync(
            IReadOnlyList<string> phrases,
            int take,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Plain category browse — no CONTAINSTABLE, no ranking. Used when the caller's intent
        /// is unambiguous (e.g. a chatbox quick-action button like "Tìm công nghệ"), where
        /// running a fuzzy text search would be both slower and less correct than just listing
        /// the most recent items of that type.
        /// </summary>
        Task<SearchResult> GetRecentByTypeAsync(
            IReadOnlyList<string> typeNames,
            int take,
            CancellationToken cancellationToken = default);
    }
}
