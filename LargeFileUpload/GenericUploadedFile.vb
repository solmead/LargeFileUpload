
Imports System.IO
Imports System.Web

Public Class GenericUploadedFile
    Implements IFileInterface

    Public Property File As FileInfo

    Public Property FileName As String Implements IFileInterface.FileName

    Public Property Name As String Implements IFileInterface.Name

    Public Property PostedTimeStamp As Date Implements IFileInterface.PostedTimeStamp

    Public Sub SetInitialFileFromUploadedFile(hpf As Web.HttpPostedFile, Optional Filename As String = "") Implements IFileInterface.SetInitialFileFromUploadedFile
        Dim context = HttpContext.Current
        Dim tguid = Guid.NewGuid.ToString
        Dim fi As New FileInfo(MySettings.FileDirectory.FullName & "/" & tguid & ".tmp")
        hpf.SaveAs(fi.FullName)
        File = fi
        Me.FileName = Filename
        PostedTimeStamp = DateTime.Now
    End Sub

    Public Sub SetIntialFileDataFromFile(Fi As IO.FileInfo, Optional Filename As String = "") Implements IFileInterface.SetIntialFileDataFromFile
        File = Fi
        Me.FileName = Filename
        PostedTimeStamp = DateTime.Now
    End Sub

    Public Sub SetIntialFileDataFromStream(gStream As IO.Stream, Optional Filename As String = "") Implements IFileInterface.SetIntialFileDataFromStream
        Throw New NotImplementedException()
    End Sub

    Public Sub SpecialHandling() Implements IFileInterface.SpecialHandling

    End Sub
End Class
