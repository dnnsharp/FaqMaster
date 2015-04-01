<%@ Page Language="C#" AutoEventWireup="true" Inherits="DnnSharp.FaqMaster.RegCore.Activate" CodeFile="Activate.aspx.cs" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml" data-ng-app="Activate">
<head runat="server">
    <title>Activate <%= DnnSharp.FaqMaster.Core.App.Info.Name %></title>
    <link type="text/css" rel="stylesheet" href="<%= ResolveInclude("/bootstrap/css/bootstrap.min.css", true) %>" />
    <link type="text/css" rel="stylesheet" href="<%= ResolveInclude("/dnnsf/css/activate.css", true) %>" />
    <script type="text/javascript" src="<%=ResolveInclude("/dnnsf/dnnsf.js", true) %>"></script>
    <script type="text/javascript" src="<%=ResolveInclude("/jquery.min.js", true) %>"></script>

    <script>
        var $ = dnnsfjQuery;
    </script>

</head>
<body class="bstrap30">
    <form class="form-horizontal" role="form" runat="server">
        <div class="container" data-ng-controller="ActivateCtl" data-ng-init="
                app = <%= HttpUtility.HtmlEncode(GetAppJson()) %>;
                returnUrl = '<%= ReturnUrl %>';">

            <div data-ng-include="'<%= ResolveInclude("/dnnsf/tpl/activate.html", true)  %>'"></div>

        </div>
    </form>

    <script type="text/javascript" src="<%=ResolveInclude("/modernizr.min.js", true)%>"></script>
    <script type="text/javascript" src="<%=ResolveInclude("/angular/angular.min.js", true)%>"></script>
    <script type="text/javascript" src="<%=ResolveInclude("/bootstrap/js/bootstrap.min.js", true)%>"></script>
    <script type="text/javascript" src="<%=ResolveInclude("/dnnsf/activate.js", true)%>"></script>

</body>
</html>
