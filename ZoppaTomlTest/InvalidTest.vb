﻿Imports System
Imports Xunit
Imports ZoppaToml
Imports ZoppaToml.TomlLexical

Public Class InvalidTest

    <Fact>
    Sub ArrayTest()
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\double-comma-1.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\double-comma-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\extend-defined-aot.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\extending-table.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\missing-separator-1.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\missing-separator-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-1.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-3.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-4.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-5.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-6.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-7.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-8.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-table-1.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-close-table-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-comma-1.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-comma-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\no-comma-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\only-comma-1.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\only-comma-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\tables-1.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\tables-2.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\text-after-array-entries.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\text-before-array-separator.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\array\text-in-array.toml")
        )
    End Sub

    <Fact>
    Sub BoolTest()
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\almost-false-with-extra.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\almost-false.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\almost-true-with-extra.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\almost-true.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\capitalized-false.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\capitalized-true.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\just-f.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\just-t.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\mixed-case-false.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\mixed-case-true.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\mixed-case.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\starting-same-false.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\starting-same-true.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\wrong-case-false.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\bool\wrong-case-true.toml")
        )
    End Sub

    <Fact>
    Sub DateTimeTest()
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\feb-29.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\feb-30.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\hour-over.toml")
        )

        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\mday-over.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\mday-under.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\minute-over.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\month-over.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\month-under.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\no-leads-month.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\no-leads-with-milli.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\no-leads.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\no-t.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\offset-overflow-hour.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\offset-overflow-minute.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\second-over.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\time-no-leads.toml")
        )
        Assert.Throws(Of TomlSyntaxException)(
            Sub() TomlDocument.LoadFromFile("invalid\datetime\y10k.toml")
        )
    End Sub

End Class
