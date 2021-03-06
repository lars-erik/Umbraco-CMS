<%@ Page Language="c#" Codebehind="republish.aspx.cs" MasterPageFile="../masterpages/umbracoDialog.Master" AutoEventWireup="True" Inherits="umbraco.cms.presentation.republish" %>
<%@ Register TagPrefix="cc1" Namespace="Umbraco.Web._Legacy.Controls" %>

<asp:Content ContentPlaceHolderID="head" runat="server">
   <script type="text/javascript">
     function showProgress(button, elementId) {
       var img = document.getElementById(elementId);

       img.style.visibility = "visible";
       button.style.display = "none";
     }
		</script>
</asp:Content>

<asp:Content ContentPlaceHolderID="body" runat="server">
<asp:Panel ID="p_republish" runat="server">
      <div class="propertyDiv">
          <p><%= Services.TextService.Localize("defaultdialogs/siterepublishHelp")%> </p>
      </div>

          <div id="buttons">
            <asp:Button ID="bt_go" OnClick="go" OnClientClick="showProgress(document.getElementById('buttons'),'progress'); return true;" runat="server" Text="Republish" />
            <em><%= Services.TextService.Localize("or") %></em>
            <a href="#" onclick="UmbClientMgr.closeModalWindow();"><%=Services.TextService.Localize("cancel")%></a>
          </div>

      <div id="progress" style="visibility: hidden;">
		<cc1:ProgressBar ID="progbar" runat="server" Title="Please wait..." />
      </div>

    </asp:Panel>

    <asp:Panel ID="p_done" Visible="false" runat="server">
     <div class="success">
      <p><%= Services.TextService.Localize("defaultdialogs/siterepublished")%></p>

     </div>
      <input type="button" class="guiInputButton" onclick="UmbClientMgr.closeModalWindow();" value="Ok" />
    </asp:Panel>
</asp:Content>