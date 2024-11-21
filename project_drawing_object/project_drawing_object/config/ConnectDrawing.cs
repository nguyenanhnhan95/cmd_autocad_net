using Autodesk.AutoCAD.ApplicationServices;


namespace project_drawing_object.config
{
    public class ConnectDrawing 
    {
        public static readonly Document DocumentAutoCad= Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
        
    }
}
