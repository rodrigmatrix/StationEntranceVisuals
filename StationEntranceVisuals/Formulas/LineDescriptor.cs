using System;
using Game.Prefabs;
using StationEntranceVisuals.Systems;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

public record struct LineDescriptor(
    Entity Entity,
    TransportType TransportType,
    bool IsCargo,
    bool IsPassenger,
    string Acronym,
    int Number,
    string SmallName,
    UnityEngine.Color Color)
{

    public string GetDisplayName()
    {
        return SEV_SettingSystem.Instance.LineDisplayName switch
        {
            Settings.LineDisplayNameOptions.Custom => SmallName,
            Settings.LineDisplayNameOptions.WriteEverywhere => Acronym,
            Settings.LineDisplayNameOptions.Generated => Number.ToString(),
            _ => SmallName
        };
    }
    
    public string GetOrderingIndex()
    {
        return SEV_SettingSystem.Instance.LineDisplayName switch
        {
            Settings.LineDisplayNameOptions.Custom => SmallName,
            Settings.LineDisplayNameOptions.WriteEverywhere => Acronym,
            Settings.LineDisplayNameOptions.Generated => Number.ToString(),
            _ => SmallName
        };
    }
}