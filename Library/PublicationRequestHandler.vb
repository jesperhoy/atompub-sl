Public MustInherit Class PublicationRequestHandler
  Implements IHttpHandler

  MustOverride ReadOnly Property WorkspaceTitle As String
  MustOverride ReadOnly Property UseWindowsAuthentication As Boolean

  MustOverride Function GetCollectionList(ByVal user As System.Security.Principal.IIdentity) As IEnumerable(Of Collection)
  MustOverride Function GetCollection(ByVal colID As String, ByVal user As System.Security.Principal.IIdentity) As Collection

  MustOverride Function Authenticate(ByVal userName As String, ByVal password As String) As Boolean

  Public Sub ProcessPublishingRequest(ByVal context As System.Web.HttpContext) Implements System.Web.IHttpHandler.ProcessRequest
    Dim user As System.Security.Principal.IIdentity
    If UseWindowsAuthentication Then
      If Not context.Request.IsAuthenticated Then GoTo markNotAuth
      user = context.Request.LogonUserIdentity
    Else
      Dim x = context.Request.Headers("Authorization")
      If String.IsNullOrEmpty(x) Then GoTo markNotAuth
      If Not x.StartsWith("basic ", StringComparison.InvariantCultureIgnoreCase) Then GoTo markNotAuth
      x = System.Text.Encoding.ASCII.GetString(System.Convert.FromBase64String(x.Substring(6)))
      Dim i = x.IndexOf(":")
      If Not Authenticate(x.Substring(0, i), x.Substring(i + 1)) Then GoTo markNotAuth
      user = New System.Security.Principal.GenericIdentity(x.Substring(0, i))
    End If

    context.Response.Expires = -1
    Select Case context.Request.QueryString("t")
      Case "", Nothing ' service document
        ProcReqServiceDocument(context, user)
      Case "c" 'collection 
        ProcReqCollection(context, user)
      Case "i" ' item
        ProcReqItem(context, user)
      Case "m" ' edit-media
        ProcReqEditMedia(context, user)
      Case Else
        Send404(context)
    End Select
    Exit Sub

