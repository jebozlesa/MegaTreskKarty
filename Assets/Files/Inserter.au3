#include <File.au3>
#include <Array.au3>

Local $sFilePath = "CardDatabaseAlbum.txt"
Local $sOutputFile = "insert_commands.txt"

; Nacítajte obsah súboru do pola
Local $aFileContent
If Not _FileReadToArray($sFilePath, $aFileContent) Then
    MsgBox(16, "Error", "Failed to read file: " & $sFilePath)
    Exit
EndIf

; Otvorte výstupný súbor pre zápis
Local $hOutputFile = FileOpen($sOutputFile, 2)
If $hOutputFile = -1 Then
    MsgBox(16, "Error", "Failed to open output file: " & $sOutputFile)
    Exit
EndIf

; Prejdite riadkami a generujte príkazy INSERT
For $i = 1 To UBound($aFileContent) - 1
    Local $sLine = $aFileContent[$i]
    Local $aValues = StringSplit($sLine, ",")

    ; Preskocte riadok, ak nemá správny pocet hodnôt
    If $aValues[0] <> 14 Then ContinueLoop

    ; Vytvorte INSERT príkaz
    Local $sInsertCmd = "INSERT INTO CardDatabase (StyleID, PersonName, Health, Strength, Speed, Attack, Defense, Knowledge, Charisma, Picture, Color, Attack1, Attack2, Attack3, Attack4) VALUES (" & _
        $i & ", '" & _
        $aValues[1] & "', " & _
        $aValues[2] & ", " & _
        $aValues[3] & ", " & _
        $aValues[4] & ", " & _
        $aValues[5] & ", " & _
        $aValues[6] & ", " & _
        $aValues[7] & ", " & _
        $aValues[8] & ", '" & _
        $aValues[9] & "', '" & _
        $aValues[10] & "', " & _
        $aValues[11] & ", " & _
        $aValues[12] & ", " & _
        $aValues[13] & ", " & _
        $aValues[14] & ")"

    ; Vypíšte INSERT príkaz na konzolu
    ConsoleWrite($sInsertCmd & @CRLF)

    ; Zapisujte INSERT príkaz do výstupného súboru
    FileWriteLine($hOutputFile, $sInsertCmd)
Next

; Zatvorte výstupný súbor
FileClose($hOutputFile)

; Oznámte, že generovanie príkazov INSERT je dokoncené
MsgBox(0, "Info", "INSERT commands generated in " & $sOutputFile)
