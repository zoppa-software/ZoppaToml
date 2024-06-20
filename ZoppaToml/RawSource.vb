Option Strict On
Option Explicit On

''' <summary>文字列の生値を表します。</summary>

Public NotInheritable Class RawSource

#Region "properties"

    ''' <summary>生のデータを取得します。</summary>
    ''' <returns>生のデータ。</returns>
    Public ReadOnly Property Raw As Byte()

    ''' <summary>生のデータの長さを取得します。</summary>
    ''' <returns>長さ。</returns>
    Public ReadOnly Property Length As Integer
        Get
            Return Me.Raw.Length
        End Get
    End Property

#End Region

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="src">生のデータ。</param>
    Public Sub New(src As String)
        Me.Raw = Text.Encoding.UTF8.GetBytes(src)
    End Sub


    Public Function GetPointer() As Pointer
        Return New Pointer(Me)
    End Function

    Public Function GetPointer(index As Integer) As Pointer
        Return New Pointer(Me, index)
    End Function

    Friend Function GetRange(start As Integer, [end] As Integer) As Range
        Return New Range(Me, start, [end])
    End Function

#Region "inner class"

    Public NotInheritable Class Pointer

        Private ReadOnly mRaw As RawSource

        Private mIndex As Integer

        Public ReadOnly Property Current As Range
            Get
                Dim b = Me.mRaw.Raw(Me.mIndex)
                If (b And &H80) = 0 Then
                    Return New Range(Me.mRaw, Me.mIndex, Me.mIndex)
                ElseIf (b And &HE0) = &HC0 Then
                    Return New Range(Me.mRaw, Me.mIndex, Me.mIndex + 1)
                ElseIf (b And &HF0) = &HE0 Then
                    Return New Range(Me.mRaw, Me.mIndex, Me.mIndex + 2)
                ElseIf (b And &HF8) = &HF0 Then
                    Return New Range(Me.mRaw, Me.mIndex, Me.mIndex + 3)
                ElseIf (b And &HFC) = &HF8 Then
                    Return New Range(Me.mRaw, Me.mIndex, Me.mIndex + 4)
                ElseIf (b And &HFE) = &HFC Then
                    Return New Range(Me.mRaw, Me.mIndex, Me.mIndex + 5)
                Else
                    Throw New InvalidOperationException()
                End If
            End Get
        End Property

        Public ReadOnly Property Index As Integer
            Get
                Return Me.mIndex
            End Get
        End Property

        Public Sub New(src As RawSource)
            Me.mRaw = src
            Me.mIndex = 0
        End Sub

        Public Sub New(raw As RawSource, index As Integer)
            Me.mRaw = raw
            Me.mIndex = index
        End Sub

        Public Function IsEnd() As Boolean
            Return Me.mIndex >= Me.mRaw.Raw.Length
        End Function

        Public Sub [Next]()
            Dim b = Me.mRaw.Raw(Me.mIndex)
            If (b And &H80) = 0 Then
                Me.mIndex += 1
            ElseIf (b And &HE0) = &HC0 Then
                Me.mIndex += 2
            ElseIf (b And &HF0) = &HE0 Then
                Me.mIndex += 3
            ElseIf (b And &HF8) = &HF0 Then
                Me.mIndex += 4
            ElseIf (b And &HFC) = &HF8 Then
                Me.mIndex += 5
            ElseIf (b And &HFE) = &HFC Then
                Me.mIndex += 6
            Else
                Throw New InvalidOperationException()
            End If
        End Sub

        Public Sub [Skip](count As Integer)
            Me.mIndex += count
        End Sub

        Public Function Peek(index As Integer) As Byte
            Return If(Me.mIndex + index < Me.mRaw.Length, Me.mRaw.Raw(Me.mIndex + index), CByte(0))
        End Function

        Public Function TakeChar(charCount As Integer, Optional isEllipsis As Boolean = True) As String
            Dim oldIndex = Me.mIndex
            Dim i As Integer = 0
            Do While i < charCount - 1 AndAlso Not Me.IsEnd()
                i += 1
                Me.Next()
            Loop

            Dim ans = ""
            If Me.IsEnd Then
                ans = Me.mRaw.GetRange(oldIndex, Me.mRaw.Length - 1).ToString()
            Else
                ans = Me.mRaw.GetRange(oldIndex, Me.mIndex).ToString() & "..."
            End If

            Me.mIndex = oldIndex
            Return ans
        End Function

    End Class

    Public Structure Range

        Private ReadOnly mSource As RawSource

        Public ReadOnly Property Start As Integer

        Public ReadOnly Property [End] As Integer

        Public ReadOnly Property Length As Integer
            Get
                Return (Me.End + 1) - Me.Start
            End Get
        End Property

        Public ReadOnly Property Raw As Byte()
            Get
                Dim res = New Byte(Me.Length - 1) {}
                Array.Copy(Me.mSource.Raw, Me.Start, res, 0, Me.Length)
                Return res
            End Get
        End Property

        Default Public ReadOnly Property Items(index As Integer) As Byte
            Get
                If index >= 0 AndAlso index < Me.Length Then
                    Return Me.mSource.Raw(Me.Start + index)
                Else
                    Throw New IndexOutOfRangeException()
                End If
            End Get
        End Property

        Public Sub New(src As RawSource, start As Integer, [end] As Integer)
            Me.mSource = src
            Me.Start = start
            Me.[End] = [end]
        End Sub

        Public Overrides Function ToString() As String
            Return Text.Encoding.UTF8.GetString(Me.mSource.Raw, Me.Start, Me.Length)
        End Function

    End Structure

#End Region

End Class
