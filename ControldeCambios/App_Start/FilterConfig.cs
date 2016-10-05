using System.Web;
using System.Web.Mvc;
using ControldeCambios.App_Start;

namespace ControldeCambios
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new MessagesActionFilter());
        }
    }
}
