Option Explicit On
Option Strict On

Imports JHSoftware.AtomPub

Public Class MyEntryCollection
  Inherits EntryCollection

  Sub New()
    Me.ID = "feed"
    Me.Title = "News"
    Me.AuthorName = "John Doe"
    Me.AuthorURL = "http://www.example.com"
    Me.CategoriesFixed = False
    Me.HtmlVersionURL = "http://www.example.com/news.aspx"
    Me.Rights = "Copyright 2010 John Doe"
  End Sub

  Public Overrides Function CreateEntry(ByVal title As String, ByVal contentHtml As String, ByVal published As Date?, ByVal categories() As String, ByVal draft As Boolean, ByVal slug As String, ByVal user As System.Security.Principal.IIdentity) As JHSoftware.AtomPub.Entry
    Dim e = New Entry
    Dim id = MyDataStore.AddItem(e)
    e.ID = id.ToString
    e.Title = title
    e.ContentHtml = contentHtml
    e.Published = If(published, Now)
    e.Updated = Now
    e.Edited = Now
    e.Categories = categories
    e.Draft = draft
    e.HtmlVersionURL = "http://www.example.com/newsitem.aspx?id=" & id
    Return e
  End Function

  Public Overrides Sub UpdateEntry(ByVal itemID As String, ByVal title As String, ByVal contentHtml As String, ByVal published As Date?, ByVal categories() As String, ByVal draft As Boolean, ByVal user As System.Security.Principal.IIdentity)
    Dim id = Integer.Parse(itemID)
    Dim e = MyDataStore.GetItem(Of Entry)(id)
    e.Title = title
    e.ContentHtml = contentHtml
    If published IsNot Nothing Then e.Published = published
    e.Categories = categories
    e.Draft = draft
  End Sub

  Public Overrides Function GetEntry(ByVal itemID As String, ByVal user As System.Security.Principal.IIdentity) As JHSoftware.AtomPub.Entry
    Dim id = Integer.Parse(itemID)
    Return MyDataStore.GetItem(Of Entry)(id)
  End Function

  Public Overrides Sub DeleteEntry(ByVal itemID As String, ByVal user As System.Security.Principal.IIdentity)
    Dim id = Integer.Parse(itemID)
    MyDataStore.RemoveItem(id)
  End Sub

  Public Overrides Function GetEntryList(ByVal index As Integer, ByVal count As Integer, ByVal includeDraft As Boolean, ByVal user As System.Security.Principal.IIdentity) As System.Collections.Generic.IEnumerable(Of JHSoftware.AtomPub.Entry)
    Dim lst = MyDataStore.GetAllItems(Of Entry)()
    If Not includeDraft Then lst = lst.FindAll(Function(e As Entry) Not e.Draft)
    If index >= lst.Count Then Return New Entry() {}
    lst.Sort(Function(x As Entry, y As Entry) -x.Edited.Value.CompareTo(y.Edited.Value))
    Return lst.GetRange(index, Math.Min(count, lst.Count - index))
  End Function

  Public Overrides Function GetLastUpdated(ByVal includeDrafts As Boolean, ByVal user As System.Security.Principal.IIdentity) As Date
    Dim rv = #1/1/2000#
    For Each e In MyDataStore.GetAllItems(Of Entry)()
      If Not includeDrafts AndAlso e.Draft Then Continue For
      If e.Updated.Value > rv Then rv = e.Updated.Value
    Next
    Return rv
  End Function

  Public Overrides Function GetCategories(ByVal user As System.Security.Principal.IIdentity) As System.Collections.Generic.IEnumerable(Of String)
    Dim rv As New SortedSet(Of String)(New CategoryComparer)
    For Each e In MyDataStore.GetAllItems(Of Entry)()
      If e.Categories Is Nothing Then Continue For
      For Each c In e.Categories
        If Not rv.Contains(c) Then rv.Add(c)
      Next
    Next
    Return rv
  End Function

  Class CategoryComparer
    Implements IComparer(Of String)
    Public Function Compare(ByVal x As String, ByVal y As String) As Integer Implements System.Collections.Generic.IComparer(Of String).Compare
      Return String.Compare(x, y, True)
    End Function
  End Class

End Class