markNotAuth:
    context.Response.StatusCode = 401
    context.Response.StatusDescription = "Unauthorized"
    If Not UseWindowsAuthentication Then context.Response.AddHeader("WWW-Authenticate", "Basic realm=""AtomPub""")
    context.Response.Write("Unauthorized")
  End Sub

  Private Sub ProcReqServiceDocument(ByVal ctx As HttpContext, ByVal user As System.Security.Principal.IIdentity)
    Dim pUrl = ctx.Request.Url.GetLeftPart(UriPartial.Path)
    Dim lst As IEnumerable(Of Collection) = Nothing
    Try
      lst = GetCollectionList(user)
    Catch ex As Exception
      ProcInheritorException(ctx, ex)
    End Try
    Dim sb As New System.Text.StringBuilder
    With sb
      .AppendLine("<?xml version=""1.0"" encoding=""utf-8""?>")
      .AppendLine("<service xmlns=""http://www.w3.org/2007/app"" xmlns:atom=""http://www.w3.org/2005/Atom"">")
      .AppendLine("  <workspace>")
      Dim x = WorkspaceTitle
      If String.IsNullOrEmpty(x) Then SendError(ctx, 500, "WorkspaceTitle is null/empty")
      .AppendLine("    <atom:title>" & XMLEncode(x) & "</atom:title>")
      For Each col In lst
        col.CheckIsValid(ctx)
        sb.AppendLine("    <collection href=""" & XMLEncode(pUrl & "?t=c&c=" & HttpUtility.UrlEncode(col.ID)) & """>")
        sb.AppendLine("      <atom:title>" & XMLEncode(col.Title) & "</atom:title>")
        If TypeOf col Is EntryCollection Then
          sb.AppendLine("      <accept>application/atom+xml;type=entry</accept>")
          Dim cats As IEnumerable(Of String) = Nothing
          Try
            cats = DirectCast(col, EntryCollection).GetCategories(user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try
          If cats IsNot Nothing Then
            sb.AppendLine("      <categories fixed=""" & If(DirectCast(col, EntryCollection).CategoriesFixed, "yes", "no") & """>")
            For Each cat In cats
              sb.AppendLine("        <atom:category term=""" & XMLEncode(cat) & """ />")
            Next
            sb.AppendLine("      </categories>")
          End If

        ElseIf TypeOf col Is MediaCollection Then
          Dim mtps As IEnumerable(Of String) = Nothing
          Try
            mtps = DirectCast(col, MediaCollection).GetAcceptedMediaTypes(user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try
          If mtps Is Nothing Then mtps = "image/jpeg,image/png,image/gif".Split(","c)
          For Each s In mtps
            .AppendLine("      <accept>" & XMLEncode(s) & "</accept>")
          Next
          .AppendLine("      <categories fixed=""yes"" />")
        End If
        .AppendLine("    </collection>")

      Next
      .AppendLine("  </workspace>")
      .AppendLine("</service>")
    End With

    ctx.Response.ContentType = "application/atomsvc+xml"
    ctx.Response.Write(sb.ToString)
  End Sub

  Private Function GetCollectionFromRequest(ByVal ctx As HttpContext, ByVal user As System.Security.Principal.IIdentity) As Collection
    Dim rv As Collection = Nothing
    Try
      rv = GetCollection(ctx.Request.QueryString("c"), user)
    Catch ex As Exception
      ProcInheritorException(ctx, ex)
    End Try
    If rv Is Nothing Then Send404(ctx)
    rv.CheckIsValid(ctx)
    Return rv
  End Function

  Private Sub ProcReqCollection(ByVal ctx As HttpContext, ByVal user As System.Security.Principal.IIdentity)
    Dim col = GetCollectionFromRequest(ctx, user)

    If TypeOf col Is EntryCollection Then
      Dim ecol = DirectCast(col, EntryCollection)
      Select Case ctx.Request.HttpMethod
        Case "GET"
          Dim PageSize = 25
          Dim pageNum As Integer
          If Not Integer.TryParse(ctx.Request.QueryString("p"), pageNum) Then pageNum = 1
          Dim lst As List(Of Entry) = Nothing
          Try
            lst = New List(Of Entry)(ecol.GetEntryList((pageNum - 1) * PageSize, PageSize + 1, True, user))
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try
          Dim NPUrl As String = Nothing
          If lst.Count > PageSize Then
            lst = lst.GetRange(0, PageSize)
            NPUrl = ctx.Request.Url.GetLeftPart(UriPartial.Path) & "?t=c&c=" & HttpUtility.UrlEncode(col.ID) & "&p=" & (pageNum + 1)
          End If
          ecol.Render(ctx, ecol.GetLastUpdated(True, user), lst, NPUrl, False)

        Case "POST"
          Dim en As Entry = Nothing
          Try
            en = ParseEntryPost(ctx)
          Catch ex As Exception
            SendError(ctx, 400, ex.Message)
          End Try
          Try
            en = ecol.CreateEntry(en.Title, en.ContentHtml, en.Published, EnumToArray(en.Categories), en.Draft, ParseSlug(ctx), user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try
          ctx.Response.StatusCode = 201 : ctx.Response.StatusDescription = "Created"
          Dim pUrl = ctx.Request.Url.GetLeftPart(UriPartial.Path)
          ctx.Response.AddHeader("Location", pUrl & "?t=i&c=" & HttpUtility.UrlEncode(col.ID) & "&i=" & HttpUtility.UrlEncode(en.ID))
          en.Render(ctx, ecol, True, False)

        Case Else
          SendError(ctx, 405, "Method not allowed")
      End Select

    ElseIf TypeOf col Is MediaCollection Then
      Dim mcol = DirectCast(col, MediaCollection)
      Select Case ctx.Request.HttpMethod
        Case "POST"
          Dim ml As MediaLink = Nothing
          Try
            ml = mcol.CreateMedia(ctx.Request.ContentType, ctx.Request.InputStream, ParseSlug(ctx), user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try
          ctx.Response.StatusCode = 201 : ctx.Response.StatusDescription = "Created"
          Dim pUrl = ctx.Request.Url.GetLeftPart(UriPartial.Path)
          ctx.Response.AddHeader("Location", pUrl & "?t=i&c=" & HttpUtility.UrlEncode(col.ID) & "&i=" & HttpUtility.UrlEncode(ml.ID))
          ml.Render(ctx, col.ID, mcol.EnableUpdates)

        Case Else
          SendError(ctx, 405, "Method not allowed")
      End Select

    Else
      Send404(ctx)
    End If
  End Sub

  Private Function ParseSlug(ByVal ctx As HttpContext) As String
    Dim x = ctx.Request.Headers("slug")
    If String.IsNullOrEmpty(x) Then Return Nothing
    Dim bytes As New List(Of Byte)
    Dim p = 0
    While p < x.Length
      If x(p) = "%"c Then
        If Not p + 2 < x.Length Then Exit While
        bytes.Add(System.Convert.ToByte(x.Substring(p + 1, 2), 16))
        p += 3
      Else
        bytes.Add(CByte(AscW(x(p))))
        p += 1
      End If
    End While
    Return System.Text.Encoding.UTF8.GetString(bytes.ToArray)
  End Function

  Private Sub ProcReqItem(ByVal ctx As HttpContext, ByVal user As System.Security.Principal.IIdentity)
    Dim col = GetCollectionFromRequest(ctx, user)
    Dim ItemID = ctx.Request.QueryString("i")

    If TypeOf col Is EntryCollection Then
      Dim ecol = DirectCast(col, EntryCollection)
      Select Case ctx.Request.HttpMethod
        Case "GET"
          Dim e As Entry = Nothing
          Try
            e = ecol.GetEntry(ItemID, user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try
          If e Is Nothing Then Send404(ctx)
          e.Render(ctx, ecol, True, False)

        Case "PUT"
          Dim e As Entry = Nothing
          Try
            e = ParseEntryPost(ctx)
            ecol.UpdateEntry(ItemID, e.Title, e.ContentHtml, e.Published, EnumToArray(e.Categories), e.Draft, user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try

        Case "DELETE"
          Try
            ecol.DeleteEntry(ItemID, user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try

        Case Else
          SendError(ctx, 405, "Method not allowed")
      End Select

    ElseIf TypeOf col Is MediaCollection Then
      Dim mcol = DirectCast(col, MediaCollection)
      Select Case ctx.Request.HttpMethod
        Case "GET"
          Dim ml As MediaLink = Nothing
          Try
            ml = DirectCast(col, MediaCollection).GetMediaLink(ItemID, user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try
          If ml Is Nothing Then Send404(ctx)
          ml.Render(ctx, col.ID, mcol.EnableUpdates)

        Case Else
          SendError(ctx, 405, "Method not allowed")
      End Select

    Else
      Send404(ctx)
    End If
  End Sub

  Private Sub ProcReqEditMedia(ByVal ctx As HttpContext, ByVal user As System.Security.Principal.IIdentity)
    Dim col = GetCollectionFromRequest(ctx, user)
    Dim ItemID = ctx.Request.QueryString("i")

    If TypeOf col Is MediaCollection Then
      Dim mcol = DirectCast(col, MediaCollection)
      If Not mcol.EnableUpdates Then Send404(ctx)
      Select Case ctx.Request.HttpMethod
        Case "HEAD" 'WLW likes to do this for some unknown reason
          REM do nothing - just return 200 OK

        Case "PUT"
          Try
            mcol.UpdateMedia(ItemID, ctx.Request.ContentType, ctx.Request.InputStream, user)
          Catch ex As Exception
            ProcInheritorException(ctx, ex)
          End Try

        Case Else
          SendError(ctx, 405, "Method not allowed")
      End Select

    Else
      Send404(ctx)
    End If

  End Sub


  Private Function ParseEntryPost(ByVal ctx As HttpContext) As Entry
    Dim strm = New System.IO.StreamReader(ctx.Request.InputStream, System.Text.Encoding.UTF8)
    Dim body = strm.ReadToEnd()
    strm.Close()

    Dim rdr = New System.IO.StringReader(body)
    Dim doc = New System.Xml.XmlDocument
    doc.Load(rdr)
    Dim elem = doc.DocumentElement
    If ResolveElementName(elem) <> "atom:entry" Then Throw New Exception("atom:entry not found at XML root")
    Dim rv As New Entry
    Dim CatLst As New List(Of String)
    rv.Categories = CatLst
    For Each n As System.Xml.XmlNode In elem.ChildNodes
      If n.NodeType <> System.Xml.XmlNodeType.Element Then Continue For
      With DirectCast(n, System.Xml.XmlElement)
        Select Case ResolveElementName(DirectCast(n, System.Xml.XmlElement))
          Case "atom:title"
            rv.Title = .InnerText
          Case "atom:content"
            Select Case .GetAttribute("type")
              Case Nothing, "", "text"
                rv.ContentHtml = HttpUtility.HtmlEncode(.InnerText)
              Case "html"
                rv.ContentHtml = .InnerText
              Case "xhtml"
                rv.ContentHtml = .InnerXml
              Case Else
                Throw New Exception("Content type not supported")
            End Select
          Case "atom:published"
            rv.Published = System.Xml.XmlConvert.ToDateTime(.InnerText, System.Xml.XmlDateTimeSerializationMode.Local)
          Case "atom:category"
            CatLst.Add(.GetAttribute("term"))
          Case "app:control"
            For Each n2 As System.Xml.XmlNode In n.ChildNodes
              If n2.NodeType <> System.Xml.XmlNodeType.Element Then Continue For
              If ResolveElementName(DirectCast(n2, System.Xml.XmlElement)) <> "app:draft" Then Continue For
              If DirectCast(n2, System.Xml.XmlElement).InnerText = "yes" Then rv.Draft = True
              Exit For
            Next
        End Select
      End With
    Next

    Return rv
  End Function

  Private Function ResolveElementName(ByVal el As System.Xml.XmlElement) As String
    Select Case el.NamespaceURI.ToLower
      Case "http://www.w3.org/2005/atom"
        Return "atom:" & el.LocalName
      Case "http://www.w3.org/2007/app"
        Return "app:" & el.LocalName
      Case Else
        Return "????"
    End Select
  End Function


  Public ReadOnly Property IsReusable As Boolean Implements System.Web.IHttpHandler.IsReusable
    Get
      Return False
    End Get
  End Property

End Class



