#include <GUIConstantsEx.au3>
#include <GuiListView.au3>
#include <WindowsConstants.au3>
#include <Array.au3>
#include <File.au3>

HotKeySet("{END}", "KillScript")

; ====== TVOJ KÓD (nemením) ======

Global $saveDir = "C:\Zlozka\MegaTreskKarty\karty_obrazky\gimpkarty"
Global $exportDir = "C:\Zlozka\MegaTreskKarty\karty_obrazky\Nový priecinok"

; Koordináty (x,y)
Global $posWidthNew[2]  = [103, 128]
Global $posHeightNew[2] = [103, 168]
Global $posOK[2]        = [262, 287]

; ====== KOORDINÁTY PRE DIALÓG "Zmenit mierku vrstvy..." ======
Global $posLayerScaleWidth[2]  = [1150, 673] ; pole Šírka
Global $posLayerScaleHeight[2] = [1150, 707] ; pole Výška
Global $posLayerScaleBtn[2]    = [1349, 864] ; tlacidlo "Zmenit mierku" / "Scale"
Global $posLayerScaleLock[2]   = [1273, 690] ; ikona viazaného pomeru strán (retaz)

; ====== KOORDINÁTY PRE DIALÓG "Upravit atribúty vrstvy..." (Ctrl+Shift+U) ======
Global $posAttrX[2]  = [1118, 806] ; pole posun X
Global $posAttrY[2]  = [1110, 838] ; pole posun Y
Global $posAttrOK[2] = [1448, 984] ; tlacidlo OK

; ====== KOORDINÁTY (DOPLN) ======
Global $posNewLayerOK[2]     = [1451, 993] ; OK v dialógu "Nová vrstva"
Global $posOpacityField[2]   = [2080, 432] ; pole/slider Opacity v paneli vrstiev (klikni do císel)
; ====== KOORDINÁTY PRE DIALÓG "New Layer" (Ctrl+Shift+N) ======
Global $posNewLayerOpacity[2] = [1313, 754] ; Krytie
Global $posNewLayerWidth[2]   = [1107, 791] ; Šírka
Global $posNewLayerHeight[2]  = [1102, 824] ; Výška
Global $posNewLayerX[2]       = [1105, 866] ; Posun osi X
Global $posNewLayerY[2]       = [1101, 897] ; Posun osi Y
Global $posNewLayerOK[2]      = [1450, 995] ; OK

Opt("WinTitleMatchMode", 2)

; 1) Aktivovat GIMP
WinActivate("GIMP")
WinWaitActive("GIMP", "", 2)

; 2) Otvorit "New Image" dialóg
Send("^n")
Sleep(2000)

; 3) Zadat šírku
MouseClick("left", $posWidthNew[0], $posWidthNew[1])
Send("^a")
Send(528)
Sleep(100)

; 4) Zadat výšku
MouseClick("left", $posHeightNew[0], $posHeightNew[1])
Send("^a")
Send(768)
Sleep(100)

; 5) Kliknút OK
MouseClick("left", $posOK[0], $posOK[1])

Sleep(800) ; nech sa GIMP stihne prepnut z dialógu na plátno

Local $imgPostava = PickOnePngFromDownloads("Vyber PNG - POSTAVA")
OpenAsLayer($imgPostava)
Sleep(1000)
ScaleActiveLayer(464, 464, 0) ; postava: pomer neriešim (netoggle)
SetLayerOffset(32, 40)

Local $imgPozadie = PickOnePngFromDownloads("Vyber PNG - POZADIE")
OpenAsLayer($imgPozadie)
Sleep(1000)
ScaleActiveLayer(528, 768, 1)

CreateLayerRect(32, 540, 496, 740, 70)
CreateLayerRect(32,  40, 496, 760, 70)

AlternativesLoop()

SaveCardXCF()



; ====== FUNKCIE ======

