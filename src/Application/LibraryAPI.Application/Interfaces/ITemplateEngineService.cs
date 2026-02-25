using System.Collections.Generic;

namespace LibraryAPI.Application.Interfaces
{
    public interface ITemplateEngineService
    {
        string RenderTemplate(string templateContent, Dictionary<string, string> variables);
    }
}
