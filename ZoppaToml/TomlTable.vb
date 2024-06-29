Option Strict On
Option Explicit On

Imports System.Dynamic

''' <summary>テーブルを表すクラスです。</summary>
Public NotInheritable Class TomlTable
    Inherits DynamicObject
    Implements ITomlElement

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

    ''' <summary>要素を参照します。</summary>
    ''' <param name="keyName">要素名。</param>
    ''' <returns>要素。</returns>
    Default Public ReadOnly Property Items(keyName As String) As ITomlElement Implements ITomlElement.Items
        Get
            If Me.Children.ContainsKey(keyName) Then
                Dim res = TryCast(Me.Children(keyName), ITomlElement)
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

    Public Function [Get]() As Object Implements ITomlElement.Get
        Return Me
    End Function

    Public Function GetValue(Of T)() As T Implements ITomlElement.GetValue
        Return DirectCast(DirectCast(Me, Object), T)
    End Function

    Public Function GetValue(Of T)(index As Integer) As T Implements ITomlElement.GetValue
        Return Me.GetValue(Of T)()
    End Function

    ''' <summary>.で結合したキー名から要素を取得します。</summary>
    ''' <param name="keyNames">キー名。</param>
    ''' <returns>要素。</returns>
    Public Function GetByKeyNames(keyNames As String) As ITomlElement
        Dim keynm = LexicalKey(keyNames)

        Dim current As TomlTable = Me
        For i As Integer = 0 To keynm.Count - 2
            Dim key = keynm(i).GetKeyString()
            If current.Children.ContainsKey(key) Then
                Dim res = TryCast(current.Children(key), TomlTable)
                If res IsNot Nothing Then
                    current = res
                Else
                    Throw New KeyNotFoundException()
                End If
            Else
                Throw New KeyNotFoundException()
            End If
        Next

        Dim lastKey = keynm(keynm.Count - 1).GetKeyString()
        If current.Children.ContainsKey(lastKey) Then
            Dim res = TryCast(current.Children(lastKey), ITomlElement)
            If res IsNot Nothing Then
                Return res
            End If
        End If
        Throw New KeyNotFoundException()
    End Function

    ''' <summary>列挙子を取得します。</summary>
    ''' <returns>列挙子。</returns>
    Public Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Dim res As New List(Of ITomlElement)(Me.Children.Values)
        Return New TomlCollectionEnumerator(Of ITomlElement)(res)
    End Function

    ''' <summary>動的メンバーを取得します。</summary>
    ''' <param name="binder">バインダー。</param>
    ''' <param name="result">取得値。</param>
    ''' <returns>取得出来たら真。</returns>
    Public Overrides Function TryGetMember(binder As GetMemberBinder, ByRef result As Object) As Boolean
        If Me.Children.ContainsKey(binder.Name) Then
            result = Me.Children(binder.Name)
            Return True
        Else
            Return False
        End If
    End Function

End Class
