Public Class SyndicationCollection
  Inherits Collection
  Public HtmlVersionURL As String
  Public AuthorName As String
  Public AuthorURL As String
  Public Rights As String
  Public OverrideGLobalID As String

  Public Sub Render(ByVal ctx As HttpContext, ByVal lastUpdated As DateTime, ByVal entries As IEnumerable(Of Entry), ByVal nextPageUrl As String)
    Render(ctx, lastUpdated, entries, nextPageUrl, True)
  End Sub

  Friend Sub Render(ByVal ctx As HttpContext, ByVal lastUpdated As DateTime, ByVal entries As IEnumerable(Of Entry), ByVal nextPageUrl As String, ByVal publicVersion As Boolean)
    ctx.Response.ContentType = "application/atom+xml"
    Dim sb As New System.Text.StringBuilder
    With sb
      .AppendLine("<?xml version=""1.0"" encoding=""utf-8""?>")
      .AppendLine("<feed xmlns=""http://www.w3.org/2005/Atom"">")
      .AppendLine("  <id>" & If(Not String.IsNullOrEmpty(OverrideGLobalID), XMLEncode(OverrideGLobalID), MakeGlobalIRI(ctx, "c+" & ID)) & "</id>")
      .AppendLine("  <title>" & XMLEncode(Title) & "</title>")
      If Not String.IsNullOrEmpty(HtmlVersionURL) Then .AppendLine("  <link rel=""alternate"" type=""application/xhtml+xml"" href=""" & XMLEncode(HtmlVersionURL) & """ />")
      .AppendLine("  <author>")
      .AppendLine("    <name>" & XMLEncode(AuthorName) & "</name>")
      If Not String.IsNullOrEmpty(AuthorURL) Then .AppendLine("    <uri>" & XMLEncode(AuthorURL) & "</uri>")
      .AppendLine("  </author>")
      If Not String.IsNullOrEmpty(Rights) Then sb.AppendLine("  <rights>" & XMLEncode(Rights) & "</rights>")
      .AppendLine("  <updated>" & System.Xml.XmlConvert.ToString(lastUpdated, System.Xml.XmlDateTimeSerializationMode.Local) & "</updated>")
      sb.AppendLine("  <generator>JH Software Atom Publisher library</generator>")
      If Not String.IsNullOrEmpty(nextPageUrl) Then sb.AppendLine("  <link rel=""next"" href=""" & XMLEncode(nextPageUrl) & """ />")
    End With
    ctx.Response.Write(sb.ToString)
    For Each en In entries
      en.Render(ctx, Me, False, publicVersion)
    Next
    ctx.Response.Write("</feed>" & vbCrLf)
  End Sub

End Class
