<%@ WebHandler Language="VB" Class="atomfeed" %>
Option Explicit On
Option Strict On

Imports System
Imports System.Web
Imports JHSoftware.AtomPub

Public Class atomfeed : Implements IHttpHandler
    
  Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
    Dim c = New MyEntryCollection
    Dim LastUpdated = c.GetLastUpdated(False, Nothing)
    Dim Entries = c.GetEntryList(0, 20, False, Nothing)   
    c.Render(context, LastUpdated, Entries, Nothing)
  End Sub
 
  Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
    Get
      Return True
    End Get
  End Property

End Class