namespace Membr.Module.Member.Application.Handlers.Udf;

using Domain;

internal sealed record MemberUdfValueDto(int MemberId, int UdfDefinitionId, string? Value)
{
    public static MemberUdfValueDto FromEntity(MemberUdfValue v) => new(v.MemberId, v.UdfDefinitionId, v.Value);
}

internal sealed record MemberSummaryDto(int Id, string FirstName, string Surname)
{
    public static MemberSummaryDto FromEntity(Member m) => new(m.Id, m.FirstName, m.Surname);
}

internal sealed record UdfValuesGridDto(
    List<UdfDefinitionDto> Definitions,
    List<MemberSummaryDto> Members,
    List<MemberUdfValueDto> Values);

internal sealed record MemberUdfFieldDto(
    int DefinitionId,
    string Name,
    UdfFieldType Type,
    List<string> Options,
    string? Value);
