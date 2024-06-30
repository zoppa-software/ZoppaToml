Option Strict On
Option Explicit On

''' <summary>Toml値を表す抽象クラスです。</summary>
Public MustInherit Class TomlValue
    Implements ITomlElement

    ' 生値参照
    Private mRange As RawSource.Range

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="rng">生値範囲。</param>
    Protected Sub New(rng As RawSource.Range)
        Me.mRange = rng
    End Sub

    ''' <summary>値の型を取得します。</summary>
    ''' <returns>値の型。</returns>
    Public ReadOnly Property GetValueType As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetObject().GetType()
        End Get
    End Property

    ''' <summary>値の型を取得します。</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値の型。</returns>
    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlElement.GetValueType
        Get
            Throw New NotSupportedException(GetMessage("E020"))
        End Get
    End Property

    ''' <summary>値の数を取得します。</summary>
    ''' <returns>値の数。</returns>
    Public ReadOnly Property Length As Integer Implements ITomlElement.Length
        Get
            Return 1
        End Get
    End Property

    ''' <summary>要素を参照します。</summary>
    ''' <param name="keyNames">要素名。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(keyName As String) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotSupportedException(GetMessage("E021"))
        End Get
    End Property

    ''' <summary>要素を参照します（配列）</summary>
    ''' <param name="index">インデックス。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(index As Integer) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotSupportedException(GetMessage("E020"))
        End Get
    End Property

    ''' <summary>値を参照します。</summary>
    ''' <returns>値。</returns>
    Public MustOverride Function GetObject() As Object

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列表現。</returns>
    Public Overrides Function ToString() As String
        Return Me.mRange.ToString()
    End Function

    ''' <summary>値を取得します。</summary>
    ''' <returns>値。</returns>
    Public Function [Get]() As Object Implements ITomlElement.Get
        Return Me.GetObject()
    End Function

    ''' <summary>値を取得します。</summary>
    ''' <typeparam name="T">型。</typeparam>
    ''' <returns>値。</returns>
    Public Function GetValue(Of T)() As T Implements ITomlElement.GetValue
        Return CType(Me.GetObject(), T)
    End Function

    ''' <summary>値を取得します。</summary>
    ''' <typeparam name="T">型。</typeparam>
    ''' <param name="index">インデックス。</param>
    ''' <returns>値。</returns>
    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlElement.GetValue
        Throw New NotSupportedException(GetMessage("E020"))
    End Function

    ''' <summary>列挙子を取得します。</summary>
    ''' <returns>列挙子。</returns>
    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Dim res As New List(Of ITomlElement) From {
            Me
        }
        Return New TomlCollectionEnumerator(Of ITomlElement)(res)
    End Function

End Class

''' <summary>Toml値を表すジェネリッククラスです。</summary>
Friend Class TomlValue(Of T)
    Inherits TomlValue

    ''' <summary>値を取得します。</summary>
    Public ReadOnly Property Value As T

    ''' <summary>コンストラクタ。</summary>
    ''' <param name="rng">生値範囲。</param>
    ''' <param name="value">値。</param>
    Public Sub New(rng As RawSource.Range, value As T)
        MyBase.New(rng)
        Me.Value = value
    End Sub

    ''' <summary>値を取得します。</summary>
    ''' <returns>値。</returns>
    Public Overrides Function GetObject() As Object
        Return Me.Value
    End Function

End Class


