Imports System.Text
Imports System.Runtime.CompilerServices
Imports System.Web.Mvc
Imports System.Web.Mvc.Html
Imports System.IO
Imports System.Web

Public Module Extensions

    <Extension()> _
    Public Function UploadFileActionLink(ByVal source As HtmlHelper, ByVal ID As String, Optional callbackFunction As String = "") As MvcHtmlString
        Dim URL As New UrlHelper(source.ViewContext.RequestContext)
        Dim FileGUID As String = source.ViewData("FileGUID")
        If FileGUID Is Nothing OrElse FileGUID = "" Then
            FileGUID = Guid.NewGuid.ToString
        End If
        Dim FileGUIDText = ID & "_" & FileGUID
        'Dim postAction = URL.RouteUrl("Default", New With {.Controller = "UploadedFile", .Action = "AsyncUpload", .FileGUID = FileGUIDText})

        Dim SB As New StringBuilder
        SB.AppendLine("<input type=""file"" id=""" & ID & """ name=""" & ID & """ />")
        SB.AppendLine("")
        SB.AppendLine(source.Hidden(ID & "_FileGUID_Actual", FileGUID).ToString)
        SB.AppendLine(source.Hidden(ID & "_FileGUID", ID & "_" & FileGUID).ToString)
        SB.AppendLine("<script type=""text/javascript"">")
        SB.AppendLine("    $(document).ready(function() {")
        SB.AppendLine("         var uploader = System.FileUploader('#" & ID & "', '" + ID + "_" + FileGUID + "'" + IIf(String.IsNullOrWhiteSpace(callbackFunction), "", ", " + callbackFunction) + ");")
        SB.AppendLine("    });")
        SB.AppendLine("</script>")

        Return New MvcHtmlString(SB.ToString)
    End Function


    <Extension()> _
    Public Function GetUploadedFile(ByVal source As System.Web.Mvc.Controller, ByVal origItem As IFileInterface, ByVal newItem As IFileInterface, ByVal baseName As String, ByVal itemName As String) As IFileInterface
        Dim Context = HttpContext.Current
        Dim Request = Context.Request

        Dim item As IFileInterface
        If origItem IsNot Nothing Then
            item = origItem
        Else
            item = newItem
        End If
        Dim guid As String = Request.Form(baseName & "_guid")
        Dim fname As String = Request.Form(baseName & "_filename")

        Dim fileUploaded = Request.Files(baseName)

        If fileUploaded IsNot Nothing AndAlso fileUploaded.ContentLength > 1000 Then
            item.SetInitialFileFromUploadedFile(fileUploaded)
            item.FileName = fileUploaded.FileName
            item.Name = itemName
            item.PostedTimeStamp = Now
            item.SpecialHandling()
            Return item
        ElseIf fname IsNot Nothing AndAlso fname.Length > 0 Then
            guid = Request.Form(baseName & "_FileGUID")
            Dim fi As New FileInfo(Settings.FileDirectory.FullName & "/AsyncUploads/" & guid & ".tmp")
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
        Return origItem
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
