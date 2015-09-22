Public MustInherit Class EntryCollection
  Inherits SyndicationCollection
  Public CategoriesFixed As Boolean

  MustOverride Function GetLastUpdated(ByVal includeDrafts As Boolean, ByVal user As System.Security.Principal.IIdentity) As DateTime
  MustOverride Function GetCategories(ByVal user As System.Security.Principal.IIdentity) As IEnumerable(Of String)
  MustOverride Function GetEntryList(ByVal index As Integer, ByVal count As Integer, ByVal includeDrafts As Boolean, ByVal user As System.Security.Principal.IIdentity) As IEnumerable(Of Entry)
  MustOverride Function GetEntry(ByVal itemID As String, ByVal user As System.Security.Principal.IIdentity) As Entry
  MustOverride Function CreateEntry(ByVal title As String, ByVal contentHtml As String, ByVal published As DateTime?, ByVal categories() As String, ByVal draft As Boolean, ByVal slug As String, ByVal user As System.Security.Principal.IIdentity) As Entry
  MustOverride Sub UpdateEntry(ByVal itemID As String, ByVal title As String, ByVal contentHtml As String, ByVal published As DateTime?, ByVal categories() As String, ByVal draft As Boolean, ByVal user As System.Security.Principal.IIdentity)
  MustOverride Sub DeleteEntry(ByVal itemID As String, ByVal user As System.Security.Principal.IIdentity)
End Class

