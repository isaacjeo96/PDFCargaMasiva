using BL;
using ML;
using PdfSharp.Pdf.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PL.Controllers
{
    public class RAdministrativaController : Controller
    {
        private const string BaseDirectory = @"~/Documentos"; // Carpeta base en la solución

        // GET: RAdministrativa        
        public ActionResult CargaMasiva()
        {
            return View(new Result { Objects = new List<object>() });
        }

        /// <summary>
        /// Procesa una lista de archivos PDF y los divide en archivos más pequeños
        /// después de un número específico de páginas.
        /// </summary>
        /// <param name="files">Lista de archivos subidos.</param>
        /// <param name="splitAfterPage">Número de páginas después del cual se divide el PDF.</param>
        /// <returns>Vista con los resultados de los archivos divididos o una vista de error.</returns>
        [HttpPost]
        public async Task<ActionResult> ProcessAndSplitPdfDirectory(IEnumerable<HttpPostedFileBase> files, int splitAfterPage)
        {
            var result = new Result { Correct = true, Objects = new List<object>() };

            if (files == null || !files.Any())
            {
                result.Correct = false;
                result.Message = "No se seleccionaron archivos o carpeta vacía.";
                return View("CargaMasiva", result);
            }

            string destDir = Server.MapPath(BaseDirectory);

            try
            {
                foreach (var file in files)
                {
                    string filePath = Path.Combine(destDir, file.FileName);

                    // Asegurarse de que el directorio existe
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    // Guardar el archivo subido
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.InputStream.CopyToAsync(fileStream);
                    }

                    // Procesar y dividir el archivo PDF
                    var processedFile = ProcessAndSplitFile(filePath, splitAfterPage);
                    result.Objects.Add(processedFile);
                }
            }
            catch (Exception ex)
            {
                result.Correct = false;
                result.Message = ex.Message;
                result.Ex = ex;
                return View("CargaMasiva", result);
            }

            return View("CargaMasiva", result);
        }

        private ProcessedFile ProcessAndSplitFile(string filePath, int splitAfterPage)
        {
            var processedFile = new ProcessedFile
            {
                FileName = Path.GetFileName(filePath)
            };

            List<string> splitFiles = null;

            // Intentar el método principal de división
            try
            {
                splitFiles = BL.RAdministrativas.SplitPdfUsingPdfSharp(filePath, splitAfterPage);
            }
            catch (Exception ex)
            {
                // Loggear el error (opcional)
                System.Diagnostics.Debug.WriteLine($"Error usando el método principal: {ex.Message}");

                // Intentar el método de respaldo
                try
                {
                    splitFiles = BL.RAdministrativas.SplitPdfUsingPdfPig(filePath, splitAfterPage);
                }
                catch (Exception backupEx)
                {
                    // Loggear el error del método de respaldo (opcional)
                    System.Diagnostics.Debug.WriteLine($"Error usando el método de respaldo: {backupEx.Message}");
                    processedFile.IsDivided = false;
                    return processedFile;
                }
            }

            processedFile.IsDivided = splitFiles != null && splitFiles.Any();
            if (processedFile.IsDivided)
            {
                // Contar el número de páginas en el archivo original
                using (var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import))
                {
                    processedFile.NumberOfPages = inputDocument.PageCount;
                }

                // Mover los archivos divididos al directorio de destino
                foreach (string splitFile in splitFiles)
                {
                    string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileName(splitFile));
                    System.IO.File.Move(splitFile, newFilePath);
                }
            }

            return processedFile;
        }
    }
}
