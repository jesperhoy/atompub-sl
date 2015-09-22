<%@ Page Language="VB" %>
<%
  Dim UrlRoot As String = Request.Url.GetLeftPart(UriPartial.Authority) & Request.ApplicationPath
  If Not UrlRoot.EndsWith("/"c) Then UrlRoot &= "/"
%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>AtomPub Test Site</title>
    <link rel="service" type="application/atomsvc+xml" href="<%=urlroot%>atompub" />
    <link rel="alternate" type="application/rss+xml" title="AtomPub Test Site News Feed" href="<%=urlroot%>atomfeed.ashx" />
</head>
<body>
    <h1>AtomPub Test Site</h1>

    <p><a href="<%=urlroot%>atomfeed.ashx">Read our Atom news feed</a></p>

    <p>To publish items to the Atom feed, point Windows Live Writer to: <b><%=UrlRoot %></b><br/>
    Username: <b>user</b> / password: <b>1234</b></p>

    <p>Note that this sample site stores feed entries and media in the web-server's memory only, so capacity is limited and all data is lost when the web-site or server is restarted.</p>

</body>
</html>
