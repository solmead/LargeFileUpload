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

Imports System.Windows.Browser

<ScriptableType()>
Public Class UploadInfo
    'Property currentSpeed = 0
    'Property averageSpeed = 0
    'Property movingAverageSpeed = 0
    'Property timeRemaining = 0
    'Property timeElapsed = 0
    'Property percentUploaded = 0
    'Property sizeUploaded = 0


    Property Tracking As New Tracking

    Property moving_average_history_size As Integer = 100

    Property File As System.IO.FileInfo

    Property ID As String = ""

    Property Message As String = ""

    Public Function updateTracking(ByVal BytesUploaded As Long) As Tracking
        Dim Pos = 1
        Try

            ' BytesUploaded = BytesUploaded Or Tracking.bytesUploaded Or 0
            Pos = 2
            If (BytesUploaded < 0) Then
                BytesUploaded = 0
            End If
            If (BytesUploaded > File.Length) Then
                BytesUploaded = File.Length
            End If
            Pos = 3
            Dim tickTime = Now

            ' Get time and deltas
            Dim lastTime = Tracking.lastTime
            Pos = 4
            Dim deltaTime = Now.Subtract(lastTime).TotalMilliseconds
            Pos = 5
            Dim deltaBytes = BytesUploaded - Tracking.bytesUploaded
            Pos = 6
            If (deltaBytes = 0 OrElse deltaTime = 0) Then
                Return Tracking
            End If
            Pos = 7
            ' Update tracking object
            Tracking.lastTime = Now
            Pos = 8
            Tracking.bytesUploaded = BytesUploaded
            Pos = 9
            ' Calculate speeds
            Tracking.currentSpeed = (deltaBytes * 8) / (deltaTime / 1000)
            Pos = 10
            Tracking.averageSpeed = (Tracking.bytesUploaded * 8) / ((Now.Subtract(Tracking.startTime).TotalMilliseconds) / 1000)
            Pos = 11
            ' Calculate moving average
            Tracking.movingAverageHistory.Add(Tracking.currentSpeed)
            Pos = 12
            If (Tracking.movingAverageHistory.Count > moving_average_history_size) Then
                Tracking.movingAverageHistory.RemoveAt(0)
            End If
            Pos = 13
            Tracking.movingAverageSpeed = calculateMovingAverage(Tracking.movingAverageHistory)
            Pos = 14
            ' Update times
            Tracking.timeRemaining = (File.Length - Tracking.bytesUploaded) * 8 / Tracking.movingAverageSpeed
            Pos = 15
            Tracking.timeElapsed = (Now.Subtract(Tracking.startTime).TotalMilliseconds) / 1000
            Pos = 16
            ' Update percent
            Tracking.percentUploaded = (Tracking.bytesUploaded / File.Length * 100)
            Pos = 17
            Tracking.TotalBytes = File.Length
            Tracking.Name = File.Name
            Pos = 18
            Return Tracking
        Catch ex As Exception
            MessageBox.Show("UpdateInfo:" & Pos & ", - " & ex.ToString)
        End Try
        Return Nothing
    End Function

    Private Function calculateMovingAverage(ByVal history As List(Of Double)) As Double
        Dim Pos = 1
        Try

            Dim vals As New List(Of Double)
            Dim size = 0
            Dim sum = 0.0
            Dim mean = 0.0
            Dim varianceTemp = 0.0
            Dim variance = 0.0
            Dim standardDev = 0.0
            Dim i = 0
            Dim mSum = 0.0
            Dim mCount = 0
            Pos = 2
            size = history.Count
            Pos = 3
            ' Check for sufficient data
            If (size >= 8) Then
                ' Clone the array and Calculate sum of the values 
                Pos = 4
                For i = 0 To size - 1
                    vals.Add(history(i))
                    sum += vals(i)
                Next
                Pos = 5
                mean = sum / size
                Pos = 6
                ' Calculate variance for the set
                For i = 0 To size - 1
                    varianceTemp += (vals(i) - mean) ^ 2
                Next
                Pos = 7
                variance = varianceTemp / size
                Pos = 8
                standardDev = Math.Sqrt(variance)
                Pos = 9
                'Standardize the Data
                For i = 0 To size - 1
                    vals(i) = (vals(i) - mean) / standardDev
                Next
                Pos = 10
                ' Calculate the average excluding outliers
                Dim deviationRange = 2.0
                Pos = 11
                For i = 0 To size - 1

                    If (vals(i) <= deviationRange AndAlso vals(i) >= -deviationRange) Then
                        mCount += 1
                        mSum += history(i)
                    End If
                Next
                Pos = 12
            Else
                Pos = 13
                ' Calculate the average (not enough data points to remove outliers)
                mCount = size
                Pos = 14
                For i = 0 To size - 1
                    mSum += history(i)
                Next
                Pos = 15
            End If
            Pos = 16
            Return mSum / mCount
        Catch ex As Exception

            MessageBox.Show("calculateMovingAverage:" & Pos & ", - " & ex.ToString)
        End Try
        Return 0
    End Function
End Class
