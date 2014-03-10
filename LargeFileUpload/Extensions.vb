Imports System.Text
Imports System.Runtime.CompilerServices
Imports System.Web.Mvc
Imports System.Web.Mvc.Html
Imports System.IO
Imports System.Web

Public Module Extensions
    Public Delegate Function GetFileInterface() As IFileInterface
    Public Enum UploadTypeEnum
        SilverLight
        Flash
    End Enum
    <Extension()> _
    Public Function UploadFileActionLink(ByVal source As HtmlHelper, ByVal ID As String, Optional UploadType As UploadTypeEnum = UploadTypeEnum.Flash, Optional initializedFunction As String = "", Optional finishedFunction As String = "") As MvcHtmlString
        Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        Dim FileGUID As String = source.ViewData("FileGUID")
        If FileGUID Is Nothing OrElse FileGUID = "" Then
            FileGUID = Guid.NewGuid.ToString
        End If
        Dim FileGUIDText = ID & "_" & FileGUID
        'Dim postAction = URL.RouteUrl("Default", New With {.Controller = "UploadedFile", .Action = "AsyncUpload", .FileGUID = FileGUIDText})
        If String.IsNullOrWhiteSpace(initializedFunction) Then
            initializedFunction = "null"
        End If
        If String.IsNullOrWhiteSpace(finishedFunction) Then
            finishedFunction = "null"
        End If

        Dim SB As New StringBuilder
        SB.AppendLine("<input type=""file"" id=""" & ID & """ name=""" & ID & """ />")
        SB.AppendLine("")
        SB.AppendLine(source.Hidden(ID & "_FileGUID_Actual", FileGUID).ToString)
        SB.AppendLine(source.Hidden(ID & "_FileGUID", ID & "_" & FileGUID).ToString)
        SB.AppendLine("<script type=""text/javascript"">")
        SB.AppendLine("    $(function() {")
        If UploadType = UploadTypeEnum.SilverLight Then
            SB.AppendLine("         var uploader = System.FileUpload.Silverlight.CreateFileUploader('#" & ID & "', '" + ID + "_" + FileGUID + "', " + initializedFunction + ", " + finishedFunction + ");")
        Else
            SB.AppendLine("         var uploader = System.FileUpload.Flash.CreateFileUploader('#" & ID & "', '" + ID + "_" + FileGUID + "', " + initializedFunction + ", " + finishedFunction + ");")
        End If
        SB.AppendLine("    });")
        SB.AppendLine("</script>")

        Return New MvcHtmlString(SB.ToString)
    End Function


    <Extension()> _
    Public Function GetUploadedFile(ByVal source As System.Web.Mvc.Controller, ByVal id As String, ByVal itemName As String, getFileInstance As GetFileInterface) As IFileInterface
        Dim Context = HttpContext.Current
        Dim Request = Context.Request

        Dim item As IFileInterface = getFileInstance()
        'Dim guid As String = Request.Form(baseName & "_guid")
        Dim fname As String = Request.Form(id & "_filename")

        Dim fileUploaded = Request.Files(id)

        If fileUploaded IsNot Nothing AndAlso fileUploaded.ContentLength > 1000 Then
            item.SetInitialFileFromUploadedFile(fileUploaded)
            item.FileName = fileUploaded.FileName
            item.Name = itemName
            item.PostedTimeStamp = Now
            item.SpecialHandling()
            Return item
        ElseIf fname IsNot Nothing AndAlso fname.Length > 0 Then
            Dim guid = Request.Form(id & "_FileGUID")
            Dim fi As New FileInfo(MySettings.FileDirectory.FullName & "/" & guid & ".tmp")
            If fi.Exists Then
                item.SetIntialFileDataFromFile(fi, fname)
                item.Name = itemName
                item.PostedTimeStamp = Now
                item.SpecialHandling()
                'P.BackImage.IsReady = True
                Return item
            Else
                Throw New Exception("Temp file not found on server. FileName=[" & fname & "] TempName=[" & fi.FullName & "]")
            End If
        End If
        Return item
    End Function


    Public Function CalcCRC32(ByVal File As System.IO.FileInfo) As String

        Dim f = File.OpenRead
        Dim sha1 As New System.Security.Cryptography.SHA1CryptoServiceProvider()
        sha1.ComputeHash(f)


        Dim hash = sha1.Hash
        Dim buff = New StringBuilder()
        For Each hashByte As Byte In hash

            buff.Append(String.Format("{0:X1}", hashByte))
        Next
        f.Close()
        Return buff.ToString
    End Function
End Module
