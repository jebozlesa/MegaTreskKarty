#include <File.au3>
#include <Array.au3>

Local $sFilePath = "CardDatabaseAlbum.txt"
Local $sOutputFile = "insert_commands.txt"

; Nac�tajte obsah s�boru do pola
Local $aFileContent
If Not _FileReadToArray($sFilePath, $aFileContent) Then
    MsgBox(16, "Error", "Failed to read file: " & $sFilePath)
    Exit
EndIf

; Otvorte v�stupn� s�bor pre z�pis
Local $hOutputFile = FileOpen($sOutputFile, 2)
If $hOutputFile = -1 Then
    MsgBox(16, "Error", "Failed to open output file: " & $sOutputFile)
    Exit
EndIf

; Prejdite riadkami a generujte pr�kazy INSERT
For $i = 1 To UBound($aFileContent) - 1
    Local $sLine = $aFileContent[$i]
    Local $aValues = StringSplit($sLine, ",")

    ; Preskocte riadok, ak nem� spr�vny pocet hodn�t
    If $aValues[0] <> 14 Then ContinueLoop

    ; Vytvorte INSERT pr�kaz
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

    ; Vyp�te INSERT pr�kaz na konzolu
    ConsoleWrite($sInsertCmd & @CRLF)

    ; Zapisujte INSERT pr�kaz do v�stupn�ho s�boru
    FileWriteLine($hOutputFile, $sInsertCmd)
Next

; Zatvorte v�stupn� s�bor
FileClose($hOutputFile)

; Ozn�mte, �e generovanie pr�kazov INSERT je dokoncen�
MsgBox(0, "Info", "INSERT commands generated in " & $sOutputFile)
