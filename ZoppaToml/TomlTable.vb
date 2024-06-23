Option Strict On
Option Explicit On

''' <summary>テーブルを表すクラスです。</summary>
Public NotInheritable Class TomlTable
    Implements ITomlElement

    Public ReadOnly Property Name As String

    Public ReadOnly Property Children As New SortedList(Of String, ITomlElement)()

    Public ReadOnly Property GetValueType As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetType()
        End Get
    End Property

    Public ReadOnly Property GetValueType(index As Integer) As Type Implements ITomlElement.GetValueType
        Get
            Return Me.GetValueType()
        End Get
    End Property

    Public ReadOnly Property Length As Integer Implements ITomlElement.Length
        Get
            Return Me.Children.Count
        End Get
    End Property

    Default Public ReadOnly Property Items(index As String) As ITomlElement Implements ITomlElement.Items
        Get
            If Me.Children.ContainsKey(index) Then
                Dim res = TryCast(Me.Children(index), ITomlElement)
                If res IsNot Nothing Then
                    Return res
                End If
            End If
            Throw New KeyNotFoundException()
        End Get
    End Property

    Default Public ReadOnly Property Items(index As Integer) As ITomlElement Implements ITomlElement.Items
        Get
            Throw New NotImplementedException()
        End Get
    End Property


    Public Sub New(name As String)
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Me.Name
    End Function

    Public Function [Get]() As Object Implements ITomlElement.Get
        Return Me
    End Function

    Public Function GetValue(Of T)() As T Implements ITomlElement.GetValue
        Return DirectCast(DirectCast(Me, Object), T)
    End Function

    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlElement.GetValue
        Return Me.GetValue(Of T)()
    End Function

End Class
