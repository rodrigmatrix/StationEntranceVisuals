﻿using System.Collections.Generic;
using System.Linq;
using Game.Prefabs;
using Unity.Entities;

namespace StationEntranceVisuals.Formulas;

public class DisplaySettings
{
    private const string Square = "LineBgSquare";
    private const string Circle = "LineBgCircle";
    private const string ViaMobilidadeOperator = "ViaMobilidadeOperator";
    private const string CptmOperator = "CptmOperator";
    private const string MetroOperator = "MetroOperator";
    private const string LinhaUniOperator = "LinhaUniOperator";
    private const string GenericSubwayOperator = "GenericSubwayOperator";
    private const string GenericTrainOperator = "GenericTrainOperator";
    private const string Black = "Black";
    private const string Transparent = "Transparent";
    private const string WheelchairVar = "wheelchair";
    private static readonly string[] ViaMobilidadeLines = ["4", "5", "8", "9", "15"];
    private static readonly string[] LinhaUniLines = ["6"];

    public static string GetShapeIcon(Entity buildingRef)
    {
        return Mod.m_Setting.LineIndicatorShapeDropdown switch
        {
            Settings.LineIndicatorShapeOptions.Square => Square,
            Settings.LineIndicatorShapeOptions.Circle => Circle,
            _ => Square
        };
    }
    
    public static int ShowWheelchairIcon(Entity buildingRef, Dictionary<string, string> vars)
    {
        return vars.TryGetValue(WheelchairVar, out _) ? 1 : 0;
    }
    
    public static string GetWheelchairIcon(Entity buildingRef)
    {
        return Mod.m_Setting.LineIndicatorShapeDropdown switch
        {
            Settings.LineIndicatorShapeOptions.Square => "WheelchairSquare",
            Settings.LineIndicatorShapeOptions.Circle => "WheelchairCircle",
            _ => "WheelchairSquare"
        };
    }
    
    public static string GetSubwayOperatorIcon(Entity buildingRef, Dictionary<string, string> vars)
    {
        var lines = LinesUtils.GetFilteredLinesList(buildingRef, "Subway", true);
        if (lines.Count < 1)
        {
            return Transparent;
        }
        return Mod.m_Setting.LineOperatorCityDropdown switch
        {
            Settings.LineOperatorCityOptions.Generic => GenericSubwayOperator + Black,
            Settings.LineOperatorCityOptions.SaoPaulo => GetSaoPauloSubwayOperatorIcon(lines),
            Settings.LineOperatorCityOptions.NewYork => GetNewYorkSubwayOperatorIcon(lines),
            Settings.LineOperatorCityOptions.London => GetLondonSubwayOperatorIcon(lines),
            _ => GenericSubwayOperator
        };
    }
    
    public static string GetTrainOperatorIcon(Entity buildingRef, Dictionary<string, string> vars)
    {
        var lines = LinesUtils.GetFilteredLinesList(buildingRef, "Train", true);
        if (lines.Count < 1)
        {
            return Transparent;
        }
        return Mod.m_Setting.LineOperatorCityDropdown switch
        {
            Settings.LineOperatorCityOptions.Generic => GenericTrainOperator + Black,
            Settings.LineOperatorCityOptions.SaoPaulo => GetSaoPauloTrainOperatorIcon(lines),
            _ => GenericTrainOperator + Black
        };
    }

    private static string GetSaoPauloSubwayOperatorIcon(HashSet<LineDescriptor> lines)
    {
        if (lines.Any(x => ViaMobilidadeLines.Where(y => y == x.GetDisplayName()).ToList().Count > 0))
        {
            return ViaMobilidadeOperator + Black;
        }

        if (lines.Any(x => LinhaUniLines.Where(y => y == x.GetDisplayName()).ToList().Count > 0))
        {
            return LinhaUniOperator + Black;
        }

        return MetroOperator + Black;
    }
    
    private static string GetSaoPauloTrainOperatorIcon(HashSet<LineDescriptor> lines)
    {
        if (lines.Any(x => ViaMobilidadeLines.Where(y => y == x.GetDisplayName()).ToList().Count > 0))
        {
            return ViaMobilidadeOperator + Black;
        }

        return CptmOperator + Black;
    }
    
    private static string GetNewYorkSubwayOperatorIcon(HashSet<LineDescriptor> lines)
    {
        return "MTAOperator";
    }
    
    private static string GetLondonSubwayOperatorIcon(HashSet<LineDescriptor> lines)
    {
        return "UndergroundOperator";
    }
}