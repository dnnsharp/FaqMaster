<%@ Control language="C#" Inherits="avt.FAQMaster.Main" AutoEventWireup="true" Explicit="True" CodeFile="Main.ascx.cs" %>

<div id = "pnlFaqs" runat  = "server" >
</div>

<div id = "pnlAddFaq" runat = "server" visible = "false" style = "margin-top: 20px;">

    <div style = "font-weight: bold; font-size: 16px; border-bottom: 1px solid #a1a1a1;" class = " text_blue_strong ">Add/Edit Faq</div>
    <br />
    <div class = "text_gray_medium"> Choose FAQ to add/edit </div>
    <asp:DropDownList runat = "server" ID = "ddEditFaq" OnSelectedIndexChanged = "OnChangeEditFaq" AutoPostBack = "true" class = "add_edit_field" Width = "410px"></asp:DropDownList>
    <br /><br />

    <div class = "text_gray_medium"> Question: </div>
    <asp:TextBox runat = "server" ID = "tbFaqQuestion" Width = "400px" class = "add_edit_field"></asp:TextBox>
    <asp:RequiredFieldValidator runat = "server" ControlToValidate = "tbFaqQuestion" Text = "Question is required!"></asp:RequiredFieldValidator>
    <div runat ="server" ID = "lblMyTokensQ" style = "margin-left: 10px; font-style: italic;"></div>
    <br /><br />
    
    
    <div class = "text_gray_medium">Answer: </div> 
    <asp:TextBox runat = "server" ID = "tbFaqAnswer" Width = "400" Height = "200" TextMode = "MultiLine" class = "add_edit_field"></asp:TextBox>
    <asp:RequiredFieldValidator runat = "server" ControlToValidate = "tbFaqAnswer" Text = "Answer is required!"></asp:RequiredFieldValidator>
    <div runat ="server" ID = "lblMyTokensA" style = "margin-left: 10px; font-style: italic;"></div>
    <br /><br />
    
    <div class = "text_gray_medium"> View Order Index:</div>
    <asp:TextBox runat = "server" ID = "tbOrder" Width = "120px" class = "add_edit_field"></asp:TextBox>
    <asp:RegularExpressionValidator runat = "server" ControlToValidate = "tbOrder" ValidationExpression = "^[0-9]*$" Text = "View Order Index must be numeric."></asp:RegularExpressionValidator>
    <br /><br />
    
    <asp:LinkButton runat = "server" ID = "btnSave" OnClick = "OnUpdateFaq"  class = "blue add_icon" Text = "Save"></asp:LinkButton>
    <asp:LinkButton ID="btnDelete" runat = "server" OnClick = "OnDeleteFaq"  class = "blue delete_icon" Text = "Delete" style = "margin-left: 10px;" Visible = "false" OnClientClick = "return confirm('Are you sure you want to delete this faq?');" CausesValidation = "false"></asp:LinkButton>
    
    <div runat = "server" id = "pnlActivate" style = "border-top: 1px solid #929292; margin-top: 20px;">
        <br />
        <div style = "color: #e26666; font-weight: bold;">
            Activate
        </div>
        <br />
        This copy of FAQMaster is not activated. Once you obtain a registration number, 
        proceed to <a href = "<%=TemplateSourceDirectory %>/Activation.aspx?rurl=<%= Request.RawUrl %>">Activate this copy</a>.
    </div>
</div>



