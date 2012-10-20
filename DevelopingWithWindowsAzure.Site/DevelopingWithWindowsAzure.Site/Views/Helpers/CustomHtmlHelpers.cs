using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DevelopingWithWindowsAzure.Site.Views.Helpers
{
	public class GridColumn<T> where T : class
	{
		public string Header { get; set; }
		public Func<T, MvcHtmlString> Value { get; set; }
	}
	public static class CustomHtmlHelpers
	{
		#region RenderGrid
		public static MvcHtmlString RenderGrid<TModel>(this HtmlHelper html, IEnumerable<TModel> items, 
			List<string> propertyNamesToIgnore = null,
			List<GridColumn<TModel>> extraColumns = null,
			Func<TModel, string> detailsAction = null, 
			Func<TModel, string> deleteAction = null) where TModel : class
		{
			var url = new UrlHelper(html.ViewContext.RequestContext);
			var propertiesToDisplay = GetDisplayProperties<TModel>(propertyNamesToIgnore);
			var sb = new StringBuilder();

			sb.AppendLine("<table>");

			sb.AppendLine("<thead>");
			sb.AppendLine("<tr>");
			foreach(var property in propertiesToDisplay)
				sb.AppendFormat("<th>{0}</th>{1}", property.Name, Environment.NewLine);
			if (extraColumns != null)
			{
				foreach (var column in extraColumns)
					sb.AppendFormat("<th>{0}</th>{1}", column.Header, Environment.NewLine);
			}
			if (detailsAction != null | deleteAction != null)
				sb.AppendLine("<th></th>");
			sb.AppendLine("</tr>");
			sb.AppendLine("</thead>");

			sb.AppendLine("<tbody>");
			foreach (var item in items)
			{
				sb.AppendLine("<tr>");
				foreach (var property in propertiesToDisplay)
					sb.AppendFormat("<td>{0}</td>{1}", property.GetValue(item, null), Environment.NewLine);
				if (extraColumns != null)
				{
					foreach (var column in extraColumns)
						sb.AppendFormat("<td>{0}</td>{1}", column.Value(item), Environment.NewLine);
				}
				if (detailsAction != null | deleteAction != null)
				{
					sb.Append("<td>");
					if (detailsAction != null)
						sb.AppendFormat("<a href=\"{0}\">Details</a>&nbsp;", detailsAction(item));
					if (deleteAction != null)
						sb.AppendFormat("<a href=\"{0}\">Delete</a>&nbsp;", deleteAction(item));
					sb.Append("</td>");
				}
				sb.AppendLine("</tr>");
			}
			sb.AppendLine("</tbody>");

			sb.AppendLine("</table>");

			return new MvcHtmlString(sb.ToString());
		}
		private static List<PropertyInfo> GetDisplayProperties<T>(List<string> propertyNamesToIgnore = null)
		{
			var properties = typeof(T).GetProperties();
			var propertiesToDisplay = new List<System.Reflection.PropertyInfo>();
			foreach (var property in properties)
			{
				if (propertyNamesToIgnore != null)
				{
					if (propertyNamesToIgnore.Contains(property.Name))
						continue;
				}
				if (property.PropertyType.IsValueType || property.PropertyType == typeof(string))
					propertiesToDisplay.Add(property);
			}
			return propertiesToDisplay;
		}
		#endregion
		#region RenderSubMenu
		public static MvcHtmlString RenderSubMenu(this HtmlHelper html, string controllerName)
		{
			var url = new UrlHelper(html.ViewContext.RequestContext);
			var sb = new StringBuilder();
			switch (controllerName)
			{
				case "MediaServices":
					sb.AppendLine("<ul>");
					sb.AppendLine(GetSubMenuItem(url, "Assets", "Assets", controllerName));
					sb.AppendLine(GetSubMenuItem(url, "Content Keys", "ContentKeys", controllerName));
					sb.AppendLine(GetSubMenuItem(url, "Files", "Files", controllerName));
					sb.AppendLine(GetSubMenuItem(url, "Jobs", "Jobs", controllerName));
					sb.AppendLine(GetSubMenuItem(url, "Locators", "Locators", controllerName));
					sb.AppendLine(GetSubMenuItem(url, "Media Processors", "MediaProcessors", controllerName));
					sb.AppendLine("</ul>");
					break;
				default:
					throw new ApplicationException("Unexpected controller name: " + controllerName);
			}
			return new MvcHtmlString(sb.ToString());
		}
		private static string GetSubMenuItem(UrlHelper url, string linkText, string action, string controllerName)
		{
			return string.Format("<li><a href=\"{0}\">{1}</a></li>", url.Action(action, controllerName), linkText);
		}
		#endregion
	}
}