Public Class MediaLink
  Public ID As String
  Public MimeType As String
  Public MediaURL As String
  Public Updated As DateTime = DateTime.Now

  Friend Sub Render(ByVal ctx As HttpContext, ByVal colID As String, ByVal includeEditMedia As Boolean)
    Dim pUrl = ctx.Request.Url.GetLeftPart(UriPartial.Path)
    Dim sb As New System.Text.StringBuilder
    With sb
      ctx.Response.ContentType = "application/atom+xml;type=entry"
      .AppendLine("<?xml version=""1.0"" encoding=""utf-8""?>")
      .AppendLine("<entry xmlns=""http://www.w3.org/2005/Atom"">")
      .AppendLine("  <id>" & MakeGlobalIRI(ctx, "m+" & colID & "+" & ID) & "</id>")
      .AppendLine("  <title>None</title>")
      .AppendLine("  <author><name>None</name></author>")
      .AppendLine("  <summary type=""text"" />")
      .AppendLine("  <updated>" & System.Xml.XmlConvert.ToString(Updated, System.Xml.XmlDateTimeSerializationMode.Local) & "</updated>")
      .AppendLine("  <content type=""" & MimeType & """ src=""" & XMLEncode(MediaURL) & """/>")
      If includeEditMedia Then .AppendLine("  <link rel=""edit-media"" href=""" & XMLEncode(pUrl & "?t=m&c=" & HttpUtility.UrlEncode(colID) & "&i=" & HttpUtility.UrlEncode(ID)) & """ />")
      .AppendLine("  <link rel=""edit"" href=""" & XMLEncode(pUrl & "?t=i&c=" & HttpUtility.UrlEncode(colID) & "&i=" & HttpUtility.UrlEncode(ID)) & """ />")
      .AppendLine("</entry>")
    End With
    ctx.Response.Write(sb.ToString)
  End Sub

End Class
