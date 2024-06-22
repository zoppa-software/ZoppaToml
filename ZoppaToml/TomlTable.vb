Option Strict On
Option Explicit On

''' <summary>テーブルを表すクラスです。</summary>
Public NotInheritable Class TomlTable
    Implements ITomlItem

    Public ReadOnly Property Name As String

    Public ReadOnly Property Children As New SortedList(Of String, ITomlItem)()

    Public ReadOnly Property GetValueType As Type Implements ITomlItem.GetValueType
        Get
            Return Me.GetType()
        End Get
    End Property

    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlItem.GetValueType
        Get
            Return Me.GetValueType()
        End Get
    End Property

    Public ReadOnly Property Length As Integer Implements ITomlItem.Length
        Get
            Return Me.Children.Count
        End Get
    End Property

    Default Public ReadOnly Property Item(index As String) As ITomlItem Implements ITomlItem.Item
        Get
            If Me.Children.ContainsKey(index) Then
                Dim res = TryCast(Me.Children(index), ITomlItem)
                If res IsNot Nothing Then
                    Return res
                End If
            End If
            Throw New KeyNotFoundException()
        End Get
    End Property

    Public Sub New(name As String)
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Me.Name
    End Function

    Public Function GetValue(Of T)() As T Implements ITomlItem.GetValue
        Return DirectCast(DirectCast(Me, Object), T)
    End Function

    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlItem.GetValue
        Return Me.GetValue(Of T)()
    End Function

End Class
