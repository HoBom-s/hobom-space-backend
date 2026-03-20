namespace HobomSpace.Api.Contracts;

/// <summary>라벨 생성 요청 DTO.</summary>
public record CreateLabelRequest(string Name, string Color);

/// <summary>라벨 수정 요청 DTO.</summary>
public record UpdateLabelRequest(string Name, string Color);

/// <summary>라벨 응답 DTO.</summary>
public record LabelResponse(long Id, long SpaceId, string Name, string Color, DateTime CreatedAt);

/// <summary>페이지에 라벨 부착 요청 DTO.</summary>
public record AddPageLabelRequest(long LabelId);
