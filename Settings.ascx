<%@ Control Inherits="DnnSharp.FaqMaster.Settings" language="C#" AutoEventWireup="true" Explicit="true" CodeFile="Settings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<table cellspacing="0" cellpadding="4" border="0" width = "100%" style = "font-size: small;">
    <tr>
        <td class="SubHead" valign = "top" style = "padding-top: 10px;">
            <dnn:Label runat = "server" id = "lblTemplate" ControlName = "ddTemplates"></dnn:Label>
        </td>
        <td valign="top">
            <asp:DropDownList runat = "server" ID = "ddTemplates" Width = "200"></asp:DropDownList>
        </td>
    </tr>
    <tr runat ="server" visible = "false">
        <td class="SubHead" valign = "top" style = "padding-top: 10px;">
            <dnn:Label runat = "server" id = "lblEffectShow" ControlName = "ddEffectsShow"></dnn:Label>
        </td>
        <td valign="top">
            <asp:DropDownList runat = "server" ID = "ddEffectsShow" Width = "200">
                <asp:ListItem Text = "none" Value = "none"></asp:ListItem>
                <asp:ListItem Text = "slide" Value = "slide"></asp:ListItem>
                <asp:ListItem Text = "fade" Value = "fade"></asp:ListItem>
            </asp:DropDownList>
        </td>
    </tr>
    <tr runat ="server" visible = "false">
        <td class="SubHead" valign = "top" style = "padding-top: 10px;">
            <dnn:Label runat = "server" id = "lblEffectHide" ControlName = "ddEffectsHide"></dnn:Label>
        </td>
        <td valign="top">
            <asp:DropDownList runat = "server" ID = "ddEffectsHide" Width = "200">
                <asp:ListItem Text = "none" Value = "none"></asp:ListItem>
                <asp:ListItem Text = "slide" Value = "slide"></asp:ListItem>
                <asp:ListItem Text = "fade" Value = "fade"></asp:ListItem>
            </asp:DropDownList>
        </td>
    </tr>
    <tr runat ="server" visible = "false">
        <td class="SubHead" valign = "top" style = "padding-top: 10px;">
            <dnn:Label runat = "server" id = "lblUseAjax" controlName = "cbUseAjax"></dnn:Label>
        </td>
        <td valign="top">
            <asp:CheckBox runat = "server" ID = "cbUseAjax" />
        </td>
    </tr>
    <tr>
        <td class="SubHead" valign = "top" style = "padding-top: 10px;">
            <dnn:Label runat = "server" id = "lblUseOwnjQuery" controlName = "cbUseOwnjQuery"></dnn:Label>
        </td>
        <td valign="top">
            <asp:CheckBox runat = "server" ID = "cbUseOwnjQuery" />
        </td>
    </tr>
</table>

