Public Class ForbiddenException
  Inherits Exception
End Class

Module Module1
  Sub ProcInheritorException(ByVal ctx As HttpContext, ByVal ex As Exception)
    If TypeOf ex Is ForbiddenException Then
      SendError(ctx, 403, "Forbidden")
    Else
      SendError(ctx, 500, ex.Message)
    End If
  End Sub

  Sub Send404(ByVal ctx As HttpContext)
    ctx.Response.StatusCode = 404
    ctx.Response.StatusDescription = "Not found"
    ctx.Response.Write("Not found")
    ctx.Response.End()
  End Sub

  Sub SendError(ByVal ctx As HttpContext, ByVal statusCode As Integer, ByVal desc As String)
    ctx.Response.StatusCode = statusCode
    ctx.Response.StatusDescription = desc
    ctx.Response.Write(desc)
    ctx.Response.End()
  End Sub

  Function XMLEncode(ByVal s As String) As String
    Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("""", "&quot;")
  End Function

  Function MakeGlobalIRI(ByVal ctx As HttpContext, ByVal idData As String) As String
    Dim x = ctx.Request.Url.Host.ToLower
    If x.StartsWith("www.") Then x = x.Substring(4)
    Dim md5 As New System.Security.Cryptography.MD5CryptoServiceProvider
    Dim g = New Guid(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(x & "+" & idData)))
    Return "urn:uuid:" & g.ToString
  End Function

  Function EnumToArray(Of T)(ByVal e As IEnumerable(Of T)) As T()
    If e Is Nothing Then Return Nothing
    Dim lst = New List(Of T)(e)
    Return lst.ToArray
  End Function

End Module
