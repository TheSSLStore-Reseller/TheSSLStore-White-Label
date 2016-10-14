using System;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    
    public static class Settings
    {
        public static string AwesomeFolder = @"~\Views\Shared\PopupForm\";
        public static Func<string, string, string> GetText; 

        public static class Autocomplete
        {
            public static int MaxResults = 10;
            public static string IdFormat = "{0}Id";
            public static int Delay = 300;
            public static int MinLength = 1;
        }

        public static class Lookup
        {
            public static string SelectedRowCssClass = "ui-state-error";
            public static int Width = 750;
            public static int Height = 400;
            public static bool ClearButton;
            public static string ChooseText = "OK";
            public static string CancelText = "Cancel";
            public static string Title = "please select";
            public static bool Multiselect;
            public static bool Fullscreen;
            public static bool Paging;
            public static bool Interactive;
        }

        public static class Confirm
        {
            public static string CssClass = "confirm";
            public static string Title = "confirm dialog";
            public static int Width = 500;
            public static int Height = 250;
            public static string YesText = "Yes";
            public static string NoText = "Cancel";
        }

        public static class PopupForm
        {
            public static bool FullScreen;
            public static int Width = 700;
            public static int Height = 330;
            public static bool RefreshOnSuccess;
            public static string OkText = "OK";
            public static string CancelText = "Cancel";
            public static bool ClientSideValidation;
            public static bool Resizable = true;
            public static bool Modal = true;
        } 
        
        public static class Popup
        {
            public static int Width = 700;
            public static int Height = 330;
            public static bool Resizable = true;
            public static bool Modal;
            public static bool FullScreen;
        }
    }
}