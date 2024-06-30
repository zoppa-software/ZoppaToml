Option Strict On
Option Explicit On

''' <summary>配列を表すクラスです。</summary>
Public NotInheritable Class TomlArray
    Implements ITomlElement

    ' 生値参照
    Private mRange As RawSource.Range

    ' 配列の要素
    Private ReadOnly mItems As New List(Of ITomlElement)()

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
                Throw New IndexOutOfRangeException(GetMessage("E002"))
            End If
        End Get
    End Property

    ''' <summary>要素を参照します。</summary>
    ''' <param name="keyNames">要素名。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(keyName As String) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotSupportedException(GetMessage("E003"))
        End Get
    End Property

    ''' <summary>要素を参照します（配列）</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(index As Integer) As ITomlElement Implements ITomlElement.Items
        Get
            If index >= 0 AndAlso index < Me.mItems.Count Then
                Return Me.mItems(index)
            Else
                Throw New IndexOutOfRangeException(GetMessage("E002"))
            End If
        End Get
    End Property

    ''' <summary>値の数を取得します。</summary>
    ''' <returns>値の数。</returns>
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

    ''' <summary>要素を追加します。</summary>
    ''' <param name="tomlValue">要素。</param>
    Friend Sub Add(tomlValue As ITomlElement)
        Me.mItems.Add(tomlValue)
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
            Throw New IndexOutOfRangeException(GetMessage("E027"))
        End If
    End Function

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列表現。</returns>
    Public Overrides Function ToString() As String
        Return Me.mRange.ToString()
    End Function

    ''' <summary>列挙子を取得します。</summary>
    ''' <returns>列挙子。</returns>
    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Return New TomlCollectionEnumerator(Of ITomlElement)(Me.mItems)
    End Function

End Class
