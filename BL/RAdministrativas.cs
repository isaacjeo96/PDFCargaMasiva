using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ML;
using PdfSharp.Pdf.IO;
using UglyToad.PdfPig.Writer;

namespace BL
{
    public class RAdministrativas
    {
        public static List<string> SplitPdfUsingPdfSharp(string filePath, int splitAfterPage)
        {
            var splitFilePaths = new List<string>();

            // Cargar el PDF usando PDFSharp
            PdfSharp.Pdf.PdfDocument inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);

            // Obtener el número total de páginas en el PDF
            int totalPages = inputDocument.PageCount;

            // Si el PDF tiene solo una página, no dividirlo
            if (totalPages <= 1)
            {
                // Añadir la ruta original a la lista y retornar
                splitFilePaths.Add(filePath);
                return splitFilePaths;
            }

            // Obtener el directorio del archivo original
            var originalDirectory = Path.GetDirectoryName(filePath);

            // Dividir el PDF en archivos más pequeños
            for (int i = 0; i < totalPages; i += splitAfterPage)
            {
                // Crear un nuevo documento PDF
                PdfSharp.Pdf.PdfDocument outputDocument = new PdfSharp.Pdf.PdfDocument();

                // Determinar el rango de páginas para el nuevo PDF
                int endPage = (i + splitAfterPage > totalPages) ? totalPages : i + splitAfterPage;

                // Copiar las páginas al nuevo documento
                for (int j = i; j < endPage; j++)
                {
                    outputDocument.AddPage(inputDocument.Pages[j]);
                }

                // Generar una ruta para guardar el nuevo archivo PDF en el directorio original
                var splitFilePath = Path.Combine(originalDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_pagina{i / splitAfterPage + 1}.pdf");

                // Guardar el nuevo documento PDF
                outputDocument.Save(splitFilePath);

                // Añadir la ruta del archivo dividido a la lista
                splitFilePaths.Add(splitFilePath);
            }

            return splitFilePaths;
        }

        // Método de respaldo utilizando PdfPig
        public static List<string> SplitPdfUsingPdfPig(string filePath, int splitAfterPage)
        {
            var splitFilePaths = new List<string>();

            // Cargar el PDF usando PdfPig
            using (var inputDocument = UglyToad.PdfPig.PdfDocument.Open(filePath))
            {
                var totalPages = inputDocument.NumberOfPages;

                // Si el PDF tiene solo una página, no dividirlo
                if (totalPages <= 1)
                {
                    splitFilePaths.Add(filePath);
                    return splitFilePaths;
                }

                var originalDirectory = Path.GetDirectoryName(filePath);

                for (int i = 0; i < totalPages; i += splitAfterPage)
                {
                    var outputDocument = new PdfDocumentBuilder();

                    int endPage = (i + splitAfterPage > totalPages) ? totalPages : i + splitAfterPage;

                    for (int j = i; j < endPage; j++)
                    {
                        var page = inputDocument.GetPage(j + 1);
                        outputDocument.AddPage(page.Size);
                    }

                    var splitFilePath = Path.Combine(originalDirectory, $"{Path.GetFileNameWithoutExtension(filePath)}_pagina{i / splitAfterPage + 1}.pdf");

                    using (var fs = new FileStream(splitFilePath, FileMode.Create))
                    {
                        var pdfBytes = outputDocument.Build();
                        fs.Write(pdfBytes, 0, pdfBytes.Length);
                    }

                    splitFilePaths.Add(splitFilePath);
                }
            }

            return splitFilePaths;
        }
    }
}

