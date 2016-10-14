namespace WBSSLStore.Web.Helpers.PopupForm
{
    public class PopupFormInfo : PopupInfoBase
    {
        public string CancelText { get; set; }
        public string OkText { get; set; }
        public bool RefreshOnSuccess { get; set; }
        public bool ClientSideValidation { get; set; }
        public string SuccessFunction { get; set; }
    }
}