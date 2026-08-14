using System.Threading;
using System.Windows.Threading;

namespace FileVisor.CustomDialogBox
{
    public static class DialogBox
    {
        public enum DialogBoxType
        {
            Message, Question, Warning, Error
        }

        public enum DialogBoxButtons
        {
            OK, OKCancel, YesNo, YesNoCancel
        }

        public enum DialogBoxResult
        {
            None, OK, Cancel, Yes, No
        }

        public static DialogBoxResult ShowDialogBox(string text, string title, DialogBoxType type, DialogBoxButtons buttons)
        {
            bool isOpened = true;
            DialogBoxResult dialogBoxResult = DialogBoxResult.None;

            Thread thread = new Thread(() =>
            {
                DialogWindow dialogWindow = new DialogWindow(text, title, type, buttons);
                dialogWindow.Show();

                dialogWindow.Closed += (sender2, e2) =>
                {
                    isOpened = false;
                    dialogBoxResult = dialogWindow.result;
                    dialogWindow.Dispatcher.InvokeShutdown();
                };

                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            while (isOpened)
                Thread.Sleep(10);

            return dialogBoxResult;
        }
    }
}