Func ExportFrontBack($baseName)
    ; FRONT
    MsgBox(64, "Export - predna strana", "Nastav spravne vrstvy pre PREDNU stranu a klikni OK.")
    _ExportToPath($exportDir & "\" & $baseName)
    Sleep(2000)

    ; BACK
    MsgBox(64, "Export - zadna strana", "Nastav spravne vrstvy pre ZADNU stranu a klikni OK.")
    _ExportToPath($exportDir & "\" & $baseName & "_back")
    Sleep(2000)
EndFunc

Func _ExportToPath($fullPath)
    ; fokus na GIMP, inak sa skratky nemusia poslat do správneho okna
    WinActivate("GIMP")
    WinWaitActive("GIMP", "", 2)
    Sleep(400)

    ; Export As
    Send("^+e")
    Sleep(2200)

    ; Location -> vlož cestu
    Send("^l")
    Sleep(600)
    ClipPut($fullPath)
    Send("^v")
    Sleep(400)
    Send("{ENTER}")
    Sleep(2500)

    ; potvrdenie exportu (ak vyskocí ešte jedno okno s nastaveniami exportu)
    Send("{ENTER}")
    Sleep(2000)
EndFunc

Func _SanitizeFileName($s)
    $s = StringStripWS($s, 3)
    $s = StringRegExpReplace($s, '[\\\/\:\*\?\"\<\>\|]', "_")
    Return $s
EndFunc

Func SaveCardXCF()
    Local $name = InputBox("Ulozenie karty", "Zadaj nazov karty (bez pripony):")
    If @error Then Return ; Cancel

    $name = _SanitizeFileName($name)
    If $name = "" Then Return

    Local $full = $saveDir & "\" & $name

    ; Save As
    Send("^+s")
    Sleep(2200)

    ; Location -> vloz plnu cestu
    Send("^l")
    Sleep(600)
    ClipPut($full)
    Send("^v")
    Sleep(400)
    Send("{ENTER}")
    Sleep(2500)

    ; hned po ulozeni spusti export front/back pod rovnakym nazvom
    ExportFrontBack($name)
EndFunc


Func AskAddAlternatives()
    Local $h = GUICreate("Alternatívy", 360, 140, -1, -1)
    GUICtrlCreateLabel("Chceš pridat alternatívy?", 12, 12, 330, 20)

    Local $bBoth = GUICtrlCreateButton("Áno (obe)", 12, 45, 100, 30)
    Local $bChar = GUICtrlCreateButton("Postava", 125, 45, 100, 30)
    Local $bBg   = GUICtrlCreateButton("Pozadie", 238, 45, 100, 30)
    Local $bNo   = GUICtrlCreateButton("Nie", 238, 85, 100, 30)

    GUISetState(@SW_SHOW, $h)

    While 1
        Switch GUIGetMsg()
            Case $GUI_EVENT_CLOSE, $bNo
                GUIDelete($h)
                Return 0
            Case $bBoth
                GUIDelete($h)
                Return 1
            Case $bChar
                GUIDelete($h)
                Return 2
            Case $bBg
                GUIDelete($h)
                Return 3
        EndSwitch
        Sleep(30)
    WEnd
EndFunc

Func AddPostavaAlternative()
    Local $img = PickOnePngFromDownloads("Vyber obrázok - POSTAVA (alt)")
    OpenAsLayer($img)
    Sleep(1200)
    ScaleActiveLayer(464, 464, 0)
    Sleep(900)
    SetLayerOffset(32, 40)
    Sleep(900)
EndFunc

Func AddPozadieAlternative()
    Local $img = PickOnePngFromDownloads("Vyber obrázok - POZADIE (alt)")
    OpenAsLayer($img)
    Sleep(1200)
    ScaleActiveLayer(528, 768, 1) ; vypne viazaný pomer (toggle)
    Sleep(900)
EndFunc

Func AlternativesLoop()
    While 1
        Local $c = AskAddAlternatives()
        Switch $c
            Case 0
                ExitLoop
            Case 1
                AddPostavaAlternative()
                AddPozadieAlternative()
            Case 2
                AddPostavaAlternative()
            Case 3
                AddPozadieAlternative()
        EndSwitch
    WEnd
EndFunc

; ====== PARAMETRICKÁ VRSTVA: BIELA OVERLAY S NEPRIEHLADNOSTOU A OBDELNÍKOM ======
Func CreateLayerRect($x1, $y1, $x2, $y2, $opacity)
    Local $w = $x2 - $x1
    Local $h = $y2 - $y1

    ; 1) Otvor "New Layer"
    Send("^+n")
    Sleep(1500)

    ; 2) Krytie
    MouseClick("left", $posNewLayerOpacity[0], $posNewLayerOpacity[1], 1, 0)
    Sleep(300)
    Send("^a")
    Send($opacity)
    Sleep(500)

    ; 3) Šírka
    MouseClick("left", $posNewLayerWidth[0], $posNewLayerWidth[1], 1, 0)
    Sleep(300)
    Send("^a")
    Send($w)
    Sleep(500)

    ; 4) Výška
    MouseClick("left", $posNewLayerHeight[0], $posNewLayerHeight[1], 1, 0)
    Sleep(300)
    Send("^a")
    Send($h)
    Sleep(500)

    ; 5) Posun X
    MouseClick("left", $posNewLayerX[0], $posNewLayerX[1], 1, 0)
    Sleep(300)
    Send("^a")
    Send($x1)
    Sleep(500)

    ; 6) Posun Y
    MouseClick("left", $posNewLayerY[0], $posNewLayerY[1], 1, 0)
    Sleep(300)
    Send("^a")
    Send($y1)
    Sleep(500)

    ; 7) OK
    MouseClick("left", $posNewLayerOK[0], $posNewLayerOK[1], 1, 0)
    Sleep(1500)
EndFunc

Func SetLayerOffset($x, $y)
    ; otvor dialog "Upravit atribúty vrstvy..."
    Send("^+u")
    Sleep(900)

    ; X
    MouseClick("left", $posAttrX[0], $posAttrX[1], 1, 0)
    Sleep(200)
    Send("^a")
    Send($x)
    Sleep(300)

    ; Y
    MouseClick("left", $posAttrY[0], $posAttrY[1], 1, 0)
    Sleep(200)
    Send("^a")
    Send($y)
    Sleep(300)

    ; OK
    MouseClick("left", $posAttrOK[0], $posAttrOK[1], 1, 0)
    Sleep(900)
