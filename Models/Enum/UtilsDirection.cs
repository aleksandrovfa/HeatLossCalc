using Autodesk.Revit.DB;
using HeatLossCalc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static HeatLossCalc.Models.UtilsDirection;

namespace HeatLossCalc.Models
{
    public class UtilsDirection
    {
        public enum Direction
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest,
            Up,
            Down,
            Inside
        }

        public static Direction GetDirection(Line line, bool isInside = false)
        {
            XYZ direction = line.Direction;
            if (direction.Z == 1)
                return Direction.Up;

            if (direction.Z == -1)
                return Direction.Down;

            if (isInside)
                return Direction.Inside;
            var projectLocation = RevitApi.Doc.ActiveProjectLocation;
            ProjectPosition projectPosition = projectLocation.GetProjectPosition(new XYZ(0, 0, 0));
            Transform rotationTransform = Transform.CreateRotation(
            new XYZ(0, 0, 1), projectPosition.Angle);

            XYZ translationVector = new XYZ(projectPosition.EastWest, projectPosition.NorthSouth, projectPosition.Elevation);
            Transform translationTransform = Transform.CreateTranslation(translationVector);
            Transform finalTransform = translationTransform.Multiply(rotationTransform);

            XYZ pointTrueNorth = finalTransform.OfVector(new XYZ(0, 1, 0));

            double angle = pointTrueNorth.AngleOnPlaneTo(direction, new XYZ(0, 0, 1)) * 180 / Math.PI;

            //double angle = Math.Atan2(direction.Y, direction.X) * 180 / Math.PI;
            if (angle < 0)
                angle += 360;
            if (angle >= 337.5 || angle < 22.5)
                return Direction.East;
            else if (angle < 67.5)
                return Direction.NorthEast;
            else if (angle < 112.5)
                return Direction.North;
            else if (angle < 157.5)
                return Direction.NorthWest;
            else if (angle < 202.5)
                return Direction.West;
            else if (angle < 247.5)
                return Direction.SouthWest;
            else if (angle < 292.5)
                return Direction.South;
            else
                return Direction.SouthEast;
        }
    }
    public static class DirectionExtension
    {
        public static string ToStringRu(this Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return "Север";
                case Direction.NorthEast:
                    return "Северо-Восток";
                case Direction.East:
                    return "Восток";
                case Direction.SouthEast:
                    return "Юго-Восток";
                case Direction.South:
                    return "Юг";
                case Direction.SouthWest:
                    return "Юго-Запад";
                case Direction.West:
                    return "Запад";
                case Direction.NorthWest:
                    return "Северо-Запад";
                case Direction.Up:
                    return "Вверх";
                case Direction.Down:
                    return "Вниз";
                case Direction.Inside:
                    return "Внутри";
                default:
                    return direction.ToString();
            }
        }
    }
}
