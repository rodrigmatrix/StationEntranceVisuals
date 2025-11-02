using Colossal.Serialization.Entities;
using Unity.Entities;
using static StationEntranceVisuals.Settings;

namespace StationEntranceVisuals.Systems
{
    public partial class SEV_SettingSystem : SystemBase, IDefaultSerializable
    {
        private const uint CURRENT_VERSION = 0;

        public static SEV_SettingSystem Instance { get; private set; }

        protected override void OnCreate()
        {
            base.OnCreate();
            Instance = this;
        }
        public LineIndicatorShapeOptions SubwayLineIndicatorShape { get; set; } = LineIndicatorShapeOptions.Square;
        public LineOperatorCityOptions LineOperatorCity { get; set; } = LineOperatorCityOptions.Generic;
        public LineDisplayNameOptions LineDisplayName { get; set; } = LineDisplayNameOptions.Custom;
        public LineIndicatorShapeOptions TrainLineIndicatorShape { get; set; } = LineIndicatorShapeOptions.Square;
        public LineIndicatorShapeOptions BusLineIndicatorShape { get; set; } = LineIndicatorShapeOptions.Diamond;
        public LineIndicatorShapeOptions TramLineIndicatorShape { get; set; } = LineIndicatorShapeOptions.Pentagon;

        public void Deserialize<TReader>(TReader reader) where TReader : IReader
        {
            reader.Read(out uint version);
            if (version > CURRENT_VERSION)
            {
                throw new System.Exception($"Unsupported version {version} for {nameof(SEV_SettingSystem)}");
            }
            reader.Read(out int lineIndicatorShape);
            SubwayLineIndicatorShape = (LineIndicatorShapeOptions)lineIndicatorShape;
            reader.Read(out int trainShapeDropdown);
            TrainLineIndicatorShape = (LineIndicatorShapeOptions)trainShapeDropdown;
            reader.Read(out int busShapeDropdown);
            BusLineIndicatorShape = (LineIndicatorShapeOptions)busShapeDropdown;
            reader.Read(out int tramShapeDropdown);
            TramLineIndicatorShape = (LineIndicatorShapeOptions)tramShapeDropdown;
            reader.Read(out int lineOperatorCity);
            LineOperatorCity = (LineOperatorCityOptions)lineOperatorCity;
            reader.Read(out int lineDisplayName);
            LineDisplayName = (LineDisplayNameOptions)lineDisplayName;

        }

        public void Serialize<TWriter>(TWriter writer) where TWriter : IWriter
        {
            writer.Write(CURRENT_VERSION);
            writer.Write((int)SubwayLineIndicatorShape);
            writer.Write((int)TrainLineIndicatorShape);
            writer.Write((int)BusLineIndicatorShape);
            writer.Write((int)TramLineIndicatorShape);
            writer.Write((int)LineOperatorCity);
            writer.Write((int)LineDisplayName);
        }

        public void SetDefaults(Context context)
        {
            SubwayLineIndicatorShape = Mod.m_Setting.LineIndicatorShapeDropdown;
            TrainLineIndicatorShape = Mod.m_Setting.TrainShapeDropdown;
            BusLineIndicatorShape = Mod.m_Setting.BusShapeDropdown;
            TramLineIndicatorShape = Mod.m_Setting.TramShapeDropdown;
            LineOperatorCity = Mod.m_Setting.LineOperatorCityDropdown;
            LineDisplayName = Mod.m_Setting.LineDisplayNameDropdown;
        }

        protected override void OnUpdate()
        {
        }
    }
}