Public MustInherit Class Collection
  Public ID As String
  Public Title As String
  Friend Sub New()
    REM friend to prevent direct inheriting 
  End Sub

  Friend Sub CheckIsValid(ByVal ctx As HttpContext)
    If String.IsNullOrEmpty(ID) Then SendError(ctx, 500, "Collection ID is null/empty")
    If String.IsNullOrEmpty(Title) Then SendError(ctx, 500, "Collection Title is null/empty")
  End Sub
End Class

