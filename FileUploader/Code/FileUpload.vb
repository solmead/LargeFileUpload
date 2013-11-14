'/*
' * Copyright (C) 2009-2012 Solmead Productions
' *
' * == BEGIN LICENSE ==
' *
' * Licensed under the terms of any of the following licenses at your
' * choice:
' *
' *  - GNU General Public License Version 2 or later (the "GPL")
' *    http://www.gnu.org/licenses/gpl.html
' *
' *  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
' *    http://www.gnu.org/licenses/lgpl.html
' *
' *  - Mozilla Public License Version 1.1 or later (the "MPL")
' *    http://www.mozilla.org/MPL/MPL-1.1.html
' *
' * == END LICENSE ==
' */

'using System;
Imports System.Net
'using System.Windows;
'using System.Windows.Controls;
'using System.Windows.Documents;
'using System.Windows.Ink;
'using System.Windows.Input;
'using System.Windows.Media;
'using System.Windows.Media.Animation;
'using System.Windows.Shapes;
'using FluxJpeg.Core.Decoder;
'using FluxJpeg.Core;
Imports System.IO
'using FluxJpeg.Core.Filtering;
'using FluxJpeg.Core.Encoder;
Imports System.Threading
Imports System.ComponentModel
Imports System.Text
Imports System.Windows.Threading



Public Enum FileUploadStatus
    Pending
    Uploading
    Complete
    [Error]
    Canceled
    Removed
    Resizing
End Enum


