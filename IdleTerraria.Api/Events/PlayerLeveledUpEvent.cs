using MediatR;
using IdleTerraria.Api.Entities;

namespace IdleTerraria.Api.Events
{
    public record PlayerLeveledUpEvent(Player Player, int NewLevel) : INotification;
}