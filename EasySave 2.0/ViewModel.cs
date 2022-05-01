using System;
using System.Collections.Generic;
using System.Text;

namespace EasySave_2._0
{
    class ViewModel
    {
        private Model model;

        public Model Model
        {
            get { return model; }
            set { model = value; }
        }

        public ViewModel()
        {
            Model = new Model();
        }


        #region Methodes


        /// Tells the Model to launch a save procedure

        public void LaunchSaveProcedure(int ID)
        {
            Model.DoSave(ID);
        }

        /// Tells the Model to create a save procedure

        public void CreateSaveProcedure(string NameProcedure, string SourcePath, string DestinationPath, SaveWorkType TypeSave, List<Extension> ListExtension)
        {
            if(TypeSave == SaveWorkType.complete)
            {
                Model.CreateCompleteWork(NameProcedure, SourcePath, DestinationPath, ListExtension);
            }
            else
            {
                Model.CreateDifferencialWork(NameProcedure, SourcePath, DestinationPath, ListExtension);
            }
        }

         
        /// Tells the Model to modify a save procedure
         
        public void ModifySaveProcedure(int ID, string NameProcedure, string SourcePath, string DestinationPath, SaveWorkType TypeSave, List<Extension> ListEncrypt)
        {
            Model.ChangeWork(ID, NameProcedure, SourcePath, DestinationPath, TypeSave, ListEncrypt);
        }

         
        /// Tells the Model to delete a save procedure
         
        public void DeleteSaveProcedure(int ID)
        {
            Model.DeleteWork(ID);
        }

         
        /// Tells the Model to launch all save procedures
         
        public void LaunchAllSaveProcedures()
        {
            Model.DoAllSave();
        }

         
        /// Tells the Model to pause the current save procedure(s)
         
        public void PauseSaveProcedure(int Index, bool Boolean)
        {
            if (Boolean)
                Model.PauseSave(Index);
            else
                Model.ResumeSave(Index);
        }

        /// Tells the Model to pause the current save procedure(s)

        public void PauseAllSaveProcedures(bool Boolean)
        {
            if (Boolean)
                Model.PauseAllSave();
            else
                Model.ResumeAllSave();
        }

        /// Tells the Model to cancel the current save procedure(s)

        public void CancelSaveProcedure(int Index)
        {
            Model.CancelSave(Index);
        }

         
        /// Tells the Model to cancel the current save procedure(s)
         
        public void CancelAllSaveProcedures()
        {
            Model.CancelAllSave();
        }

        #endregion

    }
}
