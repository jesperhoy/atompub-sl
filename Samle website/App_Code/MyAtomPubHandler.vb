Option Strict On
Option Explicit On

Imports JHSoftware.AtomPub

Public Class MyAtomPubHandler
  Inherits PublicationRequestHandler

  Public Overrides ReadOnly Property WorkspaceTitle As String
    Get
      Return "AtomPub Test Site"
    End Get
  End Property

  Public Overrides ReadOnly Property UseWindowsAuthentication As Boolean
    Get
      Return False
    End Get
  End Property

  Public Overrides Function Authenticate(ByVal userName As String, ByVal password As String) As Boolean
    Return (userName = "user" And password = "1234")
  End Function

  Public Overrides Function GetCollectionList(ByVal user As System.Security.Principal.IIdentity) As System.Collections.Generic.IEnumerable(Of JHSoftware.AtomPub.Collection)
    Return New JHSoftware.AtomPub.Collection() {New MyEntryCollection, New MyMediaCollection}
  End Function

  Public Overrides Function GetCollection(ByVal colID As String, ByVal user As System.Security.Principal.IIdentity) As JHSoftware.AtomPub.Collection
    Select Case colID
      Case "feed"
        Return New MyEntryCollection
      Case "media"
        Return New MyMediaCollection
      Case Else
        Return Nothing
    End Select
  End Function

End Class
