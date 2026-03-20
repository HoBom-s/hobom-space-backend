using HobomSpace.Domain.Entities;

namespace HobomSpace.Application.Ports;

/// <summary>Page 엔티티의 영속성 포트. soft delete 관련 메서드를 포함한다.</summary>
public interface IPageRepository
{
    /// <summary>ID로 페이지를 조회한다 (soft delete 제외).</summary>
    Task<Page?> GetByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Space에 속한 모든 페이지를 조회한다.</summary>
    Task<List<Page>> GetBySpaceIdAsync(long spaceId, CancellationToken ct = default);

    /// <summary>전문 검색으로 페이지를 조회한다.</summary>
    Task<List<Page>> SearchAsync(string query, int offset, int limit, CancellationToken ct = default);

    /// <summary>전문 검색 결과 수를 반환한다.</summary>
    Task<int> SearchCountAsync(string query, CancellationToken ct = default);

    /// <summary>Space 내에서 전문 검색으로 페이지를 조회한다.</summary>
    Task<List<Page>> SearchBySpaceIdAsync(long spaceId, string query, int offset, int limit, CancellationToken ct = default);

    /// <summary>Space 내 전문 검색 결과 수를 반환한다.</summary>
    Task<int> SearchBySpaceIdCountAsync(long spaceId, string query, CancellationToken ct = default);

    /// <summary>새 페이지를 추가한다.</summary>
    Task AddAsync(Page page, CancellationToken ct = default);

    /// <summary>페이지를 삭제 대상으로 표시한다.</summary>
    void Remove(Page page);

    /// <summary>soft-deleted 페이지를 ID로 조회한다 (QueryFilter 무시).</summary>
    Task<Page?> GetDeletedByIdAsync(long id, CancellationToken ct = default);

    /// <summary>Space의 soft-deleted 페이지 목록을 조회한다.</summary>
    Task<List<Page>> GetDeletedBySpaceIdAsync(long spaceId, int offset, int limit, CancellationToken ct = default);

    /// <summary>Space의 soft-deleted 페이지 수를 반환한다.</summary>
    Task<int> CountDeletedBySpaceIdAsync(long spaceId, CancellationToken ct = default);

    /// <summary>cutoff 이전에 삭제된 페이지를 배치 단위로 영구 삭제한다.</summary>
    Task<int> PurgeDeletedOlderThanAsync(DateTime cutoff, int batchSize, CancellationToken ct = default);
}