Public Class FileUpload
    Implements INotifyPropertyChanged

    Public Event MessageEvent(ByVal Msg As String)

    Public Event ProgressChangedEvent(ByVal sender As Object, ByVal args As UploadProgressChangedEventArgs)
    Public Event WebCalledEvent(ByVal sender As Object, ByVal args As UploadProgressChangedEventArgs)
    Public Event StatusChangedEvent(ByVal sender As Object, ByVal e As Object)
    Public Event PropertyChangedEvent(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public ChunkSize As Long = 1 * 512 * 1024 ' 4194304 / 2 / 10

    Public Property UploadUrl As Uri

    Public Property FileGUID As String = ""

    Public Sub ProgressChanged(ByVal args As UploadProgressChangedEventArgs)
        Me.Dispatcher.BeginInvoke(Sub()
                                      RaiseEvent ProgressChangedEvent(Me, args)
                                  End Sub)
    End Sub
    Public Sub WebCalled(ByVal args As UploadProgressChangedEventArgs)
        Me.Dispatcher.BeginInvoke(Sub()
                                      RaiseEvent WebCalledEvent(Me, args)
                                  End Sub)
    End Sub
    Public Sub StatusChanged(ByVal e As Object)
        Me.Dispatcher.BeginInvoke(Sub()
                                      RaiseEvent StatusChangedEvent(Me, e)
                                  End Sub)
    End Sub
    Public Sub PropertyChanged(ByVal e As System.ComponentModel.PropertyChangedEventArgs)
        Me.Dispatcher.BeginInvoke(Sub()
                                      RaiseEvent PropertyChangedEvent(Me, e)
                                  End Sub)
    End Sub
    Public Sub Message(ByVal Msg As String)
        Me.Dispatcher.BeginInvoke(Sub()
                                      RaiseEvent MessageEvent(Msg)
                                  End Sub)
    End Sub

    Private m_file As FileInfo
    Public Property File As FileInfo
        Get
            Return m_file
        End Get
        Set(ByVal value As FileInfo)
            m_file = value
            Dim temp As Stream = File.OpenRead()
            FileLength = temp.Length
            temp.Close()
        End Set
    End Property

    Public ReadOnly Property Name As String
        Get
            Return File.Name
        End Get
    End Property

    Private m_fileLength As Long
    Public Property FileLength As Long
        Get
            Return m_fileLength
        End Get
        Set(ByVal value As Long)
            m_fileLength = value
            PropertyChanged(New PropertyChangedEventArgs("FileLength"))
        End Set
    End Property


    Private resizeStream As MemoryStream

    Private m_bytesUploaded As Long
    Public Property BytesUploaded As Long
        Get
            Return m_bytesUploaded
        End Get
        Set(ByVal value As Long)
            m_bytesUploaded = value

            PropertyChanged(New PropertyChangedEventArgs("BytesUploaded"))

        End Set
    End Property


    Private m_uploadPercent As Integer
    Public Property UploadPercent As Integer
        Get
            Return m_uploadPercent
        End Get
        Set(ByVal value As Integer)
            m_uploadPercent = value

            PropertyChanged(New PropertyChangedEventArgs("UploadPercent"))

        End Set
    End Property

    Private m_status As FileUploadStatus
    Public Property Status As FileUploadStatus
        Get
            Return m_status
        End Get
        Set(ByVal value As FileUploadStatus)
            m_status = value
            PropertyChanged(New PropertyChangedEventArgs("Status"))
            StatusChanged(Nothing)

        End Set
    End Property


    Private Dispatcher As Dispatcher

    Private cancel As Boolean
    Private remove As Boolean

    Private Function ProcessReturn(ByVal xml As String) As String
        Dim XMLD As XElement = XElement.Load(New System.IO.StringReader(xml))



        'XMLDoc.LoadXml(xml)
        '<Response><Error>False</Error><Description></Description></Response>
        Dim IsError = XMLD.<Error>.Value
        'CBool(XMLDoc.SelectSingleNode("Response\Error").Value)
        Dim Description = XMLD.<Description>.Value
        ' XMLDoc.SelectSingleNode("Response\Description").Value
        If IsError Then
            Throw New Exception(Description)
        End If
        Return Description
    End Function

    Public Sub New(ByVal Dispatcher As Dispatcher)
        Me.Dispatcher = Dispatcher
        Me.Status = FileUploadStatus.Pending
    End Sub

    Public Sub New(ByVal Dispatcher As Dispatcher, ByVal uploadUrl As Uri)
        Me.New(Dispatcher)
        Me.UploadUrl = uploadUrl
    End Sub

    Public Sub New(ByVal Dispatcher As Dispatcher, ByVal uploadUrl As Uri, ByVal fileToUpload As FileInfo)
        Me.New(Dispatcher, uploadUrl)


        Me.File = fileToUpload
    End Sub

    Public Sub Upload()
        Try
            If (File Is Nothing OrElse UploadUrl Is Nothing) Then
                Message("No file or no url")
                Return
            End If
            Status = FileUploadStatus.Uploading
            cancel = False

            Message("Name=" & File.Name)
            Message("FileGUID=" & FileGUID)
            Message("Length=" & Me.FileLength)
            Message("URL=" & UploadUrl.OriginalString)
            Dim CS As Double = Me.FileLength / 100
            If CS < 4096 * 2 Then
                CS = 4096 * 2
            ElseIf CS > 512 * 1024 Then
                CS = 512 * 1024
            End If

            For a = 0 To 64
                If 2 ^ a > CS Then
                    CS = 2 ^ (a - 1)
                    Exit For
                End If
            Next
            ChunkSize = CS
            Message("ChunkSize=" & ChunkSize)

            UploadFileEx()

        Catch ex As Exception
            Message(ex.ToString)
        End Try
    End Sub



    Public Sub CancelUpload()
        cancel = True
    End Sub

    Public Sub RemoveUpload()

        cancel = True
        remove = True
        If (Status <> FileUploadStatus.Uploading) Then
            Status = FileUploadStatus.Removed
        End If
    End Sub

    Public Sub UploadFileEx()
        Try
            Status = FileUploadStatus.Uploading
            Dim temp As Long = FileLength - BytesUploaded

            Dim ub As New UriBuilder(UploadUrl)
            Dim complete As Boolean = (temp <= 0)
            ub.Query = "filename=" & File.Name & "&StartByte=" & BytesUploaded & "&Complete=" & complete & "&FileGUID=" & FileGUID & "&First=false"

            Dim webrequest As HttpWebRequest = System.Net.WebRequest.Create(ub.Uri)
            webrequest.Method = "POST"
            webrequest.BeginGetRequestStream(AddressOf WriteCallback, webrequest)
        Catch ex As Exception
            Message(ex.ToString)
        End Try
    End Sub

    Private Sub WriteCallback(ByVal asynchronousResult As IAsyncResult)
        Try
            Dim webrequest As HttpWebRequest = asynchronousResult.AsyncState
            '// End the operation.
            Dim requestStream As Stream = webrequest.EndGetRequestStream(asynchronousResult)

            Dim buffer() As Byte
            ReDim buffer(4096)
            Dim bytesRead As Integer = 0
            Dim tempTotal As Integer = 0

            Dim fileStream As Stream = File.OpenRead()

            '//using (FileStream fileStream = File.OpenRead())
            '//{
            fileStream.Position = BytesUploaded
            bytesRead = fileStream.Read(buffer, 0, buffer.Length)
            While (bytesRead <> 0 AndAlso tempTotal + bytesRead < ChunkSize AndAlso Not cancel)

                requestStream.Write(buffer, 0, bytesRead)
                requestStream.Flush()
                BytesUploaded += bytesRead
                tempTotal += bytesRead


                Dim percent2 As Integer = ((CDbl(BytesUploaded) / CDbl(FileLength)) * 100)
                Dim args2 As New UploadProgressChangedEventArgs(percent2, bytesRead, BytesUploaded, FileLength, File.Name)
                'ProgressChanged(args2)
                'Message("BytesUploaded=" & BytesUploaded)

                bytesRead = fileStream.Read(buffer, 0, buffer.Length)
            End While


            fileStream.Close()
            requestStream.Close()

            Dim percent As Integer = ((CDbl(BytesUploaded) / CDbl(FileLength)) * 100)
            Dim args As New UploadProgressChangedEventArgs(percent, bytesRead, BytesUploaded, FileLength, File.Name)
            WebCalled(args)
            webrequest.BeginGetResponse(AddressOf ReadCallback, webrequest)

        Catch ex As Exception
            Message(ex.ToString)
        End Try
    End Sub
    Private Sub ReadCallback(ByVal asynchronousResult As IAsyncResult)
        Try
            Dim webrequest As HttpWebRequest = asynchronousResult.AsyncState
            Dim response As HttpWebResponse = webrequest.EndGetResponse(asynchronousResult)
            Dim reader As New StreamReader(response.GetResponseStream())

            Dim responsestring = reader.ReadToEnd()
            reader.Close()


            responsestring = ProcessReturn(responsestring)


            If (cancel) Then

                If (remove) Then
                    Status = FileUploadStatus.Removed
                Else
                    Status = FileUploadStatus.Canceled
                End If
            ElseIf (BytesUploaded < FileLength) Then
                UploadFileEx()
            Else

                Status = FileUploadStatus.Complete
            End If

        Catch ex As Exception
            Message(ex.ToString)
        End Try
    End Sub




End Class

