using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using HeatLossCalc.Core;
using HeatLossCalc.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using static HeatLossCalc.Models.UtilsDirection;

namespace HeatLossCalc.Models
{
    public class Ray
    {
        public Space SpaceSource { get; }
        public Space SpaceFinish { get; }
        public Line Line { get; }
        public Direction Direction { get ; }
        public HeatLossSettings HTSettings { get; }

        public List<Element> _elements = new List<Element>();
        public List<Element> Elements { get { return _elements; } }
        public List<ElementFict> ElementsFict { get; set; } = new List<ElementFict>();
        public List<ReferenceWithContext> Context { get; set; } = new List<ReferenceWithContext>();
        public Ray(Space spaceSource, Space spaceFinish, Line line, List<ReferenceWithContext> context, HeatLossSettings settings)
        {
            HTSettings = settings;
            SpaceSource = spaceSource;
            SpaceFinish = spaceFinish;
            Line = line;
            if (!HTSettings.BoundarySpace.Contains(SpaceFinish.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()))
                Direction = UtilsDirection.GetDirection(Line,true);
            else
                Direction = UtilsDirection.GetDirection(Line);
            Context = context;
            ConvertContextToElements();
        }
        private void ConvertContextToElements()
        {
            foreach (var context in Context)
            {
                ElementId id1 = context.GetReference().ElementId;
                Element el = RevitApi.Doc.GetElement(id1);
                if (context.Proximity < Line.Length)
                {
                    if (el is RevitLinkInstance)
                    {
                        Document docLink = ((RevitLinkInstance)el).GetLinkDocument();
                        Element elLink = docLink.GetElement(context.GetReference().LinkedElementId);
                        AddElement(elLink);
                    }
                    else
                    {
                        AddElement(el);
                    }
                }
            }
        }
        private bool AddElement(Element element)
        {
            if (!_elements.Select(x => x.Id.IntegerValue).ToList().Contains(element.Id.IntegerValue))
            {
                _elements.Add(element);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void UpdateElementFict (HeatLossSettings settings)
        {
            ElementsFict = new List<ElementFict>();
            Elements.ForEach(x => ElementsFict.Add(ConvertElementToElementFict(x)));
            foreach (string category in settings.CategoriesPriority)
            {
                ElementsFict = ApplyPriority(ElementsFict, category);
            }
            foreach (RenameElement rename in settings.RenameElements)
            {
                ChangeSymbol(ElementsFict, rename);
            }
            ElementsFict = ElementsFict.Distinct(new CompareElementFict()).ToList();
        }
        private  ElementFict ConvertElementToElementFict(Element element)
        {
            ElementFict elementFict = new ElementFict();
            elementFict.SymbolName = element.Name;
            elementFict.CategoryName = element.Category.Name;
            return elementFict;
        }
        private  List<ElementFict> ApplyPriority(List<ElementFict> list, string category)
        {
            if (list.Where(x => x.CategoryName == category).Count() > 0)
            {
                return  list.Where(x => x.CategoryName == category).ToList();
            }
            else { return list; }
        }
        private void ChangeSymbol(List<ElementFict> list, RenameElement rename)
        {
            foreach (ElementFict f in list)
            {
                if (rename.OldSymbolName == f.SymbolName && rename.OldCategoryName == f.CategoryName)
                {
                    f.SymbolName = rename.NewSymbolName;
                    f.CategoryName = rename.NewCategoryName;
                }
            }
        }
    }
}
