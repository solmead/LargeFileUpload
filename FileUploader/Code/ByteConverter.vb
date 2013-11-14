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

Imports System.Windows.Data

'using System;
'using System.Net;
'using System.Windows;
'using System.Windows.Controls;
'using System.Windows.Documents;
'using System.Windows.Ink;
'using System.Windows.Input;
'using System.Windows.Media;
'using System.Windows.Media.Animation;
'using System.Windows.Shapes;
'using System.Windows.Data;




Public Class ByteConverter
    Implements IValueConverter



    Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert

        Dim size As String = "0 KB"

        If (value IsNot Nothing) Then
            Dim byteCount As Long = value

            If (byteCount >= 1073741824) Then
                size = String.Format("{0:##.##}", byteCount / 1073741824) + " GB"
            ElseIf (byteCount >= 1048576) Then
                size = String.Format("{0:##.##}", byteCount / 1048576) + " MB"
            ElseIf (byteCount >= 1024) Then
                size = String.Format("{0:##.##}", byteCount / 1024) + " KB"
            ElseIf (byteCount > 0 AndAlso byteCount < 1024) Then
                size = "1 KB"
            End If
        End If

        Return size
    End Function

    ' //We only use one-way binding, so we don't implement this.
    Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function

End Class


