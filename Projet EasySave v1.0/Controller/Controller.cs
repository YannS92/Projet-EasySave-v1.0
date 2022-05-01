using System;
using System.Collections.Generic;
using System.Text;

namespace Projet_EasySave_v1._0
{
    class Controller
    {

        public Controller(Model model, View view)
        {

            Model = model;
            View = view;

        }

        private Model model;

        public Model Model
        {
            get { return model; }
            set { model = value; }
        }

        private View view;

        public View View
        {
            get { return view; }
            set { view = value; }
        }

        public void Start()
        {
            View.Start();
            ShowMenu();
        }

        private void LaunchSave()
        {
            View.TerminalMessage("launch");
            int saveProcedureIndex = View.SelectSaveProcedure(Model.WorkList);

            if (saveProcedureIndex != 9)
            {
                if (View.Confirm())
                {
                    //To Implement (sauvegarde en cours blablabla)

                    View.SaveInProgressMessage(Model.WorkList[saveProcedureIndex - 1]);
                    Model.DoSave(saveProcedureIndex);
                    View.SaveIsDoneMessage(Model.WorkList[saveProcedureIndex - 1]);
                    //fonction vue pour retour user
                    ShowMenu();
                    return;
                }
                else
                {
                    ShowMenu();
                    return;
                }
            }
            else
            {
                ShowMenu();
                return;
            }
        }

        private void CreateSave()
        {
            string[] saveProcedure = View.CreateSaveProcedure();

            SaveWorkType type = SaveWorkType.unset;
            if (saveProcedure[3] == "1")
            {
                type = SaveWorkType.complete;
            }
            else if (saveProcedure[3] == "2")
            {
                type = SaveWorkType.differencial;
            }
            Model.CreateWork(int.Parse(saveProcedure[4]), saveProcedure[0], saveProcedure[1], saveProcedure[2], type);


            ShowMenu();
            return;
        }

        private void ModifySave()
        {
            View.TerminalMessage("modify");
            int saveProcedureIndex = View.SelectSaveProcedure(Model.WorkList);
            if (saveProcedureIndex != 9)
            {
                SaveWork saveProcedure = View.ModifySaveProcedure(Model.WorkList[saveProcedureIndex - 1]);
                if (saveProcedure != null)
                {
                    Model.ChangeWork(saveProcedureIndex, saveProcedure.Name, saveProcedure.SourcePath, saveProcedure.DestinationPath, saveProcedure.Type);
                    View.OperationDoneMessage();
                }
            }
            ShowMenu();
            return;
        }

        private void DeleteSave()
        {
            View.TerminalMessage("delete");
            int saveProcedureIndex = View.SelectSaveProcedure(Model.WorkList);
            if (saveProcedureIndex != 9)
            {
                if (View.Confirm())
                {
                    Model.DeleteWork(saveProcedureIndex);
                    //TODO: Afficher done delete
                    View.OperationDoneMessage();
                    ShowMenu();
                    return;
                }
                else
                {
                    ShowMenu();
                    return;
                }
            }
            ShowMenu();
            return;
            
        }

        private void LaunchAllSavesSequentially()
        {
            if (View.Confirm())
            {
                for (int i = 1; i < Model.WorkList.Length + 1; i++)
                {
                    View.SaveInProgressMessage(Model.WorkList[i - 1]);
                    Model.DoSave(i);
                    View.SaveIsDoneMessage(Model.WorkList[i - 1]);
                }
                ShowMenu();
                return;
            }
            else
            {
                ShowMenu();
                return;
            }
        }

        private void ShowMenu()
        {
            switch (View.ShowMainMenu())
            {
                case "1":
                    LaunchSave();
                    break;
                case "2":
                    CreateSave();
                    break;
                case "3":
                    ModifySave();
                    break;
                case "4":
                    DeleteSave();
                    break;
                case "5":
                    LaunchAllSavesSequentially();
                    break;
                default:
                    break;
            }
            
        }
    }
}
