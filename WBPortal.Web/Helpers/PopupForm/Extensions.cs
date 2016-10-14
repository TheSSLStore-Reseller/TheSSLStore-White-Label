using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace WBSSLStore.Web.Helpers.PopupForm
{
    internal static class Extensions
    {
        internal static object ReadParams(this object o, object src)
        {
            var tp = TypeDescriptor.GetProperties(o);
            var sp = TypeDescriptor.GetProperties(src);

            for (var i = 0; i < tp.Count; i++)
            {
                var p = tp[i];
                if (p.Name == "HtmlAttributes") continue;

                var s = sp.Find(p.Name, true);
                var val = s.GetValue(src);
                if (val != null)
                    p.SetValue(o, val);
            }
            return o;
        }

        internal static object ReadSettings(this object o, Type type)
        {
            var sets = type.GetFields();
            var props = TypeDescriptor.GetProperties(o);

            foreach (var setting in sets)
            {
                var p = props.Find(setting.Name, true);
                if (p == null || p.GetValue(o) != null) continue;
                p.SetValue(o, setting.GetValue(null));
            }
            return o;
        }

        internal static void ReadMeta(this object o, HtmlHelper html)
        {
            if (html.ViewData.ModelMetadata == null) return;

            var vals = html.ViewData.ModelMetadata.AdditionalValues;

            var props = TypeDescriptor.GetProperties(o);
            for (var i = 0; i < props.Count; i++)
            {

                if (props[i].GetValue(o) != null) continue;
                if (!vals.ContainsKey(props[i].Name)) continue;
                if (props[i].Name == "Parameters")
                {
                    var ss = new JavaScriptSerializer();
                    props[i].SetValue(o, ss.DeserializeObject(vals["Parameters"].ToString()));
                }
                else
                    props[i].SetValue(o, vals[props[i].Name]);
            }
        }

        internal static string MakePropId(this string prop, string format)
        {
            if (!prop.EndsWith("]")) return string.Format(format, prop);
            var e = CopyFromTo(prop, prop.LastIndexOf('['), prop.LastIndexOf(']'));
            var n = RemoveFromTo(prop, prop.LastIndexOf('['), prop.LastIndexOf(']'));
            return string.Format(format, n) + e;
        }

        internal static string CopyFromTo(this string s, int from, int to)
        {
            return s.Substring(from, ++to - from);
        }

        internal static string RemoveFromTo(this string s, int from, int to)
        {
            return s.Remove(from, ++to - from);
        }

        internal static string DefaultController(this string controller, string prop, string postfix)
        {
            return controller ?? (prop.GetController() + postfix);
        }

        internal static string GetController(this string s)
        {
            if (s.Contains(".")) s = CopyFromTo(s, s.LastIndexOf('.') + 1, s.Length - 1);
            if (!s.Contains("[")) return s;
            return CopyFromTo(s, 0, s.LastIndexOf('[') - 1);
        }

        internal static string JQPos(this string v)
        {
            if (string.IsNullOrWhiteSpace(v)) return "'center'";
            return (v.StartsWith("[") ? v : "'" + v + "'");
        }

        internal static string Controller(this Type t)
        {
            return t.Name.RemoveSuffix("Controller");
        }

        internal static string Name<T>(this Expression<Action<T>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method.Name.ToLower();
        }

        internal static string[] Params<T>(this Expression<Action<T>> e)
        {
            return ((MethodCallExpression)e.Body).Method.GetParameters().Select(o => o.Name).ToArray();
        }

        internal static string RemovePrefix(this string o, string prefix)
        {
            if (prefix == null) return o;
            return !o.StartsWith(prefix) ? o : o.Remove(0, prefix.Length);
        }

        internal static string RemoveSuffix(this string o, string suffix)
        {
            if (suffix == null) return o;
            return !o.EndsWith(suffix) ? o : o.Remove(o.Length - suffix.Length, suffix.Length);
        }

        internal static IEnumerable<object> GetValues<T>(this Expression<Action<T>> expression)
        {
            var call = expression.Body as MethodCallExpression;
            if (call == null)
            {
                throw new ArgumentException("Not a method call");
            }
            foreach (Expression argument in call.Arguments)
            {
                LambdaExpression lambda = Expression.Lambda(argument,
                                                            expression.Parameters);
                Delegate d = lambda.Compile();
                object value = d.DynamicInvoke(new object[1]);
                yield return value;
            }
        }

        internal static IDictionary<string, string> GetDictionary(this object source)
        {
            if (source == null) return null;
            var props = TypeDescriptor.GetProperties(source);
            var d = new Dictionary<string, string>();
            for (var i = 0; i < props.Count; i++)
            {
                d.Add(props[i].Name, props[i].GetValue(source).ToString());
            }
            return d;
        }

        internal static string ToHtml(this IDictionary<string, string> d)
        {
            if (d == null) return null;
            return d.Aggregate("", (c, v) => c + (v.Key + "=" + "'" + v.Value + "' "));
        }

        internal static IDictionary<string, string> AddVal(this IDictionary<string, string> d, string key, string value)
        {
            if (d == null) d = new Dictionary<string, string>();
            if (d.ContainsKey(key)) d[key] = d[key] + " " + value; else d.Add(key, value);
            return d;
        }


        internal static object FromModel(this object value, HtmlHelper html, string prop)
        {
            return value ?? html.ViewData.Eval(prop);
        }

        internal static string Controller(this HtmlHelper html)
        {
            return html.ViewContext.RouteData.Values["controller"].ToString();
        }

        internal static string Action(this HtmlHelper html)
        {
            return html.ViewContext.RouteData.Values["action"].ToString();
        }

        internal static string Area(this HtmlHelper html)
        {
            return html.ViewContext.RouteData.DataTokens["area"] as string;
        }
        
        internal static string Area(this UrlHelper o)
        {
            return o.RequestContext.RouteData.DataTokens["area"] as string;
        }

        internal static string Controller(this UrlHelper url)
        {
            return url.RequestContext.RouteData.Values["controller"].ToString();
        }

        internal static string Action(this UrlHelper url)
        {
            return url.RequestContext.RouteData.Values["action"].ToString();
        }
    }
}