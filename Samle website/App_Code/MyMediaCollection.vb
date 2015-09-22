Option Explicit On
Option Strict On

Imports JHSoftware.AtomPub

Public Class MyMediaCollection
  Inherits MediaCollection

  Sub New()
    Me.ID = "media"
    Me.Title = "Media"
    Me.EnableUpdates = True
  End Sub

  Public Overrides Function GetAcceptedMediaTypes(ByVal user As System.Security.Principal.IIdentity) As System.Collections.Generic.IEnumerable(Of String)
    Return "image/jpeg,image/gif,image/png".Split(","c)
  End Function

  Public Overrides Function CreateMedia(ByVal mimeType As String, ByVal mediaData As System.IO.Stream, ByVal slug As String, ByVal user As System.Security.Principal.IIdentity) As JHSoftware.AtomPub.MediaLink
    Dim urlRoot = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) & HttpContext.Current.Request.ApplicationPath
    If Not urlRoot.EndsWith("/"c) Then urlRoot &= "/"
    Dim mi = New MediaItem
    Dim id = MyDataStore.AddItem(mi)
    mi.ID = id.ToString
    mi.MediaURL = urlRoot & "atommedia.ashx?id=" & id
    mi.MimeType = mimeType
    mi.Updated = Now
    ReDim mi.Data(CInt(mediaData.Length - 1))
    mediaData.Read(mi.Data, 0, CInt(mediaData.Length))
    mediaData.Close()
    Return mi
  End Function

  Public Overrides Sub UpdateMedia(ByVal itemID As String, ByVal mimeType As String, ByVal mediaData As System.IO.Stream, ByVal user As System.Security.Principal.IIdentity)
    Dim id = Integer.Parse(itemID)
    Dim mi = MyDataStore.GetItem(Of MediaItem)(id)
    mi.MimeType = mimeType
    ReDim mi.Data(CInt(mediaData.Length - 1))
    mediaData.Read(mi.Data, 0, CInt(mediaData.Length))
    mediaData.Close()
  End Sub

  Public Overrides Function GetMediaLink(ByVal itemID As String, ByVal user As System.Security.Principal.IIdentity) As JHSoftware.AtomPub.MediaLink
    Dim id = Integer.Parse(itemID)
    Return MyDataStore.GetItem(Of MediaItem)(id)
  End Function

  Class MediaItem
    Inherits MediaLink
    Public Data As Byte()
  End Class
End Class
