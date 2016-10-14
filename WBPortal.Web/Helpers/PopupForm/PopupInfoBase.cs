namespace WBSSLStore.Web.Helpers.PopupForm
{
    public abstract class PopupInfoBase
    {
        protected PopupInfoBase()
        {
            Parameters = new string[] { };
        }

        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public string Title { get; set; }
        public string Position { get; set; }
        public bool Modal { get; set; }
        public bool Resizable { get; set; }
        public string[] Parameters { get; set; }
        public bool FullScreen { get; set; }
        public string Prefix { get; set; }
    }
}