
Public Class Entry
  Public ID As String
  Public Title As String
  Public ContentHtml As String
  Public Published As DateTime?
  Public Updated As DateTime?
  Public Edited As DateTime?
  Public HtmlVersionURL As String
  Public OverrideGLobalID As String
  Public Draft As Boolean
  Public Categories As IEnumerable(Of String)

  Friend Sub Render(ByVal ctx As HttpContext, ByVal col As SyndicationCollection, ByVal standAlone As Boolean, ByVal publicVersion As Boolean)
    Dim sb As New System.Text.StringBuilder
    With sb
      If standAlone Then
        ctx.Response.ContentType = "application/atom+xml;type=entry"
        .AppendLine("<?xml version=""1.0"" encoding=""utf-8""?>")
        .AppendLine("<entry xmlns=""http://www.w3.org/2005/Atom"">")
      Else
        .AppendLine("<entry>")
      End If
      .AppendLine("  <id>" & If(Not String.IsNullOrEmpty(OverrideGLobalID), XMLEncode(OverrideGLobalID), MakeGlobalIRI(ctx, "e+" & col.ID & "+" & ID)) & "</id>")
      .AppendLine("  <title>" & XMLEncode(Title) & "</title>")
      If Published IsNot Nothing Then .AppendLine("  <published>" & System.Xml.XmlConvert.ToString(Published.Value, System.Xml.XmlDateTimeSerializationMode.Local) & "</published>")
      If Edited IsNot Nothing Then .AppendLine("  <edited>" & System.Xml.XmlConvert.ToString(Published.Value, System.Xml.XmlDateTimeSerializationMode.Local) & "</edited>")
      If Updated Is Nothing Then Updated = Edited
      If Updated Is Nothing Then Updated = Published
      .AppendLine("  <updated>" & System.Xml.XmlConvert.ToString(Updated.Value, System.Xml.XmlDateTimeSerializationMode.Local) & "</updated>")
      .AppendLine("  <content type=""html"">" & XMLEncode(ContentHtml) & "</content>")
      If Not String.IsNullOrEmpty(HtmlVersionURL) Then .AppendLine("  <link rel=""alternate"" type=""text/html"" href=""" & XMLEncode(HtmlVersionURL) & """ />")
      If Not publicVersion Then .AppendLine("  <link rel=""edit"" href=""" & XMLEncode(ctx.Request.Url.GetLeftPart(UriPartial.Path) & "?t=i&c=" & HttpUtility.UrlEncode(col.ID) & "&i=" & HttpUtility.UrlEncode(ID)) & """ />")
      If standAlone Then
        .AppendLine("  <author>")
        .AppendLine("    <name>" & XMLEncode(col.AuthorName) & "</name>")
        If Not String.IsNullOrEmpty(col.AuthorURL) Then .AppendLine("    <uri>" & XMLEncode(col.AuthorURL) & "</uri>")
        .AppendLine("  </author>")
        If Not String.IsNullOrEmpty(col.Rights) Then .AppendLine("  <rights>" & XMLEncode(col.Rights) & "</rights>")
      End If
      If Categories IsNot Nothing Then
        For Each cat In Categories
          sb.AppendLine("  <category term=""" & XMLEncode(cat) & """ />")
        Next
      End If
      If Draft Then .AppendLine("  <app:control xmlns:app=""http://www.w3.org/2007/app""><app:draft>yes</app:draft></app:control>")
      .AppendLine("</entry>")
    End With
    ctx.Response.Write(sb.ToString)
  End Sub
End Class
