using System.Text.Json;
using Membr.Module.Member.Application.Handlers.Udf;
using Membr.Module.Member.Domain;

namespace Membr.Module.Member.Tests.Application.Udf;

public class UdfDefinitionHandlerTests
{
    private static Membr.Module.Member.Domain.Member NewMember(string firstName) =>
        new() { FirstName = firstName, Surname = "Test", DateOfBirth = new DateOnly(1990, 1, 1) };

    [Fact]
    public async Task CreateUdfDefinition_BackfillsDefaultValueOntoExistingMembers()
    {
        await using var db = TestDbContextFactory.Create();
        var member1 = NewMember("Ada");
        var member2 = NewMember("Grace");
        db.Members.AddRange(member1, member2);
        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new CreateUdfDefinitionHandler(db);
        var result = await handler.Handle(
            new CreateUdfDefinitionRequest("Newsletter Opt-in", UdfFieldType.Bool, null, "true"), CancellationToken.None);

        Assert.NotNull(result.Definition);
        var values = db.MemberUdfValues.ToList();
        Assert.Equal(2, values.Count);
        Assert.All(values, v => Assert.Equal("true", v.Value));
    }

    [Fact]
    public async Task CreateUdfDefinition_RejectsMultiSelectWithoutOptions()
    {
        await using var db = TestDbContextFactory.Create();
        var handler = new CreateUdfDefinitionHandler(db);

        var result = await handler.Handle(
            new CreateUdfDefinitionRequest("Interests", UdfFieldType.MultiSelect, [], null), CancellationToken.None);

        Assert.Null(result.Definition);
        Assert.NotNull(result.Error);
    }

    private static readonly string[] valueArray = ["Chess", "Running"];

    [Fact]
    public async Task UpdateUdfDefinition_RemovingOption_StripsItFromExistingMemberValues()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember("Ada");
        db.Members.Add(member);
        await db.SaveChangesAsync(CancellationToken.None);

        var createHandler = new CreateUdfDefinitionHandler(db);
        var created = await createHandler.Handle(
            new CreateUdfDefinitionRequest("Interests", UdfFieldType.MultiSelect, ["Chess", "Running"], null),
            CancellationToken.None);

        var updateValueHandler = new UpdateMemberUdfValueHandler(db);
        await updateValueHandler.Handle(
            member.Id, created.Definition!.Id, JsonSerializer.Serialize(valueArray), CancellationToken.None);

        var updateHandler = new UpdateUdfDefinitionHandler(db);
        var updateResult = await updateHandler.Handle(
            created.Definition.Id,
            new UpdateUdfDefinitionRequest("Interests", ["Chess"], null, true),
            CancellationToken.None);

        Assert.NotNull(updateResult.Definition);
        var value = db.MemberUdfValues.Single(v => v.MemberId == member.Id);
        var selected = JsonSerializer.Deserialize<List<string>>(value.Value!);
        Assert.Equal(["Chess"], selected);
    }

    [Fact]
    public async Task ApplyDefaultToAllMembers_OverwritesEditedValuesAndFillsMissingMembers()
    {
        await using var db = TestDbContextFactory.Create();
        var member1 = NewMember("Ada");
        db.Members.Add(member1);
        await db.SaveChangesAsync(CancellationToken.None);

        var createHandler = new CreateUdfDefinitionHandler(db);
        var created = await createHandler.Handle(
            new CreateUdfDefinitionRequest("Notes", UdfFieldType.String, null, "default note"), CancellationToken.None);

        var updateValueHandler = new UpdateMemberUdfValueHandler(db);
        await updateValueHandler.Handle(member1.Id, created.Definition!.Id, "edited note", CancellationToken.None);

        var member2 = NewMember("Grace");
        db.Members.Add(member2);
        await db.SaveChangesAsync(CancellationToken.None);

        var applyHandler = new ApplyDefaultToAllMembersHandler(db);
        await applyHandler.Handle(created.Definition.Id, CancellationToken.None);

        var values = db.MemberUdfValues.ToList();
        Assert.Equal(2, values.Count);
        Assert.All(values, v => Assert.Equal("default note", v.Value));
    }

    private static readonly string[] value = ["Running"];

    [Fact]
    public async Task UpdateMemberUdfValue_RejectsSelectionNotInOptions()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember("Ada");
        db.Members.Add(member);
        await db.SaveChangesAsync(CancellationToken.None);

        var createHandler = new CreateUdfDefinitionHandler(db);
        var created = await createHandler.Handle(
            new CreateUdfDefinitionRequest("Interests", UdfFieldType.MultiSelect, ["Chess"], null), CancellationToken.None);

        var updateValueHandler = new UpdateMemberUdfValueHandler(db);
        var result = await updateValueHandler.Handle(
            member.Id, created.Definition!.Id, JsonSerializer.Serialize(value), CancellationToken.None);

        Assert.Equal(UpdateMemberUdfValueStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task UpdateMemberUdfValue_RejectsNonBooleanValueForBoolField()
    {
        await using var db = TestDbContextFactory.Create();
        var member = NewMember("Ada");
        db.Members.Add(member);
        await db.SaveChangesAsync(CancellationToken.None);

        var createHandler = new CreateUdfDefinitionHandler(db);
        var created = await createHandler.Handle(
            new CreateUdfDefinitionRequest("Newsletter", UdfFieldType.Bool, null, null), CancellationToken.None);

        var updateValueHandler = new UpdateMemberUdfValueHandler(db);
        var result = await updateValueHandler.Handle(member.Id, created.Definition!.Id, "maybe", CancellationToken.None);

        Assert.Equal(UpdateMemberUdfValueStatus.Invalid, result.Status);
    }
}
