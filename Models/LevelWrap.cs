using Autodesk.Revit.DB;

namespace HeatLossCalc.Models
{
    public class LevelWrap
    {
        private Level _level;

        public Level Level { get { return _level; } }
        public bool IsChecked { get; set; } = false;

        public LevelWrap(Level level) 
        {
            _level = level;
        }
    }
}