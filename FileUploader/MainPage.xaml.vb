Imports System.Windows.Browser
Imports System.Net
Imports System.Threading
Imports System.IO

Partial Public Class MainPage
    Inherits UserControl

    Property JavascriptCompleteFunction As String = ""
    'Property UploadPage As String = ""
    Property UploadChunkSize As Long = 100 * 1024
    Property MaximumUpload As Long = -1
    Property Filter As String = "All Files (*.*)|*.*"
    Property JavascriptStartFunction As String = ""
    Property JavascriptStatFunction As String = ""
    Property JavascriptMessageFunction As String = ""
    Property ID As String = ""
    Property FileGUID As String = ""

    Property Upload As New UploadInfo

    Private WithEvents DTimer As New System.Windows.Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 0, 0, 0, 1)}

    Private dialog As OpenFileDialog

    Private Message As String = ""

    Dim baseUri As Uri = New Uri(Application.Current.Host.Source, "/")


    Public WithEvents WR As New FileUpload(Me.Dispatcher)


    Private Sub JS_StartUpload(ByVal UI As Tracking)
        Try

            If JavascriptStartFunction <> "" Then
                HtmlPage.Window.Invoke(JavascriptStartFunction, UI)
            End If
        Catch ex As Exception
            Message = "JS_StartUpload " & ex.ToString
        End Try
    End Sub

    Private Sub JS_CompleteUpload(ByVal UI As Tracking)
        Try
            If JavascriptCompleteFunction <> "" Then
                HtmlPage.Window.Invoke(JavascriptCompleteFunction, UI)
            End If
        Catch ex As Exception
            Message = "JS_CompleteUpload " & ex.ToString
        End Try

    End Sub

    Private Sub JS_UploadStat(ByVal UI As Tracking)
        Try
            If JavascriptStatFunction <> "" Then
                HtmlPage.Window.Invoke(JavascriptStatFunction, UI)
            End If
        Catch ex As Exception
            Message = "JS_UploadStat " & ex.ToString
        End Try

    End Sub

    Private Sub JS_Message(ByVal Message As String)
        Try
            If JavascriptMessageFunction <> "" Then
                HtmlPage.Window.Invoke(JavascriptMessageFunction, New Tracking With {.Message = Message})
            End If
        Catch ex As Exception
            Message = "JS_Message " & ex.ToString
        End Try

    End Sub

    Private Sub WriteMessage(ByVal Msg As String)
        JS_Message(Msg)
        'Message = Message & " " & Msg
    End Sub


    <ScriptableMember()>
    Public Sub StopUpload()
        WR.CancelUpload()
    End Sub

    <ScriptableMember()>
    Public Sub ShowDialog()
        MessageBox.Show("External Show Dialog called")
        SelectFile()
    End Sub

    Public Sub New()
        InitializeComponent()
        dialog = New OpenFileDialog()
        dialog.Multiselect = False
        dialog.Filter = Filter
        HtmlPage.RegisterScriptableObject("Page", Me)
        ''HtmlPage.Window.Invoke("TalkToJavaScript", "Hello from Silverlight")
    End Sub

    Private Sub SelectFile()
        Try
            JS_Message("Opening Dialog")
            Dim dlg = dialog
            'dlg.Multiselect = False
            'dlg.Filter = Filter
            Dim retval = dlg.ShowDialog
            If retval IsNot Nothing AndAlso retval.Value Then
                JS_Message("Setting up Upload")
                Button1.IsEnabled = False
                Upload.File = dlg.File
                Upload.ID = ID
                Upload.Tracking.Name = Upload.File.Name
                Upload.Tracking.ID = ID

                JS_StartUpload(Upload.Tracking)
                'UploadFile()
                WR.File = Upload.File
                WR.FileGUID = FileGUID
                WR.UploadUrl = New Uri(baseUri.AbsoluteUri & "FileHandler.ashx")
                WR.Upload()

                DTimer.Start()

            End If

        Catch ex As Exception
            MessageBox.Show(ex.ToString)
        End Try
    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles Button1.Click
        'HtmlPage.Window.Invoke("TalkToJavaScript", "Hello from Silverlight")
        SelectFile()
    End Sub
    Private Function GetResourceString(ByVal Name As String, Optional ByVal [Default] As String = "") As String
        Try

            If App.Current.Resources.Contains(Name) Then

                Dim Temp = App.Current.Resources(Name)
                If Temp IsNot Nothing Then
                    Dim S = Temp.ToString
                    If S <> "" Then
                        'MessageBox.Show(Name & "=" & S)
                        Return S
                    End If
                End If
            End If
        Catch ex As Exception

        End Try
        Return [Default]
    End Function
    Private Function GetResourceNumber(ByVal Name As String, Optional ByVal [Default] As Double = 0) As Double
        Try
            If App.Current.Resources.Contains(Name) Then
                Dim Temp = App.Current.Resources(Name)
                If Temp IsNot Nothing Then
                    Dim S = Temp.ToString
                    If Val(S) > 0 Then
                        'MessageBox.Show(Name & "=" & Val(S))
                        Return Val(S)
                    End If
                End If
            End If
        Catch ex As Exception

        End Try
        Return [Default]
    End Function

    Private Sub MainPage_Loaded(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        Dim Pos = 1
        JS_Message("Version: 1.2")
        Try

            Filter = GetResourceString("Filter", Filter)
            Pos = 2
            JavascriptCompleteFunction = GetResourceString("JavascriptCompleteFunction", JavascriptCompleteFunction)
            Pos = 3
            JavascriptStartFunction = GetResourceString("JavascriptStartFunction", JavascriptStartFunction)
            Pos = 4
            'UploadPage = GetResourceString("UploadPage", UploadPage)
            Pos = 5
            FileGUID = GetResourceString("FileGUID", FileGUID)
            Pos = 6
            UploadChunkSize = GetResourceNumber("UploadChunkSize", UploadChunkSize)
            Pos = 7
            MaximumUpload = GetResourceNumber("MaximumUpload", MaximumUpload)
            Pos = 8
            JavascriptStatFunction = GetResourceString("JavascriptStatFunction", JavascriptStatFunction)
            JavascriptMessageFunction = GetResourceString("JavascriptMessageFunction", JavascriptMessageFunction)
            'JavascriptMessageFunction
            Pos = 9
            ID = GetResourceString("ID", ID)
            Pos = 10
            'MessageBox.Show("Filter=" & Filter)
        Catch ex As Exception
            MessageBox.Show("MainPageLoaded - " & Pos & " - " & ex.ToString)
        End Try


    End Sub

    Private Sub DTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles DTimer.Tick


        If Message <> "" Then MessageBox.Show(Message)
        Message = ""
        If WR.Status = FileUploadStatus.Canceled OrElse WR.Status = FileUploadStatus.Error OrElse WR.Status = FileUploadStatus.Removed OrElse WR.Status = FileUploadStatus.Complete Then
            DTimer.Stop()
        End If
    End Sub

    Private Sub WR_Message(ByVal Msg As String) Handles WR.MessageEvent

        WriteMessage(Msg)
    End Sub



    Private Sub WR_ProgressChangedEvent(ByVal sender As Object, ByVal args As UploadProgressChangedEventArgs) Handles WR.ProgressChangedEvent
        'Upload.updateTracking(args.TotalBytesUploaded)
        Dim T As New Tracking With {.percentUploaded = args.ProgressPercentage,
                                    .timeRemaining = Upload.Tracking.timeRemaining - Now.Subtract(Upload.Tracking.lastTime).TotalMilliseconds / 1000,
                                    .bytesUploaded = args.TotalBytesUploaded,
                                    .ID = Upload.Tracking.ID,
                                    .Name = Upload.Tracking.Name}
        JS_UploadStat(T)
    End Sub

    Private Sub WR_StatusChanged(ByVal sender As Object, ByVal e As Object) Handles WR.StatusChangedEvent
        If WR.Status = FileUploadStatus.Complete Then
            JS_CompleteUpload(Upload.Tracking)
        ElseIf WR.Status = FileUploadStatus.Canceled OrElse WR.Status = FileUploadStatus.Error OrElse WR.Status = FileUploadStatus.Removed Then
            JS_CompleteUpload(Upload.Tracking)
        End If
    End Sub

    Private Sub WR_WebCalledEvent(ByVal sender As Object, ByVal args As UploadProgressChangedEventArgs) Handles WR.WebCalledEvent
        Upload.updateTracking(args.TotalBytesUploaded)
        JS_UploadStat(Upload.Tracking)

    End Sub
End Class