
Public MustInherit Class MediaCollection
  Inherits Collection
  Public EnableUpdates As Boolean = True

  MustOverride Function GetAcceptedMediaTypes(ByVal user As System.Security.Principal.IIdentity) As IEnumerable(Of String)
  MustOverride Function CreateMedia(ByVal mimeType As String, ByVal mediaData As IO.Stream, ByVal slug As String, ByVal user As System.Security.Principal.IIdentity) As MediaLink
  MustOverride Sub UpdateMedia(ByVal itemID As String, ByVal mimeType As String, ByVal mediaData As IO.Stream, ByVal user As System.Security.Principal.IIdentity)
  MustOverride Function GetMediaLink(ByVal itemID As String, ByVal user As System.Security.Principal.IIdentity) As MediaLink
End Class

