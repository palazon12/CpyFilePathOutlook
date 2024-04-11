Imports System.IO
Imports Microsoft.Office.Interop.Outlook
Imports System.Text
Imports System.Runtime.InteropServices
Imports System.Collections.Specialized

Public Class Form1
    ' VARIABLES GLOBALES '
    Dim filteredFiles As New List(Of String)()

    Private Sub DataGridView1_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles DataGridView1.MouseDown
        ' AQUI REVISAMOS QUE EL EVENTO DEL CLICK DEL MOUSE SEA EL CLICK DERECHO '
        If e.Button = MouseButtons.Right Then
            '' OBTENEMOS EL CONTENIDO DEL PORTAPAPELES '
            Dim clipboardFiles As String() = ObtenerRutasArchivosPortapapeles()

            ' FILTRAMOS LA LISTA DE ARCHIVOS POR LAS EXTENSIONES DEFINIDAS A CONTINUACIÓN: '
            Dim allowedExtensions As String() = {".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"}

            ' BUCLE PARA AÑADIR LOS ARCHIVOS PERMITIDOS POR LAS EXTENSIONES '
            For Each file In clipboardFiles
                Dim extension = Path.GetExtension(file).ToLowerInvariant()
                If allowedExtensions.Contains(extension) Then
                    filteredFiles.Add(file)
                End If
            Next

            ' Habilitar o deshabilitar el ítem del menú contextual según si hay archivos válidos en el portapapeles '
            If filteredFiles.Count > 0 Then
                cntxtMenuStripPegar.Enabled = True
            Else
                cntxtMenuStripPegar.Enabled = False
            End If

            ' MOSTRAMOS EL CONTEXT MENU STRIP (MENÚ CONTEXTUAL) '
            cntxtMenuStripPegar.Show(DataGridView1, e.Location)
        Else
            ' Limpiar la lista de archivos filtrados si se hace clic derecho fuera del ContextMenuStrip
            filteredFiles.Clear()
        End If
    End Sub

    ' MÉTODO PARA OBTENER LAS RUTAS DE LOS ARCHIVOS EN EL PORTAPAPELES '
    Private Function ObtenerRutasArchivosPortapapeles() As String()
        ' Verificar si hay datos en el portapapeles
        If Clipboard.ContainsData("FileGroupDescriptor") Then
            Dim filePaths As New List(Of String)

            ' Obtener los datos del portapapeles
            Dim theStream As IO.MemoryStream = DirectCast(Clipboard.GetData("FileGroupDescriptor"), MemoryStream)
            Try
                Using br As New BinaryReader(theStream)
                    ' Leer el número de archivos en el portapapeles
                    Dim numFiles As Integer = br.ReadInt32()

                    ' Iterar sobre cada archivo en el portapapeles
                    For fileCounter As Integer = 1 To numFiles
                        ' Saltar al nombre del archivo en el descriptor de archivos
                        br.BaseStream.Seek(72, SeekOrigin.Current)

                        ' Leer el nombre del archivo
                        Dim fileName As String = System.Text.Encoding.ASCII.GetString(br.ReadBytes(260)).TrimEnd(Chr(0))

                        ' Construir la ruta completa del archivo
                        Dim tempDirectory As String = Path.GetTempPath()
                        Dim filePath As String = Path.Combine(tempDirectory, fileName)

                        ' Agregar la ruta del archivo a la lista
                        filePaths.Add(filePath)
                    Next
                End Using
            Finally
                ' Cerrar el MemoryStream
                If theStream IsNot Nothing Then
                    theStream.Close()
                End If
            End Try

            ' Convertir la lista de rutas de archivos a un array y devolverlo
            Return filePaths.ToArray()
        Else
            ' Devolver un array de cadenas vacío si no hay datos en el portapapeles
            Return New String() {}
        End If
    End Function

    ' EN ESTE MÉTODO TRATAREMOS EL CLICK DEL CONTEXT MENU STRIP Y LE DIREMOS QUE HACER '
    Private Sub cntxtMenuStripPegar_ItemClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ToolStripItemClickedEventArgs) Handles cntxtMenuStripPegar.ItemClicked
        ' Accede a la lista filteredFiles para mostrar las rutas completas de los archivos seleccionados en un MessageBox
        Dim message As String = "Archivos seleccionados:" & Environment.NewLine
        For Each file As String In filteredFiles
            message &= file & Environment.NewLine
        Next

        MessageBox.Show(message, "Archivos seleccionados", MessageBoxButtons.OK, MessageBoxIcon.Information)
        filteredFiles.Clear()
    End Sub

    ' EN ESTE MÉTODO TRATAREMOS EL CIERRE DEL CONTEXT MENU STRIP '
    Private Sub cntxtMenuStripPegar_Closed(ByVal sender As Object, ByVal e As ToolStripDropDownClosedEventArgs) Handles cntxtMenuStripPegar.Closed
        ' Limpiar la lista de archivos filtrados al cerrar el ContextMenuStrip
        filteredFiles.Clear()
    End Sub
End Class