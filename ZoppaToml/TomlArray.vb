Option Strict On
Option Explicit On

Public NotInheritable Class TomlArray
    Implements ITomlItem

    ' 生値参照
    Private mRange As RawSource.Range

    Private ReadOnly mItems As New List(Of ITomlItem)()

    Public Sub New(rng As RawSource.Range)
        Me.mRange = rng
    End Sub

    Public ReadOnly Property GetValueType As Type Implements ITomlItem.GetValueType
        Get
            Return Me.GetType()
        End Get
    End Property

    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlItem.GetValueType
        Get
            If index >= 0 AndAlso index < Me.mItems.Count Then
                Return Me.mItems(index).GetValueType()
            Else
                Throw New IndexOutOfRangeException()
            End If
        End Get
    End Property

    Default Public ReadOnly Property Item(index As String) As ITomlItem Implements ITomlItem.Item
        Get
            Throw New NotSupportedException()
        End Get
    End Property

    Private ReadOnly Property Length As Integer Implements ITomlItem.Length
        Get
            Return Me.mItems.Count
        End Get
    End Property

    Public Sub Add(tomlValue As ITomlItem)
        Me.mItems.Add(tomlValue)
    End Sub

    Public Function GetValue(Of T)() As T Implements ITomlItem.GetValue
        Return DirectCast(DirectCast(Me, Object), T)
    End Function

    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlItem.GetValue
        If index >= 0 AndAlso index < Me.mItems.Count Then
            Return Me.mItems(index).GetValue(Of T)()
        Else
            Throw New IndexOutOfRangeException()
        End If
    End Function

End Class
