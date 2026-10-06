namespace IdleTerraria.Api.DTOs.Responses;

public sealed class HuntingLootResponse
{
    public int ItemTemplateId { get; init; }

    public string ItemCode { get; init; } = string.Empty;

    public string ItemName { get; init; } = string.Empty;

    public int Quantity { get; init; }
}