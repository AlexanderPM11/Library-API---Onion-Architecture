using System.Collections.Generic;
using LibraryAPI.Application.Interfaces;

namespace LibraryAPI.Application.Services
{
    public class TemplateEngineService : ITemplateEngineService
    {
        public string RenderTemplate(string templateContent, Dictionary<string, string> variables)
        {
            if (string.IsNullOrWhiteSpace(templateContent))
                return string.Empty;

            var rendered = templateContent;

            // Inyectar variables globales por defecto
            rendered = System.Text.RegularExpressions.Regex.Replace(
                rendered, 
                @"\{\{\s*CurrentYear\s*\}\}", 
                System.DateTime.UtcNow.Year.ToString(), 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            if (variables == null || variables.Count == 0)
                return rendered;
            
            foreach (var kvp in variables)
            {
                // Reemplaza de forma Case-Insensitive y tolerando posibles espacios {{ Key }}
                var pattern = @"\{\{\s*" + System.Text.RegularExpressions.Regex.Escape(kvp.Key) + @"\s*\}\}";
                rendered = System.Text.RegularExpressions.Regex.Replace(
                    rendered, 
                    pattern, 
                    match => kvp.Value ?? string.Empty, 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return rendered;
        }
    }
}