EndFunc


Func ScaleActiveLayer($w, $h, $toggleLock)
    ; 1) Otvorit dialóg "Zmenit mierku vrstvy..." (Ctrl+Shift+M)
    Send("^+m")
    Sleep(2000)

    ; 2) Volitelne prepnút viazaný pomer strán (klik = toggle)
    If $toggleLock Then
        MouseClick("left", $posLayerScaleLock[0], $posLayerScaleLock[1], 1, 0)
        Sleep(500)
    EndIf

    ; 3) Zadat šírku
    MouseClick("left", $posLayerScaleWidth[0], $posLayerScaleWidth[1], 1, 0)
    Send("^a")
    Send($w)
    Sleep(500)

    ; 4) Zadat výšku
    MouseClick("left", $posLayerScaleHeight[0], $posLayerScaleHeight[1], 1, 0)
    Send("^a")
    Send($h)
    Sleep(500)

    ; 5) Kliknút na "Zmenit mierku"
    MouseClick("left", $posLayerScaleBtn[0], $posLayerScaleBtn[1], 1, 0)
    Sleep(600)
EndFunc


Func PickOnePngFromDownloads($title)
    Local $sFolder = @UserProfileDir & "\Downloads"

    ; povolené formáty
    Local $reExt = "(?i)\.(png|jpg|jpeg|bmp|gif|tif|tiff|webp)$"

    ; aData[row][0]=filename, [1]=timeSort(YYYYMMDDHHMMSS), [2]=fullpath
    Local $aData[0][3]
    Local $count = 0

    Local $hSearch = FileFindFirstFile($sFolder & "\*.*")
    If $hSearch = -1 Then
        MsgBox(16, "Chyba", "Neviem cítat Downloads.")
        Exit
    EndIf

    While 1
        Local $f = FileFindNextFile($hSearch)
        If @error Then ExitLoop

        Local $full = $sFolder & "\" & $f

        ; preskoc priecinky
        If StringInStr(FileGetAttrib($full), "D") Then ContinueLoop

        ; len obrázky podla prípony
        If Not StringRegExp($f, $reExt) Then ContinueLoop

        ReDim $aData[$count + 1][3]
        $aData[$count][0] = $f
        $aData[$count][1] = FileGetTime($full, 0, 1) ; YYYYMMDDHHMMSS
        $aData[$count][2] = $full
        $count += 1
    WEnd
    FileClose($hSearch)

    If $count = 0 Then
        MsgBox(16, "Chyba", "V Downloads nie sú žiadne obrázky (png/jpg/jpeg/bmp/gif/tif/tiff/webp).")
        Exit
    EndIf

    ; najnovšie hore
    _ArraySort($aData, 1, 0, 0, 1)

    Local $hGUI = GUICreate($title, 760, 520, -1, -1)
    Local $idLV = GUICtrlCreateListView("Súbor|Cas", 10, 10, 740, 440, BitOR($LVS_REPORT, $LVS_SHOWSELALWAYS))
    Local $hLV = GUICtrlGetHandle($idLV)

    _GUICtrlListView_SetExtendedListViewStyle($hLV, BitOR($LVS_EX_FULLROWSELECT, $LVS_EX_GRIDLINES))
    _GUICtrlListView_SetColumnWidth($hLV, 0, 500)
    _GUICtrlListView_SetColumnWidth($hLV, 1, 200)

    Local $i
    For $i = 0 To UBound($aData) - 1
        GUICtrlCreateListViewItem($aData[$i][0] & "|" & $aData[$i][1], $idLV)
    Next

    Local $idOK = GUICtrlCreateButton("OK", 560, 460, 90, 30)
    Local $idCancel = GUICtrlCreateButton("Cancel", 660, 460, 90, 30)

    GUISetState(@SW_SHOW, $hGUI)

    While 1
        Switch GUIGetMsg()
            Case $GUI_EVENT_CLOSE, $idCancel
                GUIDelete($hGUI)
                Exit

            Case $idOK
                Local $aSel = _GUICtrlListView_GetSelectedIndices($hLV, True)
                If @error Or $aSel[0] <> 1 Then
                    MsgBox(48, "Info", "Vyber presne 1 obrázok.")
                    ContinueLoop
                EndIf

                Local $idx = $aSel[1]
                Local $path = $aData[$idx][2]

                GUIDelete($hGUI)
                Return $path
        EndSwitch
    WEnd
EndFunc


Func OpenAsLayer($fullPath)
    ; Open as Layers
    Send("^!o")
    Sleep(400)

    ; Location
    Send("^l")
    Sleep(100)

    ClipPut($fullPath)
    Send("^v")
    Send("{ENTER}")
    Sleep(300)

    ; Otvorit
    Send("{ENTER}")

    ; pockaj, kým sa file dialog zavrie (aby další import nepadol na nacasovanie)
    Sleep(1200)
EndFunc


; ====== END = okamžite ukonci skript ======

Func KillScript()
    Exit
EndFunc