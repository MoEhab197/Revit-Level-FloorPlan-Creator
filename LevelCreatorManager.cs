using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace LevelCreator
{
    [Transaction(TransactionMode.Manual)]
    public class LevelCreatorManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,ref string message,ElementSet elemets)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            using (Transaction tx = new Transaction(doc, "Create Levels and Floor Plans"))
            {
                tx.Start();
                //Validation for already existing levels 

                FilteredElementCollector levelCollector = new FilteredElementCollector(doc).OfClass(typeof(Level));
                List<string> existinglevelNames = levelCollector.Select(l => l.Name).ToList();

                //Creation of levels and floor plans
                for (int i = 1;i<=100;i++)
                {
                    string levelName = $"KAITECH - Level{i}";
                    if (existinglevelNames.Contains(levelName))
                    {
                        message = "Some levels with the target names already exist.";
                        return Result.Failed;
                    }
                    //Create Level
                    double levelElevation = (i-1) * 3 * 3.28084; // convert between feet & meter
                    Level newlevel = Level.Create(doc,levelElevation);
                    newlevel.Name = levelName;

                    //Create Floor Plan View
                    ViewFamilyType planType = new FilteredElementCollector(doc)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .FirstOrDefault(vft => vft.ViewFamily == ViewFamily.FloorPlan);

                    if (planType != null)
                    {
                        ViewPlan newPlan = ViewPlan.Create(doc, planType.Id, newlevel.Id);
                        newPlan.Name = levelName;
                    }

                }
                tx.Commit();
            }
            return Result.Succeeded;
        }      
    }
}
