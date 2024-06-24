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

    ''' <summary>新しいインスタンスを生成します。</summary>
    ''' <param name="bytes">生のデータ。</param>
    Public Sub New(bytes As Byte())
        Me.Raw = bytes
    End Sub

    ''' <summary>ポインタを取得します。</summary>
    ''' <returns>ポインタ。</returns>
    Public Function GetPointer() As Pointer
        Return New Pointer(Me)
    End Function

    ''' <summary>ポインタを取得します。</summary>
    ''' <param name="index">最初のインデックス。</param>
    ''' <returns>ポインタ。</returns>
    Public Function GetPointer(index As Integer) As Pointer
        Return New Pointer(Me, index)
    End Function

    ''' <summary>範囲を取得します。</summary>
    ''' <param name="start">開始インデックス。</param>
    ''' <param name="end">終了インデックス。</param>
    ''' <returns>範囲。</returns>
    Friend Function GetRange(start As Integer, [end] As Integer) As Range
        Return New Range(Me, start, [end])
    End Function

#Region "inner class"

    ''' <summary>ポインタを表現します。</summary>
    Public NotInheritable Class Pointer

        ' 生値参照
        Private ReadOnly mRaw As RawSource

        ' インデックス
        Private mIndex As Integer

        ''' <summary>現在位置の一文字分のバイト範囲を取得します。</summary>
        ''' <returns>バイト範囲。</returns>
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
                    Throw New InvalidOperationException("有効な UTF8文字ではありません")
                End If
            End Get
        End Property

        ''' <summary>現在位置のインデックスを取得します。</summary>
        ''' <returns>インデックス。</returns>
        Public ReadOnly Property Index As Integer
            Get
                Return Me.mIndex
            End Get
        End Property

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="src">生値参照。</param>
        Public Sub New(src As RawSource)
            Me.mRaw = src
            Me.mIndex = 0
        End Sub

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="raw">生値参照。</param>
        ''' <param name="index">インデックス。</param>
        Public Sub New(raw As RawSource, index As Integer)
            Me.mRaw = raw
            Me.mIndex = index
        End Sub

        ''' <summary>終端を超えたかどうかを取得します。</summary>
        ''' <returns>終端を超えた場合は true。</returns>
        Public Function IsEnd() As Boolean
            Return Me.mIndex >= Me.mRaw.Raw.Length
        End Function

        ''' <summary>現在のバイトとスキップするバイト数を取得します。</summary>
        ''' <returns>バイト。</returns>
        Public Function GetCurrentByteAndSkip() As (curByte As Byte, skip As Integer)
            Dim b = Me.mRaw.Raw(Me.mIndex)
            If (b And &H80) = 0 Then
                Return (b, 1)
            ElseIf (b And &HE0) = &HC0 Then
                Return (b, 2)
            ElseIf (b And &HF0) = &HE0 Then
                Return (b, 3)
            ElseIf (b And &HF8) = &HF0 Then
                Return (b, 4)
            ElseIf (b And &HFC) = &HF8 Then
                Return (b, 5)
            ElseIf (b And &HFE) = &HFC Then
                Return (b, 6)
            Else
                Throw New InvalidOperationException("有効な UTF8文字ではありません")
            End If
        End Function

        ''' <summary>次の文字へ進みます。</summary>
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
                Throw New InvalidOperationException("有効な UTF8文字ではありません")
            End If
        End Sub

        ''' <summary>指定したバイト数だけスキップします。</summary>
        ''' <param name="count">スキップする数。</param>
        Public Sub [Skip](count As Integer)
            Me.mIndex += count
        End Sub

        ''' <summary>指定したインデックスのバイトを取得します。</summary>
        ''' <param name="index">インデックス。</param>
        ''' <returns>バイト。</returns>
        Public Function Peek(index As Integer) As Byte
            Return If(Me.mIndex + index < Me.mRaw.Length, Me.mRaw.Raw(Me.mIndex + index), CByte(0))
        End Function

        ''' <summary>指定した文字数の文字列を取得します。</summary>
        ''' <param name="charCount">文字数数。</param>
        ''' <param name="isEllipsis">省略記号を付加するかどうか。</param>
        ''' <returns>文字列。</returns>
        Public Function TakeChar(charCount As Integer, Optional isEllipsis As Boolean = True) As String
            Dim oldIndex = Me.mIndex

            ' 指定文字数分、ポインタを進める
            Dim i As Integer = 0
            Do While i < charCount - 1 AndAlso Not Me.IsEnd()
                i += 1
                Me.Next()
            Loop

            ' 省略記号を付加して文字列を作成
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

    ''' <summary>範囲データを表現します。</summary>
    Public Structure Range

        ' 生値参照
        Private ReadOnly mSource As RawSource

        ''' <summary>範囲の最初のバイトのインデックスを取得します。</summary>
        ''' <returns>要素のインデックス。</returns>
        Public ReadOnly Property Start As Integer

        ''' <summary>範囲の最後のバイトのインデックスを取得します。</summary>
        ''' <returns>要素のインデックス。</returns>
        Public ReadOnly Property [End] As Integer

        ''' <summary>範囲の長さを取得します。</summary>
        ''' <returns>長さ。</returns>
        Public ReadOnly Property Length As Integer
            Get
                Return (Me.End + 1) - Me.Start
            End Get
        End Property

        ''' <summary>範囲内の生のデータを取得します。</summary>
        ''' <returns>生のデータ。</returns>
        Public ReadOnly Property Raw As Byte()
            Get
                Dim res = New Byte(Me.Length - 1) {}
                Array.Copy(Me.mSource.Raw, Me.Start, res, 0, Me.Length)
                Return res
            End Get
        End Property

        ''' <summary>範囲内のバイトを取得します。</summary>
        ''' <param name="index">インデックス。</param>
        ''' <returns>バイト。</returns>
        Default Public ReadOnly Property Items(index As Integer) As Byte
            Get
                If index >= 0 AndAlso index < Me.Length Then
                    Return Me.mSource.Raw(Me.Start + index)
                Else
                    Return 0
                End If
            End Get
        End Property

        ''' <summary>コンストラクタ。</summary>
        ''' <param name="src">生値参照。</param>
        ''' <param name="start">範囲の最初のバイトのインデックス。</param>
        ''' <param name="end">範囲の最後のバイトのインデックス。</param>
        Public Sub New(src As RawSource, start As Integer, [end] As Integer)
            Me.mSource = src
            Me.Start = start
            Me.[End] = [end]
        End Sub

        ''' <summary>範囲内のバイト列を文字列に変換します。</summary>
        ''' <returns>文字列。</returns>
        Public Overrides Function ToString() As String
            Return Text.Encoding.UTF8.GetString(Me.mSource.Raw, Me.Start, Me.Length)
        End Function

    End Structure

#End Region

End Class
