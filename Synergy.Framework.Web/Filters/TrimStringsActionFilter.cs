using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace Synergy.Framework.Web.Filters;

internal class TrimStringsActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Geçici bir koleksiyon oluşturuyoruz
        var updatedArguments = new Dictionary<string, object>();

        foreach (var parameter in context.ActionArguments)
        {
            // Eğer parametre bir string ise, doğrudan trim yapıyoruz
            if (parameter.Value is string stringValue)
            {
                updatedArguments[parameter.Key] = stringValue.Trim();
            }
            else
            {
                // Nesne parametreleri için String alanlarını kontrol ediyoruz
                TrimStringProperties(parameter.Value);
                updatedArguments[parameter.Key] = parameter.Value;
            }
        }

        // Güncellenmiş argümanları ActionArguments koleksiyonuna geri ekliyoruz
        foreach (var updatedArgument in updatedArguments)
        {
            context.ActionArguments[updatedArgument.Key] = updatedArgument.Value;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Yöntem çalıştıktan sonra yapılacak işlem varsa burada eklenebilir
    }

    private void TrimStringProperties(object parameter)
    {
        if (parameter == null)
            return;

        // Nesne tipinde olup olmadığını kontrol ediyoruz
        var properties = parameter.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Sadece string tipindeki özellikler üzerinde işlem yapıyoruz
            if (property.PropertyType == typeof(string))
            {
                var value = (string)property.GetValue(parameter);

                // Eğer değer boş değilse, Trim işlemi yapıyoruz
                if (value != null)
                {
                    property.SetValue(parameter, value.Trim());
                }
            }
        }
    }
}
