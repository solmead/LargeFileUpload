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




Public Delegate Sub ProgressChangedEvent(ByVal sender As Object, ByVal args As UploadProgressChangedEventArgs)
Public Class UploadProgressChangedEventArgs
    Public Property ProgressPercentage As Integer
    Public Property BytesUploaded As Long
    Public Property TotalBytesUploaded As Long
    Public Property TotalBytes As Long
    Public Property FileName As String

    Public Sub New()

    End Sub

    Public Sub New(ByVal progressPercentage As Integer, ByVal bytesUploaded As Long, ByVal totalBytesUploaded As Long, ByVal totalBytes As Long, ByVal fileName As String)

        Me.ProgressPercentage = progressPercentage
        Me.BytesUploaded = bytesUploaded
        Me.TotalBytes = totalBytes
        Me.FileName = fileName
        Me.TotalBytesUploaded = totalBytesUploaded
    End Sub
End Class

