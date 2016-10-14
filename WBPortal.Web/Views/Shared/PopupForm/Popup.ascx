<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<WBSSLStore.Web.Helpers.PopupForm.PopupInfo>" %>
<%
    var o = "p" + Model.Prefix + (Model.Action + Model.Controller + Model.Area).ToLower();
%>

<script type="text/javascript">
        $(function () {$ae.popup('<%=o %>',<%=Model.Width %>, <%=Model.Height %>, '<%=Model.Title %>', <%=Model.Modal.ToString().ToLower() %>, <%=Model.Position %>, <%=Model.Resizable.ToString().ToLower() %>, {<%var i = 0;foreach (var button in Model.Buttons){i++;%>  "<%=button.Key %>" : <%=button.Value %><%=i == Model.Buttons.Count ? "": "," %><%} %>}, <%=Model.FullScreen.ToString().ToLower() %>);});
        var l<%=o %> = null;
        function call<%=o %>(<%=WBSSLStore.Web.Helpers.PopupForm.JsTools.MakeParameters(Model.Parameters) %>) { 
            if(l<%=o %> != null) return;
            l<%=o %> = true;
            <%if(Model.Content == null)
              {%>
            $.get('<%=Url.Action(Model.Action, Model.Controller, new{area = Model.Area}) %>',            
            <%=WBSSLStore.Web.Helpers.PopupForm.JsTools.JsonParam(Model.Parameters) %>            
            function(d){
            l<%=o %> = null;
            $("#<%=o %>").html(d).dialog('open');
            });
            <%
              }else
              {%>
            $("#<%=o %>").dialog('open');  
            l<%=o %> = null;
              <%}%> 
                                       
        }  

      
</script>
<div id="<%=o %>">
<%=Model.Content %>
</div>
