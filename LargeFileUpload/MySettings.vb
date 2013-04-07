Imports System.IO
Imports System.Web

Public Class Settings
    Inherits ConfigSettings.Settings

    Public Shared ReadOnly Property FileDirectory() As DirectoryInfo
        Get
            Dim context = HttpContext.Current
            Dim di As New DirectoryInfo(context.Server.MapPath(FileLocation))
            If Not di.Exists Then di.Create()
            Return di
        End Get
    End Property

    Public Shared Property FileLocation() As String
        Get
            Dim s = ConfigProperty("FileLocation")
            If String.IsNullOrWhiteSpace(s) Then
                s = "/Uploads/"
                ConfigProperty("FileLocation") = s
            End If

            Return s
        End Get
        Set(ByVal value As String)
            ConfigProperty("FileLocation") = value
        End Set
    End Property

End Class
