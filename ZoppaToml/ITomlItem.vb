Option Strict On
Option Explicit On

''' <summary>項目を表すインターフェイスです。</summary>
Public Interface ITomlItem

    ReadOnly Property GetValueType() As Type

    ReadOnly Property GetValueType(index As Integer) As Type

    ReadOnly Property Length() As Integer

    Function GetValue(Of T)() As T

    Function GetValue(Of T)(index As Integer) As T

    Default ReadOnly Property Item(index As String) As ITomlItem

End Interface
