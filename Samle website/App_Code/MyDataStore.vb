Option Explicit On
Option Strict On

Imports JHSoftware.AtomPub

Public Class MyDataStore

  Private Shared Items As New Dictionary(Of Integer, Object)
  Private Shared NextID As Integer = CInt(Now.TimeOfDay.TotalMilliseconds)

  Public Shared Function AddItem(ByVal value As Object) As Integer
    Dim id = NextID
    NextID += 1
    Items.Add(id, value)
    Return id
  End Function

  Public Shared Function GetItem(Of T)(ByVal id As Integer) As T
    Dim rv As Object = Nothing
    If Not Items.TryGetValue(id, rv) OrElse Not TypeOf rv Is T Then Return Nothing
    Return DirectCast(rv, T)
  End Function

  Public Shared Sub RemoveItem(ByVal id As Integer)
    Items.Remove(id)
  End Sub

  Public Shared Function GetAllItems(Of T)() As List(Of T)
    Dim rv As New List(Of T)
    For Each itm In Items.Values
      If TypeOf itm Is T Then rv.Add(DirectCast(itm, T))
    Next
    Return rv
  End Function

End Class
