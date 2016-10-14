<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<WBSSLStore.Web.Helpers.PopupForm.ConfirmInfo>" %>
<%@ Import Namespace="Omu.Awesome.Mvc" %>
<%
    var o = Model.CssClass; %>
<script type="text/javascript">
        var currentForm<%=Model.CssClass %>;      
        $(function () {
            $ae.confirm('<%=o %>', currentForm<%=Model.CssClass %>, <%=Model.Height %>, <%=Model.Width %>, '<%= WBSSLStore.Web.Util.WBSiteTools.JsEncode(Model.YesText) %>', '<%= WBSSLStore.Web.Util.WBSiteTools.JsEncode(Model.NoText) %>');
        });
</script>
<div id="dialog-confirm-<%=o %>" title="<%=Model.Title %>">
    <%=Model.Message %>
</div>
