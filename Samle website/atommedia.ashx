<%@ WebHandler Language="VB" Class="atommedia" %>
Option Explicit On
Option Strict On

Imports System
Imports System.Web
Imports JHSoftware.AtomPub

Public Class atommedia : Implements IHttpHandler
    
  Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
    Dim id = Integer.Parse(context.Request.QueryString("id"))
    Dim mi = MyDataStore.GetItem(Of MyMediaCollection.MediaItem)(id)
    context.Response.ContentType = mi.MimeType
    context.Response.BinaryWrite(mi.Data)
  End Sub
 
  Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
    Get
      Return True
    End Get
  End Property

End Class