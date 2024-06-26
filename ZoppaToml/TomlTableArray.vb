Option Strict On
Option Explicit On

''' <summary>テーブル配列を表すクラスです。</summary>
Public NotInheritable Class TomlTableArray
    Implements ITomlElement

    ' 生値参照
    Private mRange As RawSource.Range

    ' 配列の要素
    Private ReadOnly mItems As New List(Of TomlTable)()

#Region "properties"

    ''' <summary>値の型を取得します。</summary>
    ''' <returns>値の型。</returns>
    Public ReadOnly Property GetValueType As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetType()
        End Get
    End Property

    ''' <summary>値の型を取得します。</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値の型。</returns>
    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlElement.GetValueType
        Get
            If index >= 0 AndAlso index < Me.mItems.Count Then
                Return Me.mItems(index).GetValueType()
            Else
                Throw New IndexOutOfRangeException()
            End If
        End Get
    End Property

    ''' <summary>要素を参照します。</summary>
    ''' <param name="keyNames">要素名。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(keyName As String) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotSupportedException("配列は名前参照できません。")
        End Get
    End Property

    ''' <summary>要素を参照します（配列）</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>要素。</returns>
    ''' <summary>値の数を取得します。</summary>
    ''' <returns>値の数。</returns>
    Default Public ReadOnly Property Items(index As Integer) As ITomlElement Implements ITomlElement.Items
        Get
            If index >= 0 AndAlso index < Me.mItems.Count Then
                Return Me.mItems(index)
            Else
                Throw New IndexOutOfRangeException()
            End If
        End Get
    End Property

    Private ReadOnly Property Length As Integer Implements ITomlElement.Length
        Get
            Return Me.mItems.Count
        End Get
    End Property

#End Region

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="rng">生値参照。</param>
    Public Sub New(rng As RawSource.Range)
        Me.mRange = rng
    End Sub

    ''' <summary>値を取得します。</summary>
    ''' <returns>値。</returns>
    Public Function [Get]() As Object Implements ITomlElement.Get
        Return Me
    End Function

    ''' <summary>値を取得します。</summary>
    ''' <typeparam name="T">型。</typeparam>
    ''' <returns>値。</returns>
    Public Function GetValue(Of T)() As T Implements ITomlElement.GetValue
        Return DirectCast(DirectCast(Me, Object), T)
    End Function

    ''' <summary>値を取得します。</summary>
    ''' <typeparam name="T">型。</typeparam>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値。</returns>
    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlElement.GetValue
        If index >= 0 AndAlso index < Me.mItems.Count Then
            Return Me.mItems(index).GetValue(Of T)()
        Else
            Throw New IndexOutOfRangeException("配列の範囲外を指定しています。")
        End If
    End Function

    ''' <summary>新しいテーブルを取得します。</summary>
    ''' <returns>テーブル。</returns>
    Friend Function GetNew() As TomlTable
        Dim tbl As New TomlTable("")
        Me.mItems.Add(tbl)
        Return tbl
    End Function

    ''' <summary>カレントテーブルを取得します。</summary>
    ''' <returns>テーブル。</returns>
    Friend Function GetCurrent() As TomlTable
        Return Me.mItems(Me.mItems.Count - 1)
    End Function

End Class
