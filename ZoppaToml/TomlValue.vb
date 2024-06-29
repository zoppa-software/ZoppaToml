Option Strict On
Option Explicit On

''' <summary>Toml値を表す抽象クラスです。</summary>
Public MustInherit Class TomlValue
    Implements ITomlElement

    ' 生値参照
    Private mRange As RawSource.Range

    Protected Sub New(rng As RawSource.Range)
        Me.mRange = rng
    End Sub

    Public ReadOnly Property GetValueType As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetObject().GetType()
        End Get
    End Property

    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetObject().GetType()
        End Get
    End Property

    Public ReadOnly Property Length As Integer Implements ITomlElement.Length
        Get
            Return 0
        End Get
    End Property

    ''' <summary>要素を参照します。</summary>
    ''' <param name="keyNames">要素名。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(keyName As String) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotSupportedException("値は名前参照できません")
        End Get
    End Property

    Default Public ReadOnly Property Items(index As Integer) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    Public MustOverride Function GetObject() As Object

    ''' <summary>文字列表現を取得します。</summary>
    ''' <returns>文字列表現。</returns>
    Public Overrides Function ToString() As String
        Return Me.mRange.ToString()
    End Function

    Public Function [Get]() As Object Implements ITomlElement.Get
        Return Me.GetObject()
    End Function

    Public Function GetValue(Of T)() As T Implements ITomlElement.GetValue
        Return CType(Me.GetObject(), T)
    End Function

    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlElement.GetValue
        Return CType(Me.GetObject(), T)
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

Friend Class TomlValue(Of T)
    Inherits TomlValue

    Public ReadOnly Value As T

    Public Sub New(rng As RawSource.Range, value As T)
        MyBase.New(rng)
        Me.Value = value
    End Sub

    Public Overrides Function GetObject() As Object
        Return Me.Value
    End Function

End Class


