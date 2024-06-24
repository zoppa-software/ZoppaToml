Option Strict On
Option Explicit On

Public NotInheritable Class TomlArray
    Implements ITomlElement

    ' 生値参照
    Private mRange As RawSource.Range

    Private ReadOnly mItems As New List(Of ITomlElement)()

    Public Sub New(rng As RawSource.Range)
        Me.mRange = rng
    End Sub

    Public ReadOnly Property GetValueType As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetType()
        End Get
    End Property

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

    Public Sub Add(tomlValue As ITomlElement)
        Me.mItems.Add(tomlValue)
    End Sub

    Public Function [Get]() As Object Implements ITomlElement.Get
        Return Me
    End Function

    Public Function GetValue(Of T)() As T Implements ITomlElement.GetValue
        Return DirectCast(DirectCast(Me, Object), T)
    End Function

    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlElement.GetValue
        If index >= 0 AndAlso index < Me.mItems.Count Then
            Return Me.mItems(index).GetValue(Of T)()
        Else
            Throw New IndexOutOfRangeException()
        End If
    End Function

    Public Function GetNew() As TomlTable
        Dim tbl As New TomlTable("")
        Me.mItems.Add(tbl)
        Return tbl
    End Function

End Class
