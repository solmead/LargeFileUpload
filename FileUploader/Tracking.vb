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
Public Class Tracking
    Property startTime As Date = Now
    Property lastTime As Date = startTime
    Property currentSpeed As Double = 0.0
    Property averageSpeed As Double = 0.0
    Property movingAverageSpeed As Double = 0.0
    Property movingAverageHistory As New List(Of Double)
    Property timeRemaining As Double = 0.0
    Property timeElapsed As Double = 0.0
    Property percentUploaded As Double = 0.0
    Property bytesUploaded As Long = 0
    Property TotalBytes As Long = 0

    Property Name As String = ""
    Property ID As String = ""

    Property Message As String = ""
End Class
